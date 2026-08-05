using System.Net;
using System.Net.Http.Json;
using Kompass.Domain.Funding;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kompass.Tests;

public sealed class FoerdervoraussetzungenHttpIntegrationTests
{
    [Fact]
    public async Task Voraussetzungen_koennen_ueber_Http_gespeichert_und_mit_Wpb_Vorschlag_gelesen_werden()
    {
        var verzeichnis = Path.Combine(Path.GetTempPath(), $"kompass-foerder-http-{Guid.NewGuid():N}");
        Directory.CreateDirectory(verzeichnis);
        try
        {
            await using var factory = new ApiFactory(verzeichnis);
            using var client = factory.CreateClient();
            Guid projektId;
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<KompassDbContext>();
                Assert.Empty(await db.Database.GetPendingMigrationsAsync());
                var projekt = new Projekt(Guid.NewGuid(), "Schule");
                db.Projekte.Add(projekt);
                var v = new Foerdervoraussetzungen(Guid.NewGuid(), projekt.Id);
                v.B56BestandswerteUebernehmen(1200m, 400m);
                db.Foerdervoraussetzungen.Add(v);
                await db.SaveChangesAsync();
                projektId = projekt.Id;
            }

            var route = $"/api/projekte/{projektId}/foerdervoraussetzungen";
            var antwort = await client.PutAsJsonAsync(route, new
            {
                Baujahr = 1970,
                Erstnutzung = new DateOnly(1971, 1, 1),
                Gebaeudeart = FoerderGebaeudeart.Nichtwohngebaeude,
                Nutzung = FoerderNutzung.Selbstgenutzt,
                Eigentuemart = Antragstellerart.Kommune,
                QpReferenz = 100m,
                QpReferenzQuelle = "GEG-Nachweis",
                WpbFachlichBestaetigt = true,
                Nachweise = "Energieausweis"
            });

            Assert.Equal(HttpStatusCode.OK, antwort.StatusCode);
            var gespeichert = await antwort.Content.ReadFromJsonAsync<Antwort>();
            Assert.NotNull(gespeichert);
            Assert.Equal(4m, gespeichert.WpbVerhaeltnis);
            Assert.Equal(WpbPruefstatus.RechnerischErfuellt, gespeichert.WpbRechnerischerVorschlag);
            Assert.Equal(1200m, gespeichert.Nettogrundflaeche);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(verzeichnis)) Directory.Delete(verzeichnis, true);
        }
    }

    [Fact]
    public async Task Ungueltige_Voraussetzungen_liefern_400_statt_500()
    {
        var verzeichnis = Path.Combine(Path.GetTempPath(), $"kompass-foerder-http-{Guid.NewGuid():N}");
        Directory.CreateDirectory(verzeichnis);
        try
        {
            await using var factory = new ApiFactory(verzeichnis);
            using var client = factory.CreateClient();
            Guid projektId;
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<KompassDbContext>();
                var projekt = new Projekt(Guid.NewGuid(), "Schule");
                db.Projekte.Add(projekt);
                await db.SaveChangesAsync();
                projektId = projekt.Id;
            }

            var antwort = await client.PutAsJsonAsync(
                $"/api/projekte/{projektId}/foerdervoraussetzungen",
                new { QpReferenz = 100m, QpReferenzQuelle = "" });

            Assert.Equal(HttpStatusCode.BadRequest, antwort.StatusCode);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(verzeichnis)) Directory.Delete(verzeichnis, true);
        }
    }

    private sealed record Antwort(decimal? Nettogrundflaeche, decimal? WpbVerhaeltnis, WpbPruefstatus WpbRechnerischerVorschlag);

    private sealed class ApiFactory(string verzeichnis) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.UseSetting("ConnectionStrings:KompassDatabase", $"Data Source={Path.Combine(verzeichnis, "kompass.db")}");
            builder.UseSetting("Fachdatenbanken:Verzeichnis", string.Empty);
        }
    }
}
