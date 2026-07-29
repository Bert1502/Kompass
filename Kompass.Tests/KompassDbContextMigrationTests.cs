using Kompass.Persistence.Data;
using Kompass.Application.B56Import;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kompass.Tests.Persistence;

public sealed class KompassDbContextMigrationTests
{
    [Fact]
    public async Task Alle_Migrationen_koennen_auf_leere_Datenbank_angewendet_werden()
    {
        var datenbankpfad =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-migrationstest-{Guid.NewGuid():N}.db");

        try
        {
            var options =
                new DbContextOptionsBuilder<KompassDbContext>()
                    .UseSqlite(
                        $"Data Source={datenbankpfad}")
                    .Options;

            await using var context =
                new KompassDbContext(options);

            await context.Database.MigrateAsync();

            var angewendeteMigrationen =
                await context.Database
                    .GetAppliedMigrationsAsync();

            Assert.Equal(
                [
                    "20260718205341_InitialCreate",
                    "20260719104649_ProjektverwaltungErweitert",
                    "20260720073017_AddB56ImportRegister",
                    "20260724184936_PersistB56DomainResults",
                    "20260725075146_VersionB56Snapshots",
                    "20260725084054_AddB56SnapshotLifecycle",
                    "20260725085558_AddB56ProjectModelOrigin",
                    "20260725101500_TrackB56AlternativePresence",
                    "20260727160844_AddErgaenzbareProjektdaten",
                    "20260728033325_AddWirtschaftlichkeitsannahmen",
                    "20260728064802_AddFoerderprogramme",
                    "20260728070720_RefineFoerderprogrammRegeln",
                    "20260728075125_AddAlternativeFoerderungZuordnung",
                    "20260728103810_AddWaermebruecken",
                    "20260729043015_AddPersistedB56SnapshotVergleiche",
                    "20260729140029_AddProjektStammdaten",
                    "20260729152143_AddVerbrauchsDaten"
                ],
                angewendeteMigrationen);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            LoescheFallsVorhanden(datenbankpfad);
            LoescheFallsVorhanden($"{datenbankpfad}-shm");
            LoescheFallsVorhanden($"{datenbankpfad}-wal");
        }
    }

    [Fact]
    public async Task Bestehender_Import_wird_als_Legacy_Snapshot_migriert()
    {
        var datenbankpfad =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-legacy-migrationstest-{Guid.NewGuid():N}.db");

        var importId =
            Guid.NewGuid();

        try
        {
            var options =
                new DbContextOptionsBuilder<KompassDbContext>()
                    .UseSqlite(
                        $"Data Source={datenbankpfad}")
                    .Options;

            await using var context =
                new KompassDbContext(options);

            var migrator =
                context.Database.GetService<IMigrator>();

            await migrator.MigrateAsync(
                "20260724184936_PersistB56DomainResults");

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO B56ImportEintraege (
                    ImportId,
                    ProjektId,
                    Projektname,
                    Originaldateiname,
                    Archivdateipfad,
                    Sha256,
                    DateigroesseBytes,
                    ImportiertAm,
                    Dateiendung,
                    FachdatenJson)
                VALUES (
                    {importId},
                    {Guid.NewGuid()},
                    {"Legacy-Projekt"},
                    {"legacy.xlsx"},
                    {"archiv/legacy.xlsx"},
                    {new string('a', 64)},
                    {1024L},
                    {DateTimeOffset.Parse("2026-07-24T08:00:00+02:00")},
                    {".xlsx"},
                    {"{}"})
                """);

            await migrator.MigrateAsync();

            var snapshot =
                await context.B56ImportEintraege
                    .AsNoTracking()
                    .SingleAsync(
                        eintrag =>
                            eintrag.ImportId == importId);

            Assert.Equal(
                B56SnapshotVersionen.AktuelleSchemaVersion,
                snapshot.SnapshotSchemaVersion);
            Assert.Equal(
                B56SnapshotVersionen.LegacyParserVersion,
                snapshot.ParserVersion);
            Assert.Equal(
                "{}",
                snapshot.FachdatenJson);
            Assert.Equal(
                B56SnapshotStatus.TechnischGeprueft,
                snapshot.SnapshotStatus);
            Assert.Null(
                snapshot.BestaetigtAm);
            Assert.Null(
                snapshot.VerworfenAm);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            LoescheFallsVorhanden(datenbankpfad);
            LoescheFallsVorhanden($"{datenbankpfad}-shm");
            LoescheFallsVorhanden($"{datenbankpfad}-wal");
        }
    }

    private static void LoescheFallsVorhanden(
        string dateipfad)
    {
        if (File.Exists(dateipfad))
        {
            File.Delete(dateipfad);
        }
    }
}
