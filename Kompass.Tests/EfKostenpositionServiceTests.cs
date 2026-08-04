using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests;

public sealed class EfKostenpositionServiceTests
{
    [Fact]
    public async Task Hinzufuegen_Listen_und_Entfernen_persistieren_Position()
    {
        await using var db = await KostenTestdatenbank.ErstellenAsync();
        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        var position = new Kostenposition(
            Guid.NewGuid(), "Fachplanung", 8_500.50m, Kostenart.Fachplanung);

        var gespeichert = await db.Service.HinzufuegenAsync(
            projektId, alternativeId, position);
        db.Context.ChangeTracker.Clear();
        var geladen = await db.Service.ListenAsync(projektId, alternativeId);

        Assert.NotNull(gespeichert);
        var einzelne = Assert.Single(geladen);
        Assert.Equal(position.Id, einzelne.Id);
        Assert.Equal(8_500.50m, einzelne.Betrag);
        Assert.Equal(8_500.50m, geladen.Sum(eintrag => eintrag.Betrag));

        Assert.True(await db.Service.EntfernenAsync(
            projektId, alternativeId, position.Id));
        db.Context.ChangeTracker.Clear();
        Assert.Empty(await db.Service.ListenAsync(projektId, alternativeId));
    }

    [Fact]
    public async Task Listen_liefert_nur_Positionen_der_Alternative_im_Projekt()
    {
        await using var db = await KostenTestdatenbank.ErstellenAsync();
        var (projektA, alternativeA) = await db.ErzeugeProjektMitAlternativeAsync("A");
        var (projektB, alternativeB) = await db.ErzeugeProjektMitAlternativeAsync("B");

        await db.Service.HinzufuegenAsync(
            projektA,
            alternativeA,
            new Kostenposition(Guid.NewGuid(), "A", 100m, Kostenart.Architektur));
        await db.Service.HinzufuegenAsync(
            projektB,
            alternativeB,
            new Kostenposition(Guid.NewGuid(), "B", 200m, Kostenart.Tga));

        Assert.Single(await db.Service.ListenAsync(projektA, alternativeA));
        Assert.Empty(await db.Service.ListenAsync(projektA, alternativeB));
    }

    [Fact]
    public async Task Hinzufuegen_und_Entfernen_scheitern_bei_falscher_Zuordnung()
    {
        await using var db = await KostenTestdatenbank.ErstellenAsync();
        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        var position = new Kostenposition(
            Guid.NewGuid(), "Planung", 500m, Kostenart.Fachplanung);

        Assert.Null(await db.Service.HinzufuegenAsync(
            Guid.NewGuid(), alternativeId, position));
        Assert.False(await db.Service.EntfernenAsync(
            projektId, alternativeId, Guid.NewGuid()));
    }
}

internal sealed class KostenTestdatenbank : IAsyncDisposable
{
    private readonly SqliteConnection _verbindung;

    private KostenTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context)
    {
        _verbindung = verbindung;
        Context = context;
        Service = new EfKostenpositionService(context);
    }

    public KompassDbContext Context { get; }
    public EfKostenpositionService Service { get; }

    public static async Task<KostenTestdatenbank> ErstellenAsync()
    {
        var verbindung = new SqliteConnection("DataSource=:memory:");
        await verbindung.OpenAsync();
        var optionen = new DbContextOptionsBuilder<KompassDbContext>()
            .UseSqlite(verbindung)
            .Options;
        var context = new KompassDbContext(optionen);
        await context.Database.MigrateAsync();
        return new KostenTestdatenbank(verbindung, context);
    }

    public async Task<(Guid ProjektId, Guid AlternativeId)>
        ErzeugeProjektMitAlternativeAsync(string suffix = "Test")
    {
        var projekt = new Projekt(Guid.NewGuid(), $"Projekt {suffix}");
        var alternative = new Modernisierungsalternative(
            Guid.NewGuid(), $"Alternative {suffix}", "Kurztext");
        projekt.AlternativeHinzufuegen(alternative);
        Context.Projekte.Add(projekt);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return (projekt.Id, alternative.Id);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _verbindung.DisposeAsync();
    }
}
