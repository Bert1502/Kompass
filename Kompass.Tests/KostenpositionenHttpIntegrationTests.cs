using System.Net;
using System.Net.Http.Json;
using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests;

public sealed class KostenpositionenHttpIntegrationTests
{
    [Fact]
    public async Task Kostenposition_kann_ueber_Http_angelegt_abgerufen_und_geloescht_werden()
    {
        var testverzeichnis = Path.Combine(
            Path.GetTempPath(),
            $"kompass-kosten-http-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testverzeichnis);

        try
        {
            await using var factory = new KostenpositionenApiFactory(testverzeichnis);
            using var client = factory.CreateClient();
            var (projektId, alternativeId) = await ErzeugeProjektMitAlternativeAsync(factory);
            var route = $"/api/projekte/{projektId}/alternativen/{alternativeId}/kostenpositionen";

            var hinzufuegenAntwort = await client.PostAsJsonAsync(
                route,
                new
                {
                    Bezeichnung = "Fachplanung",
                    Betrag = 8_500.50m,
                    Kostenart = Kostenart.Fachplanung
                });

            Assert.Equal(HttpStatusCode.Created, hinzufuegenAntwort.StatusCode);
            Assert.Equal(route, hinzufuegenAntwort.Headers.Location?.AbsolutePath);
            var gespeichert = await hinzufuegenAntwort.Content
                .ReadFromJsonAsync<KostenpositionAntwort>();
            Assert.NotNull(gespeichert);
            Assert.Equal("Fachplanung", gespeichert.Bezeichnung);
            Assert.Equal(8_500.50m, gespeichert.Betrag);
            Assert.Equal(Kostenart.Fachplanung, gespeichert.Kostenart);

            var positionen = await client.GetFromJsonAsync<List<KostenpositionAntwort>>(route);
            var geladen = Assert.Single(positionen!);
            Assert.Equal(gespeichert.Id, geladen.Id);

            var loeschenAntwort = await client.DeleteAsync($"{route}/{gespeichert.Id}");
            Assert.Equal(HttpStatusCode.NoContent, loeschenAntwort.StatusCode);
            Assert.Empty((await client.GetFromJsonAsync<List<KostenpositionAntwort>>(route))!);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(testverzeichnis))
            {
                Directory.Delete(testverzeichnis, recursive: true);
            }
        }
    }

    private static async Task<(Guid ProjektId, Guid AlternativeId)>
        ErzeugeProjektMitAlternativeAsync(KostenpositionenApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KompassDbContext>();
        Assert.Empty(await dbContext.Database.GetPendingMigrationsAsync());

        await dbContext.Database.OpenConnectionAsync();
        await using (var fremdschluesselAbfrage =
            dbContext.Database.GetDbConnection().CreateCommand())
        {
            fremdschluesselAbfrage.CommandText = "PRAGMA foreign_keys;";
            Assert.Equal(1L, await fremdschluesselAbfrage.ExecuteScalarAsync());
        }

        var projekt = new Projekt(Guid.NewGuid(), "HTTP-Kostentest");
        var alternative = new Modernisierungsalternative(
            Guid.NewGuid(), "MP1", "Modernisierungspaket 1");
        projekt.AlternativeHinzufuegen(alternative);
        dbContext.Projekte.Add(projekt);
        await dbContext.SaveChangesAsync();
        return (projekt.Id, alternative.Id);
    }

    private sealed record KostenpositionAntwort(
        Guid Id,
        string Bezeichnung,
        decimal Betrag,
        Kostenart Kostenart);

    private sealed class KostenpositionenApiFactory(string testverzeichnis)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.AddConsole());
            builder.UseSetting(
                "ConnectionStrings:KompassDatabase",
                $"Data Source={Path.Combine(testverzeichnis, "kompass.db")}");
            builder.UseSetting("Fachdatenbanken:Verzeichnis", string.Empty);
        }
    }
}
