using System.Security.Cryptography;
using System.Text;
using Kompass.Application.FachdatenImport;
using Kompass.Domain.Massnahmen;
using Kompass.Domain.Materialien;
using Kompass.Domain.Funding;
using Kompass.Domain.Fachdaten;
using Kompass.Domain.Regelwerke;
using Kompass.Domain.Economics;
using Kompass.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Persistence.Services;

public sealed class FachdatenbankImportService : IFachdatenbankImportService
{
    private const string ErwarteteSchemaVersion = "1.0.0";

    private static readonly IReadOnlyDictionary<string, string[]> ErwarteteTabellen =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["01_RegelwerkDB.sqlite"] = ["regelwerk", "anforderung", "gebaeudekategorie", "nachweisart", "randbedingung", "temperaturkategorie"],
            ["02_FoerderDB.sqlite"] = ["programm", "foerdergeber", "foerdertatbestand", "foerderkondition", "kumulierungsregel", "nachweisanforderung"],
            ["03_WirtschaftlichkeitsDB.sqlite"] = ["energietraeger", "zeitreihe", "zeitwert", "standardannahme", "nutzungsdauer", "kostenkennwert", "betriebskostenansatz"],
            ["04_MassnahmenDB.sqlite"] = ["massnahme", "massnahmenkategorie", "massnahmenpaket", "paketposition"],
            ["05_MaterialDB.sqlite"] = ["material", "materialkategorie", "materialkennwert", "epd", "epd_modulwert"],
            ["06_ProjektDB.sqlite"] = ["projekt", "b56_import", "variante", "modernisierungsalternative", "projektmassnahme"]
        };

    private readonly KompassDbContext _dbContext;

    public FachdatenbankImportService(KompassDbContext dbContext) => _dbContext = dbContext;

    public async Task<FachdatenimportErgebnis> PruefenAsync(string verzeichnis, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(verzeichnis) || !Directory.Exists(verzeichnis))
        {
            throw new DirectoryNotFoundException("Das Fachdatenbankverzeichnis wurde nicht gefunden.");
        }

        var result = new List<FachdatenbankPruefergebnis>();
        foreach (var erwartung in ErwarteteTabellen)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(await PruefeDateiAsync(Path.Combine(verzeichnis, erwartung.Key), erwartung.Value, cancellationToken));
        }

        return new FachdatenimportErgebnis(result, 0, 0, 0, true);
    }

    public async Task<FachdatenimportErgebnis> ImportierenAsync(string verzeichnis, CancellationToken cancellationToken = default)
    {
        var pruefung = await PruefenAsync(verzeichnis, cancellationToken);
        if (!pruefung.IstGueltig)
        {
            throw new InvalidOperationException("Der Fachdatenimport wurde wegen Schemafehlern abgebrochen.");
        }

        var kategorien = 0;
        var massnahmen = 0;
        var stammdaten = 0;

        foreach (var datei in ErwarteteTabellen.Keys)
        {
            stammdaten += await ImportiereQuellenAsync(
                Path.Combine(verzeichnis, datei),
                cancellationToken);
        }

        stammdaten += await ImportiereFoerdergeberAsync(Path.Combine(verzeichnis, "02_FoerderDB.sqlite"), cancellationToken);
        kategorien += await ImportiereMassnahmenkategorienAsync(Path.Combine(verzeichnis, "04_MassnahmenDB.sqlite"), cancellationToken);
        kategorien += await ImportiereMaterialkategorienAsync(Path.Combine(verzeichnis, "05_MaterialDB.sqlite"), cancellationToken);
        stammdaten += await ImportiereRegelwerkeAsync(Path.Combine(verzeichnis, "01_RegelwerkDB.sqlite"), cancellationToken);
        stammdaten += await ImportiereFoerderprogrammeAsync(Path.Combine(verzeichnis, "02_FoerderDB.sqlite"), cancellationToken);
        massnahmen += await ImportiereMassnahmenAsync(Path.Combine(verzeichnis, "04_MassnahmenDB.sqlite"), cancellationToken);
        stammdaten += await ImportiereMaterialienAsync(Path.Combine(verzeichnis, "05_MaterialDB.sqlite"), cancellationToken);
        stammdaten += await ImportiereZeitreihenAsync(Path.Combine(verzeichnis, "03_WirtschaftlichkeitsDB.sqlite"), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new FachdatenimportErgebnis(pruefung.Datenbanken, stammdaten, kategorien, massnahmen, false);
    }

    private async Task<int> ImportiereQuellenAsync(string pfad, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        if (!await HatSpaltenAsync(connection, "data_source", ["id", "name", "source_type"], cancellationToken)) return 0;

        var count = 0;
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, source_type, reference, valid_from, valid_to, retrieved_on, checksum_sha256, notes FROM data_source;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var fachlicheId = reader.GetString(0);
            if (await _dbContext.Fachdatenquellen.AnyAsync(x => x.FachlicheId == fachlicheId, cancellationToken)) continue;
            var quelle = new Fachdatenquelle(StabileGuid("fachdatenquelle", fachlicheId), fachlicheId, reader.GetString(1), reader.GetString(2));
            quelle.Beschreiben(OptionalString(reader, 3), OptionalDate(reader, 4), OptionalDate(reader, 5), OptionalDate(reader, 6), OptionalString(reader, 7), OptionalString(reader, 8));
            _dbContext.Fachdatenquellen.Add(quelle);
            count++;
        }
        return count;
    }

    private async Task<int> ImportiereRegelwerkeAsync(string pfad, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        if (!await HatSpaltenAsync(connection, "regelwerk", ["id", "code", "titel", "fassung", "valid_from"], cancellationToken) ||
            !await HatSpaltenAsync(connection, "anforderung", ["id", "regelwerk_id", "anforderungsart", "bezeichnung", "valid_from"], cancellationToken)) return 0;

        var count = 0;
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, code, titel, fassung, valid_from, source_id FROM regelwerk;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var regelwerke = new List<(string Id, string Code, string Titel, string Fassung, DateOnly GueltigAb, string? QuelleId)>();
        while (await reader.ReadAsync(cancellationToken)) regelwerke.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), DateOnly.Parse(reader.GetString(4)), OptionalString(reader, 5)));

        foreach (var row in regelwerke)
        {
            if (await _dbContext.Regelwerke.AnyAsync(x => x.Code == row.Code && x.Version == 1, cancellationToken)) continue;
            var regelwerk = new Regelwerk(StabileGuid("regelwerk", row.Id), row.Code, 1, row.Titel, row.Fassung, row.GueltigAb, row.QuelleId is null ? null : StabileGuid("fachdatenquelle", row.QuelleId));
            await using var requirement = connection.CreateCommand();
            requirement.CommandText = "SELECT a.id, a.anforderungsart, a.bezeichnung, a.operator, a.grenzwert, a.einheit, a.textwert, a.valid_from FROM anforderung a WHERE a.regelwerk_id=$id;";
            requirement.Parameters.AddWithValue("$id", row.Id);
            await using var requirementReader = await requirement.ExecuteReaderAsync(cancellationToken);
            while (await requirementReader.ReadAsync(cancellationToken))
            {
                var grenzwert = requirementReader.IsDBNull(4) ? (decimal?)null : Convert.ToDecimal(requirementReader.GetDouble(4));
                var textwert = OptionalString(requirementReader, 6);
                if (!grenzwert.HasValue && textwert is null) continue;
                regelwerk.AnforderungHinzufuegen(new Regelwerksanforderung(StabileGuid("anforderung", requirementReader.GetString(0)), requirementReader.GetString(0), requirementReader.GetString(1), requirementReader.GetString(2), DateOnly.Parse(requirementReader.GetString(7)), grenzwert, textwert, OptionalString(requirementReader, 3), OptionalString(requirementReader, 5)));
                count++;
            }
            _dbContext.Regelwerke.Add(regelwerk);
            count++;
        }
        return count;
    }

    private async Task<int> ImportiereFoerderprogrammeAsync(string pfad, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        if (!await HatSpaltenAsync(connection, "programm", ["id", "code", "name", "valid_from"], cancellationToken)) return 0;

        var count = 0;
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.id, p.code, p.name, p.gebaeudeart, p.valid_from, p.valid_to,
                   COALESCE((SELECT MAX(k.wert) FROM foerderkondition k JOIN foerdertatbestand t ON t.id=k.foerdertatbestand_id WHERE t.programm_id=p.id AND k.konditionsart IN ('FOERDERQUOTE','TILGUNGSZUSCHUSS')),0),
                   (SELECT MAX(k.wert) FROM foerderkondition k JOIN foerdertatbestand t ON t.id=k.foerdertatbestand_id WHERE t.programm_id=p.id AND k.konditionsart='MAX_KREDIT'),
                   COALESCE((SELECT GROUP_CONCAT(t.bezeichnung, '; ') FROM foerdertatbestand t WHERE t.programm_id=p.id),p.name),
                   COALESCE((SELECT GROUP_CONCAT(n.bezeichnung, '; ') FROM nachweisanforderung n WHERE n.programm_id=p.id),'Aktuelle Programmnachweise prüfen')
            FROM programm p;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetString(0); var code = reader.GetString(1);
            if (await _dbContext.Foerderprogramme.AnyAsync(x => x.Programmkennung == code && x.Version == 1, cancellationToken)) continue;
            var programmId = StabileGuid("foerderprogramm", id);
            _dbContext.Foerderprogramme.Add(new Foerderprogramm(programmId, code, 1, DateOnly.Parse(reader.GetString(4)), OptionalDate(reader, 5), OptionalString(reader, 3) ?? "WG/NWG", reader.GetString(8), "Technische Mindestanforderungen gemäß aktueller Programmquelle prüfen.", Convert.ToDecimal(reader.GetDouble(6)), reader.IsDBNull(7) ? null : Convert.ToDecimal(reader.GetDouble(7)), "Kumulierbarkeit anhand der aktuellen Programmbedingungen prüfen.", reader.GetString(9), $"Importquelle {id}"));
            count++;
        }
        await reader.DisposeAsync();

        if (!await HatSpaltenAsync(connection, "foerdertatbestand", ["id", "programm_id", "code", "bezeichnung"], cancellationToken)) return count;
        await using var facts = connection.CreateCommand(); facts.CommandText = "SELECT id, programm_id, code, bezeichnung FROM foerdertatbestand;";
        await using var factReader = await facts.ExecuteReaderAsync(cancellationToken);
        while (await factReader.ReadAsync(cancellationToken))
        {
            var id = factReader.GetString(0); var programmId = StabileGuid("foerderprogramm", factReader.GetString(1)); var code = factReader.GetString(2);
            if (await _dbContext.Foerdertatbestaende.AnyAsync(x => x.FoerderprogrammId == programmId && x.Code == code, cancellationToken)) continue;
            _dbContext.Foerdertatbestaende.Add(new Foerdertatbestand(StabileGuid("foerdertatbestand", id), programmId, code, factReader.GetString(3))); count++;
        }
        return count;
    }

    private async Task<int> ImportiereMaterialienAsync(string pfad, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly"); await connection.OpenAsync(cancellationToken);
        if (!await HatSpaltenAsync(connection, "material", ["id", "name", "kategorie_id", "valid_from"], cancellationToken)) return 0;
        var count = 0; await using var command = connection.CreateCommand(); command.CommandText = "SELECT id, name, kategorie_id, valid_from, source_id FROM material;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetString(0); if (await _dbContext.Materialien.AnyAsync(x => x.FachlicheId == id && x.Version == 1, cancellationToken)) continue;
            var sourceId = OptionalString(reader, 4);
            _dbContext.Materialien.Add(new Material(StabileGuid("material", id), id, 1, reader.GetString(1), StabileGuid("materialkategorie", reader.GetString(2)), DateOnly.Parse(reader.GetString(3)), sourceId is null ? null : StabileGuid("fachdatenquelle", sourceId))); count++;
        }
        return count;
    }

    private async Task<int> ImportiereZeitreihenAsync(string pfad, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly"); await connection.OpenAsync(cancellationToken);
        if (!await HatSpaltenAsync(connection, "zeitreihe", ["id", "typ", "bezeichnung", "einheit", "szenario"], cancellationToken) || !await HatSpaltenAsync(connection, "zeitwert", ["zeitreihe_id", "stichtag", "wert"], cancellationToken)) return 0;
        var count = 0; await using var command = connection.CreateCommand(); command.CommandText = "SELECT id, typ, bezeichnung, einheit, szenario, source_id FROM zeitreihe;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<(string Id, string Typ, string Name, string Einheit, string Szenario, string? Quelle)>();
        while (await reader.ReadAsync(cancellationToken)) rows.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), OptionalString(reader, 5)));
        foreach (var row in rows)
        {
            if (await _dbContext.WirtschaftlicheZeitreihen.AnyAsync(x => x.FachlicheId == row.Id && x.Version == 1, cancellationToken)) continue;
            var series = new WirtschaftlicheZeitreihe(StabileGuid("zeitreihe", row.Id), row.Id, 1, row.Typ, row.Name, row.Einheit, row.Szenario, row.Quelle is null ? null : StabileGuid("fachdatenquelle", row.Quelle));
            await using var values = connection.CreateCommand(); values.CommandText = "SELECT stichtag, wert FROM zeitwert WHERE zeitreihe_id=$id;"; values.Parameters.AddWithValue("$id", row.Id);
            await using var valueReader = await values.ExecuteReaderAsync(cancellationToken);
            while (await valueReader.ReadAsync(cancellationToken)) { var date = DateOnly.Parse(valueReader.GetString(0)); series.WertHinzufuegen(StabileGuid("zeitwert", $"{row.Id}:{date:yyyy-MM-dd}"), date, Convert.ToDecimal(valueReader.GetDouble(1))); count++; }
            _dbContext.WirtschaftlicheZeitreihen.Add(series); count++;
        }
        return count;
    }

    private static async Task<FachdatenbankPruefergebnis> PruefeDateiAsync(string pfad, IReadOnlyList<string> tabellen, CancellationToken cancellationToken)
    {
        var fehler = new List<string>();
        var warnungen = new List<string>();
        var zeilen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(pfad))
        {
            fehler.Add("Datei fehlt.");
            return new FachdatenbankPruefergebnis(Path.GetFileName(pfad), string.Empty, zeilen, fehler, warnungen);
        }

        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        var integrity = await ScalarStringAsync(connection, "PRAGMA integrity_check;", cancellationToken);
        if (!string.Equals(integrity, "ok", StringComparison.OrdinalIgnoreCase)) fehler.Add($"SQLite-Integrität: {integrity}");
        var schemaVersion = await ScalarStringAsync(connection, "SELECT value FROM db_info WHERE key='schema_version';", cancellationToken);
        if (schemaVersion != ErwarteteSchemaVersion) fehler.Add($"Unerwartete Schema-Version '{schemaVersion}'.");

        foreach (var tabelle in tabellen)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$name;";
            command.Parameters.AddWithValue("$name", tabelle);
            if (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 0)
            {
                fehler.Add($"Tabelle '{tabelle}' fehlt.");
                continue;
            }

            command.Parameters.Clear();
            command.CommandText = $"SELECT COUNT(*) FROM \"{tabelle}\";";
            zeilen[tabelle] = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        }

        if (zeilen.Where(x => x.Key is not "gebaeudekategorie" and not "nachweisart" and not "randbedingung" and not "temperaturkategorie" and not "foerdergeber" and not "energietraeger" and not "massnahme" and not "massnahmenkategorie" and not "materialkategorie").All(x => x.Value == 0))
        {
            warnungen.Add("Die fachlichen Kerntabellen enthalten noch keine freigabefähigen Daten.");
        }

        return new FachdatenbankPruefergebnis(Path.GetFileName(pfad), schemaVersion, zeilen, fehler, warnungen);
    }

    private async Task<int> ImportiereMassnahmenkategorienAsync(string pfad, CancellationToken cancellationToken)
    {
        var count = 0;
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT id, code, bezeichnung FROM massnahmenkategorie;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = reader.GetString(1);
            if (await _dbContext.Massnahmenkategorien.AnyAsync(x => x.Code == code, cancellationToken)) continue;
            _dbContext.Massnahmenkategorien.Add(new Massnahmenkategorie(StabileGuid("massnahmenkategorie", reader.GetString(0)), code, reader.GetString(2))); count++;
        }
        return count;
    }

    private async Task<int> ImportiereMassnahmenAsync(string pfad, CancellationToken cancellationToken)
    {
        var count = 0;
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly"); await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT id, code, bezeichnung, kategorie_id, mengeneinheit, valid_from FROM massnahme;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = reader.GetString(1);
            if (await _dbContext.Massnahmenkatalogeintraege.AnyAsync(x => x.Code == code && x.Version == 1, cancellationToken)) continue;
            _dbContext.Massnahmenkatalogeintraege.Add(new Massnahmenkatalogeintrag(StabileGuid("massnahme", reader.GetString(0)), code, 1, reader.GetString(2), StabileGuid("massnahmenkategorie", reader.GetString(3)), reader.GetString(4), DateOnly.Parse(reader.GetString(5)))); count++;
        }
        return count;
    }

    private async Task<int> ImportiereMaterialkategorienAsync(string pfad, CancellationToken cancellationToken)
    {
        var count = 0;
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly"); await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT id, code, bezeichnung FROM materialkategorie;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = reader.GetString(1);
            if (await _dbContext.Materialkategorien.AnyAsync(x => x.Code == code, cancellationToken)) continue;
            _dbContext.Materialkategorien.Add(new Materialkategorie(StabileGuid("materialkategorie", reader.GetString(0)), code, reader.GetString(2))); count++;
        }
        return count;
    }

    private async Task<int> ImportiereFoerdergeberAsync(string pfad, CancellationToken cancellationToken)
    {
        var count = 0;
        await using var connection = new SqliteConnection($"Data Source={pfad};Mode=ReadOnly");
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, ebene FROM foerdergeber;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var fachlicheId = reader.GetString(0);
            if (await _dbContext.Foerdergeber.AnyAsync(x => x.FachlicheId == fachlicheId, cancellationToken)) continue;
            _dbContext.Foerdergeber.Add(new Foerdergeber(StabileGuid("foerdergeber", fachlicheId), fachlicheId, reader.GetString(1), reader.GetString(2)));
            count++;
        }
        return count;
    }

    private static async Task<string> ScalarStringAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand(); command.CommandText = sql;
        return Convert.ToString(await command.ExecuteScalarAsync(cancellationToken)) ?? string.Empty;
    }

    private static async Task<bool> HatSpaltenAsync(
        SqliteConnection connection,
        string tabelle,
        IReadOnlyCollection<string> spalten,
        CancellationToken cancellationToken)
    {
        var vorhanden = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tabelle}\");";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) vorhanden.Add(reader.GetString(1));
        return spalten.All(vorhanden.Contains);
    }

    private static string? OptionalString(SqliteDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) || string.IsNullOrWhiteSpace(reader.GetString(ordinal))
            ? null
            : reader.GetString(ordinal);

    private static DateOnly? OptionalDate(SqliteDataReader reader, int ordinal)
    {
        var value = OptionalString(reader, ordinal);
        return value is null ? null : DateOnly.Parse(value);
    }

    private static Guid StabileGuid(string bereich, string id)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"KOMPASS:{bereich}:{id}"));
        return new Guid(hash.AsSpan(0, 16));
    }
}
