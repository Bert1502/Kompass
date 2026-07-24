using Kompass.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
                    "20260724184936_PersistB56DomainResults"
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

    private static void LoescheFallsVorhanden(
        string dateipfad)
    {
        if (File.Exists(dateipfad))
        {
            File.Delete(dateipfad);
        }
    }
}
