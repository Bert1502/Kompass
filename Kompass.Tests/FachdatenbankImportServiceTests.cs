using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class FachdatenbankImportServiceTests
{
    [Fact]
    public async Task Dry_Run_prueft_alle_sechs_Datenbanken_ohne_Zieldaten_zu_aendern()
    {
        var verzeichnis = ErzeugeQuelldatenbanken();
        try
        {
            await using var context = ErzeugeContext();
            await context.Database.EnsureCreatedAsync();
            var service = new FachdatenbankImportService(context);
            var ergebnis = await service.PruefenAsync(verzeichnis);
            Assert.True(ergebnis.IstGueltig);
            Assert.True(ergebnis.DryRun);
            Assert.Equal(6, ergebnis.Datenbanken.Count);
            Assert.Empty(await context.Massnahmenkategorien.ToListAsync());
        }
        finally { SqliteConnection.ClearAllPools(); Directory.Delete(verzeichnis, true); }
    }

    [Fact]
    public async Task Import_ist_idempotent_und_uebernimmt_nur_sichere_Entwurfsdaten()
    {
        var verzeichnis = ErzeugeQuelldatenbanken();
        try
        {
            await using var context = ErzeugeContext();
            await context.Database.EnsureCreatedAsync();
            var service = new FachdatenbankImportService(context);
            var erster = await service.ImportierenAsync(verzeichnis);
            var zweiter = await service.ImportierenAsync(verzeichnis);
            Assert.Equal(3, erster.AngelegteStammdaten);
            Assert.Equal(10, erster.AngelegteKategorien);
            Assert.Equal(4, erster.AngelegteMassnahmen);
            Assert.Equal(0, zweiter.AngelegteStammdaten);
            Assert.Equal(0, zweiter.AngelegteKategorien);
            Assert.Equal(0, zweiter.AngelegteMassnahmen);
            Assert.Equal(5, await context.Massnahmenkategorien.CountAsync());
            Assert.Equal(4, await context.Massnahmenkatalogeintraege.CountAsync());
            Assert.Equal(5, await context.Materialkategorien.CountAsync());
        }
        finally { SqliteConnection.ClearAllPools(); Directory.Delete(verzeichnis, true); }
    }

    private static KompassDbContext ErzeugeContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return new KompassDbContext(new DbContextOptionsBuilder<KompassDbContext>().UseSqlite(connection).Options);
    }

    private static string ErzeugeQuelldatenbanken()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"kompass-fachdaten-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        Erzeuge(Path.Combine(dir, "01_RegelwerkDB.sqlite"), ["regelwerk", "anforderung", "gebaeudekategorie", "nachweisart", "randbedingung", "temperaturkategorie"]);
        ErzeugeFoerderung(Path.Combine(dir, "02_FoerderDB.sqlite"));
        Erzeuge(Path.Combine(dir, "03_WirtschaftlichkeitsDB.sqlite"), ["energietraeger", "zeitreihe", "zeitwert", "standardannahme", "nutzungsdauer", "kostenkennwert", "betriebskostenansatz"]);
        ErzeugeMassnahmen(Path.Combine(dir, "04_MassnahmenDB.sqlite"));
        ErzeugeMaterialien(Path.Combine(dir, "05_MaterialDB.sqlite"));
        Erzeuge(Path.Combine(dir, "06_ProjektDB.sqlite"), ["projekt", "b56_import", "variante", "modernisierungsalternative", "projektmassnahme"]);
        return dir;
    }

    private static void Erzeuge(string path, IReadOnlyList<string> tables)
    {
        using var connection = new SqliteConnection($"Data Source={path}"); connection.Open();
        Ausfuehren(connection, "CREATE TABLE db_info (key TEXT PRIMARY KEY, value TEXT NOT NULL); INSERT INTO db_info VALUES ('schema_version','1.0.0');");
        foreach (var table in tables) Ausfuehren(connection, $"CREATE TABLE \"{table}\" (id TEXT PRIMARY KEY);");
    }

    private static void ErzeugeMassnahmen(string path)
    {
        using var connection = new SqliteConnection($"Data Source={path}"); connection.Open();
        Ausfuehren(connection, "CREATE TABLE db_info (key TEXT PRIMARY KEY, value TEXT NOT NULL); INSERT INTO db_info VALUES ('schema_version','1.0.0'); CREATE TABLE massnahmenkategorie (id TEXT PRIMARY KEY, code TEXT, bezeichnung TEXT); CREATE TABLE massnahme (id TEXT PRIMARY KEY, code TEXT, bezeichnung TEXT, kategorie_id TEXT, mengeneinheit TEXT, valid_from TEXT); CREATE TABLE massnahmenpaket (id TEXT PRIMARY KEY); CREATE TABLE paketposition (id TEXT PRIMARY KEY);");
        for (var i = 1; i <= 5; i++) Ausfuehren(connection, $"INSERT INTO massnahmenkategorie VALUES ('K-{i}','K{i}','Kategorie {i}');");
        for (var i = 1; i <= 4; i++) Ausfuehren(connection, $"INSERT INTO massnahme VALUES ('M-{i}','M-{i}','Maßnahme {i}','K-1','m²','2026-08-02');");
    }

    private static void ErzeugeFoerderung(string path)
    {
        using var connection = new SqliteConnection($"Data Source={path}"); connection.Open();
        Ausfuehren(connection, "CREATE TABLE db_info (key TEXT PRIMARY KEY, value TEXT NOT NULL); INSERT INTO db_info VALUES ('schema_version','1.0.0'); CREATE TABLE programm (id TEXT PRIMARY KEY); CREATE TABLE foerdergeber (id TEXT PRIMARY KEY, name TEXT, ebene TEXT); CREATE TABLE foerdertatbestand (id TEXT PRIMARY KEY); CREATE TABLE foerderkondition (id TEXT PRIMARY KEY); CREATE TABLE kumulierungsregel (id TEXT PRIMARY KEY); CREATE TABLE nachweisanforderung (id TEXT PRIMARY KEY);");
        Ausfuehren(connection, "INSERT INTO foerdergeber VALUES ('FG-BUND','Bund','BUND'),('FG-EU','Europäische Union','EU'),('FG-LAND','Land/Region','LAND');");
    }

    private static void ErzeugeMaterialien(string path)
    {
        using var connection = new SqliteConnection($"Data Source={path}"); connection.Open();
        Ausfuehren(connection, "CREATE TABLE db_info (key TEXT PRIMARY KEY, value TEXT NOT NULL); INSERT INTO db_info VALUES ('schema_version','1.0.0'); CREATE TABLE material (id TEXT PRIMARY KEY); CREATE TABLE materialkategorie (id TEXT PRIMARY KEY, code TEXT, bezeichnung TEXT); CREATE TABLE materialkennwert (id TEXT PRIMARY KEY); CREATE TABLE epd (id TEXT PRIMARY KEY); CREATE TABLE epd_modulwert (id TEXT PRIMARY KEY);");
        for (var i = 1; i <= 5; i++) Ausfuehren(connection, $"INSERT INTO materialkategorie VALUES ('MK-{i}','MK{i}','Materialkategorie {i}');");
    }

    private static void Ausfuehren(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand(); command.CommandText = sql; command.ExecuteNonQuery();
    }
}
