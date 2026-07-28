using Kompass.Domain.Common;
using Kompass.Domain.Projects;
using Kompass.Domain.Waermebruecken;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class EfWaermebrueckeServiceTests
{
    [Fact]
    public async Task Listen_liefert_leere_Liste_wenn_keine_Waermebruecken_vorhanden()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var ergebnis = await db.Service.ListenAsync(Guid.NewGuid());

        Assert.Empty(ergebnis);
    }

    [Fact]
    public async Task Anlegen_und_Listen_gibt_angelegte_Waermebruecke_zurueck()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var wb = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

        var angelegt = await db.Service.AnlegenAsync(wb);

        Assert.NotNull(angelegt);
        Assert.Equal("WB01", angelegt.InterneNummer);

        var liste = await db.Service.ListenAsync(projektId);

        Assert.Single(liste);
        Assert.Equal("WB01", liste[0].InterneNummer);
    }

    [Fact]
    public async Task Anlegen_liefert_null_wenn_Projekt_nicht_gefunden()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var wb = new Waermebruecke(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

        var angelegt = await db.Service.AnlegenAsync(wb);

        Assert.Null(angelegt);
    }

    [Fact]
    public async Task Anlegen_wirft_DomainException_bei_doppelter_InternerNummer()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var wb1 = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

        await db.Service.AnlegenAsync(wb1);

        var wb2 = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB01",
            "Andere Wärmebrücke",
            WaermebrueckeTyp.Wandanschluss);

        await Assert.ThrowsAsync<DomainException>(
            () => db.Service.AnlegenAsync(wb2));
    }

    [Fact]
    public async Task Abrufen_gibt_Waermebruecke_zurueck()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var wb = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

        await db.Service.AnlegenAsync(wb);

        var abgerufen = await db.Service.AbrufenAsync(projektId, wb.Id);

        Assert.NotNull(abgerufen);
        Assert.Equal(wb.Id, abgerufen.Id);
    }

    [Fact]
    public async Task Abrufen_liefert_null_wenn_nicht_gefunden()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var ergebnis = await db.Service.AbrufenAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Aktualisieren_speichert_geaenderte_Daten()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var wb = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB01",
            "Alt",
            WaermebrueckeTyp.Ecke);

        await db.Service.AnlegenAsync(wb);

        wb.DatenAktualisieren(
            "WB01",
            "Aktualisiert",
            WaermebrueckeTyp.Wandanschluss,
            WaermebrueckeStatus.Berechnet,
            GleichwertigkeitStatus.Gleichwertig,
            psiWert: 0.05m);

        var aktualisiert = await db.Service.AktualisierenAsync(wb);

        Assert.True(aktualisiert);

        db.Context.ChangeTracker.Clear();

        var abgerufen = await db.Service.AbrufenAsync(projektId, wb.Id);

        Assert.NotNull(abgerufen);
        Assert.Equal("Aktualisiert", abgerufen.Bezeichnung);
        Assert.Equal(WaermebrueckeStatus.Berechnet, abgerufen.Status);
        Assert.Equal(0.05m, abgerufen.PsiWert);
    }

    [Fact]
    public async Task Aktualisieren_liefert_false_wenn_nicht_gefunden()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var wb = new Waermebruecke(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WB01",
            "Bezeichnung",
            WaermebrueckeTyp.Ecke);

        var ergebnis = await db.Service.AktualisierenAsync(wb);

        Assert.False(ergebnis);
    }

    [Fact]
    public async Task Loeschen_entfernt_Waermebruecke()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var wb = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

        await db.Service.AnlegenAsync(wb);

        var geloescht = await db.Service.LoeschenAsync(projektId, wb.Id);

        Assert.True(geloescht);

        var liste = await db.Service.ListenAsync(projektId);

        Assert.Empty(liste);
    }

    [Fact]
    public async Task Loeschen_liefert_false_wenn_nicht_gefunden()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var ergebnis = await db.Service.LoeschenAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.False(ergebnis);
    }

    [Fact]
    public async Task Listen_liefert_nur_Waermebruecken_des_angegebenen_Projekts()
    {
        await using var db = await WaermebrueckeTestdatenbank.ErstellenAsync();

        var projektId1 = await db.ErzeugeProjektAsync("Projekt A");
        var projektId2 = await db.ErzeugeProjektAsync("Projekt B");

        var wb1 = new Waermebruecke(
            Guid.NewGuid(),
            projektId1,
            "WB01",
            "Wärmebrücke Projekt 1",
            WaermebrueckeTyp.Ecke);

        var wb2 = new Waermebruecke(
            Guid.NewGuid(),
            projektId2,
            "WB01",
            "Wärmebrücke Projekt 2",
            WaermebrueckeTyp.Wandanschluss);

        await db.Service.AnlegenAsync(wb1);
        await db.Service.AnlegenAsync(wb2);

        var liste = await db.Service.ListenAsync(projektId1);

        Assert.Single(liste);
        Assert.Equal(projektId1, liste[0].ProjektId);
    }
}

internal sealed class WaermebrueckeTestdatenbank : IAsyncDisposable
{
    private WaermebrueckeTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context,
        EfWaermebrueckeService service)
    {
        Verbindung = verbindung;
        Context = context;
        Service = service;
    }

    public KompassDbContext Context { get; }
    public EfWaermebrueckeService Service { get; }
    private SqliteConnection Verbindung { get; }

    public static async Task<WaermebrueckeTestdatenbank> ErstellenAsync()
    {
        var verbindung = new SqliteConnection("DataSource=:memory:");
        await verbindung.OpenAsync();

        var options =
            new DbContextOptionsBuilder<KompassDbContext>()
                .UseSqlite(verbindung)
                .Options;

        var context = new KompassDbContext(options);
        await context.Database.MigrateAsync();

        var service = new EfWaermebrueckeService(context);

        return new WaermebrueckeTestdatenbank(verbindung, context, service);
    }

    public async Task<Guid> ErzeugeProjektAsync(string name = "Testobjekt")
    {
        var projekt = new Projekt(Guid.NewGuid(), name);

        Context.Projekte.Add(projekt);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        return projekt.Id;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Verbindung.DisposeAsync();
    }
}
