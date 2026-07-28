using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Domain.Reports;
using Kompass.Domain.Waermebruecken;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class BerichtsServiceTests
{
    [Fact]
    public async Task Alternativenvergleich_liefert_null_wenn_Projekt_nicht_gefunden()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.AlternativenvergleichErzeugenAsync(Guid.NewGuid());

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Alternativenvergleich_liefert_Bericht_mit_leerem_Projekt()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync("Mustergebäude");

        var bericht =
            await db.Service.AlternativenvergleichErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(projektId, bericht.Kopf.ProjektId);
        Assert.Equal("Mustergebäude", bericht.Kopf.ProjektName);
        Assert.Equal(Berichtstyp.Alternativenvergleich, bericht.Kopf.Berichtstyp);
        Assert.Empty(bericht.Alternativen);
    }

    [Fact]
    public async Task Alternativenvergleich_listet_Alternativen_mit_Kostenpositionen()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        await db.ErzeugeAlternativeAsync(
            projektId,
            b56Position: 1,
            bezeichnung: "Vollsanierung",
            gesamtkosten: 45000m);

        await db.ErzeugeAlternativeAsync(
            projektId,
            b56Position: 2,
            bezeichnung: "Teilsanierung",
            gesamtkosten: 20000m);

        var bericht =
            await db.Service.AlternativenvergleichErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(2, bericht.Alternativen.Count);

        var erste = bericht.Alternativen[0];
        Assert.Equal(1, erste.B56Position);
        Assert.Equal("Vollsanierung", erste.Bezeichnung);
        Assert.Equal(45000m, erste.Gesamtkosten);
        Assert.Equal(1, erste.AnzahlKostenpositionen);
    }

    [Fact]
    public async Task Alternativenvergleich_sortiert_nach_B56Position()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        await db.ErzeugeAlternativeAsync(
            projektId,
            b56Position: 3,
            bezeichnung: "Alt C",
            gesamtkosten: 0m);

        await db.ErzeugeAlternativeAsync(
            projektId,
            b56Position: 1,
            bezeichnung: "Alt A",
            gesamtkosten: 0m);

        var bericht =
            await db.Service.AlternativenvergleichErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(1, bericht.Alternativen[0].B56Position);
        Assert.Equal(3, bericht.Alternativen[1].B56Position);
    }

    [Fact]
    public async Task Waermebrueckenuebersicht_liefert_null_wenn_Projekt_nicht_gefunden()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.WaermebrueckenuebersichtErzeugenAsync(Guid.NewGuid());

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Waermebrueckenuebersicht_liefert_Bericht_ohne_Waermebruecken()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync("Mustergebäude");

        var bericht =
            await db.Service.WaermebrueckenuebersichtErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(projektId, bericht.Kopf.ProjektId);
        Assert.Equal("Mustergebäude", bericht.Kopf.ProjektName);
        Assert.Equal(Berichtstyp.Waermebrueckenuebersicht, bericht.Kopf.Berichtstyp);
        Assert.Empty(bericht.Waermebruecken);
    }

    [Fact]
    public async Task Waermebrueckenuebersicht_listet_Waermebruecken_des_Projekts()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var wb1 = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

        var wb2 = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            "WB02",
            "Fensteranschluss",
            WaermebrueckeTyp.Wandanschluss);

        db.Context.Waermebruecken.AddRange(wb1, wb2);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();

        var bericht =
            await db.Service.WaermebrueckenuebersichtErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(2, bericht.Waermebruecken.Count);
    }

    [Fact]
    public async Task Waermebrueckenuebersicht_isoliert_Waermebruecken_nach_Projekt()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId1 = await db.ErzeugeProjektAsync("Projekt 1");
        var projektId2 = await db.ErzeugeProjektAsync("Projekt 2");

        db.Context.Waermebruecken.Add(
            new Waermebruecke(
                Guid.NewGuid(),
                projektId1,
                "WB01",
                "Ecke",
                WaermebrueckeTyp.Ecke));

        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();

        var bericht =
            await db.Service.WaermebrueckenuebersichtErzeugenAsync(projektId2);

        Assert.NotNull(bericht);
        Assert.Empty(bericht.Waermebruecken);
    }
}

internal sealed class BerichtsTestdatenbank : IAsyncDisposable
{
    private BerichtsTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context,
        BerichtsService service)
    {
        Verbindung = verbindung;
        Context = context;
        Service = service;
    }

    public KompassDbContext Context { get; }
    public BerichtsService Service { get; }
    private SqliteConnection Verbindung { get; }

    public static async Task<BerichtsTestdatenbank> ErstellenAsync()
    {
        var verbindung = new SqliteConnection("DataSource=:memory:");
        await verbindung.OpenAsync();

        var options =
            new DbContextOptionsBuilder<KompassDbContext>()
                .UseSqlite(verbindung)
                .Options;

        var context = new KompassDbContext(options);
        await context.Database.MigrateAsync();

        var service = new BerichtsService(context);

        return new BerichtsTestdatenbank(verbindung, context, service);
    }

    public async Task<Guid> ErzeugeProjektAsync(string name = "Testobjekt")
    {
        var projekt = new Projekt(Guid.NewGuid(), name);
        Context.Projekte.Add(projekt);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return projekt.Id;
    }

    public async Task ErzeugeAlternativeAsync(
        Guid projektId,
        int b56Position,
        string bezeichnung,
        decimal gesamtkosten)
    {
        var alternativeId = Guid.NewGuid();

        var alternative = new Modernisierungsalternative(
            alternativeId,
            bezeichnung,
            string.Empty,
            quellSnapshotId: null,
            b56Position: b56Position);

        Context.Set<Modernisierungsalternative>().Add(alternative);

        Context.Entry(alternative)
            .Property("ProjektId")
            .CurrentValue = projektId;

        if (gesamtkosten > 0)
        {
            var kostenposition = new Kostenposition(
                Guid.NewGuid(),
                "Maßnahme",
                gesamtkosten,
                Kostenart.Sonstige);

            Context.Set<Kostenposition>().Add(kostenposition);

            Context.Entry(kostenposition)
                .Property("ModernisierungsalternativeId")
                .CurrentValue = alternativeId;
        }

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Verbindung.DisposeAsync();
    }
}
