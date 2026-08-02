using System.Security.Cryptography;
using System.Text;
using Kompass.Application.FachdatenImport;
using Kompass.Domain.Massnahmen;
using Kompass.Domain.Materialien;
using Kompass.Domain.Funding;
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
        stammdaten += await ImportiereFoerdergeberAsync(Path.Combine(verzeichnis, "02_FoerderDB.sqlite"), cancellationToken);
        kategorien += await ImportiereMassnahmenkategorienAsync(Path.Combine(verzeichnis, "04_MassnahmenDB.sqlite"), cancellationToken);
        massnahmen += await ImportiereMassnahmenAsync(Path.Combine(verzeichnis, "04_MassnahmenDB.sqlite"), cancellationToken);
        kategorien += await ImportiereMaterialkategorienAsync(Path.Combine(verzeichnis, "05_MaterialDB.sqlite"), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new FachdatenimportErgebnis(pruefung.Datenbanken, stammdaten, kategorien, massnahmen, false);
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

    private static Guid StabileGuid(string bereich, string id)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"KOMPASS:{bereich}:{id}"));
        return new Guid(hash.AsSpan(0, 16));
    }
}
