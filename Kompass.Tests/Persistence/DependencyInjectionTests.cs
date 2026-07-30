using Kompass.Persistence;
using Kompass.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kompass.Tests.Persistence;

public sealed class DependencyInjectionTests
{
    [Fact]
    public async Task Relative_Sqlite_DataSource_wird_im_AppContext_BaseDirectory_erstellt()
    {
        var dateiname =
            $"kompass-relative-{Guid.NewGuid():N}.db";

        var datenbankpfad =
            Path.Combine(
                AppContext.BaseDirectory,
                dateiname);

        try
        {
            var configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:KompassDatabase"] =
                                $"Data Source={dateiname}"
                        })
                    .Build();

            var services =
                new ServiceCollection();

            services.AddPersistence(
                configuration);

            await using var provider =
                services.BuildServiceProvider();

            await using var scope =
                provider.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<KompassDbContext>();

            await dbContext.Database.MigrateAsync();

            Assert.True(
                File.Exists(
                    datenbankpfad));
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            LoescheFallsVorhanden(
                datenbankpfad);
            LoescheFallsVorhanden(
                $"{datenbankpfad}-shm");
            LoescheFallsVorhanden(
                $"{datenbankpfad}-wal");
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
