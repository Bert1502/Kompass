using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class ProjektServiceTests
{
    [Fact]
    public async Task Crud_bereinigt_speichert_aktualisiert_und_loescht_Projekt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var erstellt =
            await service.ErstellenAsync(
                "  Rathaus  ");

        var gelesen =
            await service.NachIdAbrufenAsync(
                erstellt.Id);

        var aktualisiert =
            await service.AktualisierenAsync(
                erstellt.Id,
                "  Schule  ");

        var geloescht =
            await service.LoeschenAsync(
                erstellt.Id);

        var nachDemLoeschen =
            await service.NachIdAbrufenAsync(
                erstellt.Id);

        Assert.Equal(
            "Rathaus",
            erstellt.Name);
        Assert.Equal(
            erstellt,
            gelesen);
        Assert.Equal(
            "Schule",
            aktualisiert?.Name);
        Assert.True(
            geloescht);
        Assert.Null(
            nachDemLoeschen);
    }

    [Fact]
    public async Task AlleAbrufen_sortiert_nach_Name_und_zaehlt_Alternativen()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var projektZ =
            new Projekt(
                Guid.NewGuid(),
                "Zentrale");

        projektZ.AlternativeHinzufuegen(
            new Modernisierungsalternative(
                Guid.NewGuid(),
                "Fenster",
                ""));

        var projektA =
            new Projekt(
                Guid.NewGuid(),
                "Altbau");

        testdatenbank.Context.Projekte.AddRange(
            projektZ,
            projektA);

        await testdatenbank.Context.SaveChangesAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var projekte =
            await service.AlleAbrufenAsync();

        Assert.Equal(
            ["Altbau", "Zentrale"],
            projekte.Select(
                projekt => projekt.Name));
        Assert.Equal(
            [0, 1],
            projekte.Select(
                projekt => projekt.AnzahlAlternativen));
    }

    [Fact]
    public async Task Erstellen_lehnt_doppelten_bereinigten_Namen_ab()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        await service.ErstellenAsync(
            "Rathaus");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ErstellenAsync(
                "  Rathaus  "));
    }

    [Fact]
    public async Task Aktualisieren_lehnt_Namen_eines_anderen_Projekts_ab()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var erstesProjekt =
            await service.ErstellenAsync(
                "Rathaus");

        await service.ErstellenAsync(
            "Schule");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AktualisierenAsync(
                erstesProjekt.Id,
                "Schule"));
    }

    [Fact]
    public async Task Unbekannte_oder_leere_Id_liefert_keinen_Treffer()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        Assert.Null(
            await service.NachIdAbrufenAsync(
                Guid.Empty));
        Assert.Null(
            await service.NachIdAbrufenAsync(
                Guid.NewGuid()));
        Assert.Null(
            await service.AktualisierenAsync(
                Guid.NewGuid(),
                "Unbekannt"));
        Assert.False(
            await service.LoeschenAsync(
                Guid.Empty));
        Assert.False(
            await service.LoeschenAsync(
                Guid.NewGuid()));
    }
}

internal sealed class ProjektTestdatenbank : IAsyncDisposable
{
    private ProjektTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context)
    {
        Verbindung = verbindung;
        Context = context;
    }

    private SqliteConnection Verbindung { get; }

    public KompassDbContext Context { get; }

    public static async Task<ProjektTestdatenbank> ErstellenAsync()
    {
        var verbindung =
            new SqliteConnection(
                "Data Source=:memory:");

        await verbindung.OpenAsync();

        var options =
            new DbContextOptionsBuilder<KompassDbContext>()
                .UseSqlite(verbindung)
                .Options;

        var context =
            new KompassDbContext(options);

        await context.Database.MigrateAsync();

        return new ProjektTestdatenbank(
            verbindung,
            context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Verbindung.DisposeAsync();
    }
}
