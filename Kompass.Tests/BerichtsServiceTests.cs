using Kompass.Domain.Economics;
using Kompass.Domain.Funding;
using Kompass.Domain.Projects;
using Kompass.Domain.Reports;
using Kompass.Domain.Verbrauch;
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

public sealed class WirtschaftlichkeitsberichtServiceTests
{
    [Fact]
    public async Task Wirtschaftlichkeitsbericht_liefert_null_wenn_Projekt_nicht_gefunden()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var ergebnis = await db.Service.WirtschaftlichkeitsberichtErzeugenAsync(
            Guid.NewGuid(),
            WirtschaftlichkeitsBasis.Bilanziert);

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Wirtschaftlichkeitsbericht_liefert_leeren_Bericht_wenn_keine_Annahmen()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync("Mustergebäude");
        await db.ErzeugeAlternativeAsync(projektId, 1, "Alt A", 50000m);

        var bericht = await db.Service.WirtschaftlichkeitsberichtErzeugenAsync(
            projektId,
            WirtschaftlichkeitsBasis.Bilanziert);

        Assert.NotNull(bericht);
        Assert.Equal(projektId, bericht.Kopf.ProjektId);
        Assert.Equal(Berichtstyp.Wirtschaftlichkeitsbericht, bericht.Kopf.Berichtstyp);
        Assert.Empty(bericht.Alternativen);
    }

    [Fact]
    public async Task Wirtschaftlichkeitsbericht_berechnet_Ergebnis_fuer_Alternative_mit_Annahmen()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();
        var alternativeId = await db.ErzeugeAlternativeAsync(
            projektId, 1, "Vollsanierung", 50000m);

        await db.ErzeugeWirtschaftlichkeitsannahmenAsync(
            alternativeId,
            WirtschaftlichkeitsBasis.Bilanziert,
            betrachtungszeitraum: 20,
            diskontsatz: 0.04m,
            inflationsrate: 0.02m,
            foerderung: 10000m);

        var bericht = await db.Service.WirtschaftlichkeitsberichtErzeugenAsync(
            projektId,
            WirtschaftlichkeitsBasis.Bilanziert);

        Assert.NotNull(bericht);
        Assert.Single(bericht.Alternativen);

        var zeile = bericht.Alternativen[0];
        Assert.Equal(alternativeId, zeile.AlternativeId);
        Assert.Equal("Vollsanierung", zeile.Bezeichnung);
        Assert.Equal(WirtschaftlichkeitsBasis.Bilanziert, zeile.Basis);
        Assert.Equal(50000m, zeile.Investitionskosten);
        Assert.Equal(10000m, zeile.Foerderung);
        Assert.Equal(40000m, zeile.Ergebnis.Eigenanteil);
    }

    [Fact]
    public async Task Wirtschaftlichkeitsbericht_filtert_nach_Basis()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();
        var alternativeId = await db.ErzeugeAlternativeAsync(
            projektId, 1, "Alt A", 30000m);

        await db.ErzeugeWirtschaftlichkeitsannahmenAsync(
            alternativeId,
            WirtschaftlichkeitsBasis.Praktisch,
            betrachtungszeitraum: 15,
            diskontsatz: 0.03m,
            inflationsrate: 0.02m,
            foerderung: 0m);

        var bilanziertBericht = await db.Service.WirtschaftlichkeitsberichtErzeugenAsync(
            projektId,
            WirtschaftlichkeitsBasis.Bilanziert);

        var praktischBericht = await db.Service.WirtschaftlichkeitsberichtErzeugenAsync(
            projektId,
            WirtschaftlichkeitsBasis.Praktisch);

        Assert.NotNull(bilanziertBericht);
        Assert.Empty(bilanziertBericht.Alternativen);

        Assert.NotNull(praktischBericht);
        Assert.Single(praktischBericht.Alternativen);
    }
}

public sealed class FoerderuebersichtServiceTests
{
    [Fact]
    public async Task Foerderuebersicht_liefert_null_wenn_Projekt_nicht_gefunden()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.FoerderuebersichtErzeugenAsync(Guid.NewGuid());

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Foerderuebersicht_liefert_Bericht_ohne_Alternativen()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync("Mustergebäude");

        var bericht =
            await db.Service.FoerderuebersichtErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(projektId, bericht.Kopf.ProjektId);
        Assert.Equal("Mustergebäude", bericht.Kopf.ProjektName);
        Assert.Equal(Berichtstyp.Foerderuebersicht, bericht.Kopf.Berichtstyp);
        Assert.Empty(bericht.Alternativen);
    }

    [Fact]
    public async Task Foerderuebersicht_listet_ohne_Zuordnung_alle_Katalogprogramme_als_Kandidaten()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();
        await db.ErzeugeAlternativeAsync(projektId, 1, "Alt A", 40000m);
        await db.ErzeugeAlternativeAsync(projektId, 2, "Alt B", 20000m);
        await db.ErzeugeFoerderprogrammAsync("BEG EM", 1);

        var bericht =
            await db.Service.FoerderuebersichtErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(2, bericht.Alternativen.Count);
        Assert.All(bericht.Alternativen, a => Assert.Single(a.ZugeordneteProgramme));
    }

    [Fact]
    public async Task Foerderuebersicht_zeigt_zugeordnete_Foerderprogramme()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();
        var alternativeId = await db.ErzeugeAlternativeAsync(
            projektId, 1, "Vollsanierung", 50000m);
        var programmId = await db.ErzeugeFoerderprogrammAsync("BEG EM", 1);
        await db.ErzeugeFoerderungZuordnungAsync(alternativeId, programmId);

        var bericht =
            await db.Service.FoerderuebersichtErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Single(bericht.Alternativen);

        var alternative = bericht.Alternativen[0];
        Assert.Equal(alternativeId, alternative.AlternativeId);
        Assert.Equal("Vollsanierung", alternative.Bezeichnung);
        Assert.Single(alternative.ZugeordneteProgramme);
        Assert.Equal("BEG EM", alternative.ZugeordneteProgramme[0].Programmkennung);
    }

    [Fact]
    public async Task Verbrauchsvergleich_liefert_null_wenn_Projekt_nicht_gefunden()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var bericht =
            await db.Service.VerbrauchsvergleichErzeugenAsync(Guid.NewGuid());

        Assert.Null(bericht);
    }

    [Fact]
    public async Task Verbrauchsvergleich_liefert_leeren_Bericht_ohne_Verbrauchsdaten()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync("Mustergebäude");

        var bericht =
            await db.Service.VerbrauchsvergleichErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(projektId, bericht.Kopf.ProjektId);
        Assert.Equal("Mustergebäude", bericht.Kopf.ProjektName);
        Assert.Equal(Berichtstyp.Verbrauchsvergleich, bericht.Kopf.Berichtstyp);
        Assert.Empty(bericht.Zeilen);
    }

    [Fact]
    public async Task Verbrauchsvergleich_listet_Zeilen_sortiert_nach_Periode()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        await db.ErzeugeVerbrauchsDatenAsync(
            projektId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m);

        await db.ErzeugeVerbrauchsDatenAsync(
            projektId,
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 12, 31),
            Energietraeger.Gas,
            11000m,
            2200m);

        var bericht =
            await db.Service.VerbrauchsvergleichErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        Assert.Equal(2, bericht.Zeilen.Count);
        Assert.Equal(new DateOnly(2023, 1, 1), bericht.Zeilen[0].PeriodeVon);
        Assert.Equal(new DateOnly(2024, 1, 1), bericht.Zeilen[1].PeriodeVon);
    }

    [Fact]
    public async Task Verbrauchsvergleich_berechnet_Abweichung_korrekt()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        await db.ErzeugeVerbrauchsDatenAsync(
            projektId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m,
            b56VergleichsWert: 10000m);

        var bericht =
            await db.Service.VerbrauchsvergleichErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        var zeile = Assert.Single(bericht.Zeilen);
        Assert.Equal(12000m, zeile.Menge);
        Assert.Equal(10000m, zeile.B56VergleichsWert);
        Assert.Equal(2000m, zeile.Abweichung);
        Assert.Equal(20m, zeile.AbweichungProzent);
    }

    [Fact]
    public async Task Verbrauchsvergleich_hat_keine_Abweichung_ohne_B56Vergleichswert()
    {
        await using var db = await BerichtsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        await db.ErzeugeVerbrauchsDatenAsync(
            projektId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Strom,
            5000m,
            1000m);

        var bericht =
            await db.Service.VerbrauchsvergleichErzeugenAsync(projektId);

        Assert.NotNull(bericht);
        var zeile = Assert.Single(bericht.Zeilen);
        Assert.Null(zeile.B56VergleichsWert);
        Assert.Null(zeile.Abweichung);
        Assert.Null(zeile.AbweichungProzent);
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

    public async Task<Guid> ErzeugeAlternativeAsync(
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
        return alternativeId;
    }

    public async Task ErzeugeWirtschaftlichkeitsannahmenAsync(
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        int betrachtungszeitraum,
        decimal diskontsatz,
        decimal inflationsrate,
        decimal foerderung)
    {
        var annahmen = new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            alternativeId,
            basis,
            betrachtungszeitraum,
            diskontsatz,
            inflationsrate,
            jaehrlicheWartungsmehrkosten: 0m,
            nutzungsdauer: betrachtungszeitraum,
            foerderung);

        Context.Wirtschaftlichkeitsannahmen.Add(annahmen);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task<Guid> ErzeugeFoerderprogrammAsync(
        string programmkennung,
        int version)
    {
        var programm = new Foerderprogramm(
            Guid.NewGuid(),
            programmkennung,
            version,
            gueltigAb: new DateOnly(2024, 1, 1),
            gueltigBis: null,
            zielgruppe: "Eigentümer",
            foerdergegenstand: "Gebäudesanierung",
            technischeMindestanforderungen: "Effizienzhaus 85",
            foerdersatz: 0.15m,
            hoechstbetrag: 30000m,
            kumulierbarkeit: "Nicht kumulierbar",
            pflichtnachweise: "Energieausweis",
            quellenstand: "2024-01-01");

        Context.Foerderprogramme.Add(programm);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return programm.Id;
    }

    public async Task ErzeugeFoerderungZuordnungAsync(
        Guid alternativeId,
        Guid foerderprogrammId)
    {
        var zuordnung = new FoerderungZuordnung(
            Guid.NewGuid(),
            alternativeId,
            foerderprogrammId);

        Context.FoerderungZuordnungen.Add(zuordnung);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task ErzeugeVerbrauchsDatenAsync(
        Guid projektId,
        DateOnly periodeVon,
        DateOnly periodeBis,
        Energietraeger energietraeger,
        decimal menge,
        decimal kosten,
        decimal? b56VergleichsWert = null)
    {
        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            projektId,
            periodeVon,
            periodeBis,
            energietraeger,
            menge,
            kosten);

        if (b56VergleichsWert.HasValue)
        {
            daten.Aktualisieren(
                periodeVon,
                periodeBis,
                energietraeger,
                menge,
                kosten,
                witterungsbereinigungsFaktor: null,
                flaeche: null,
                b56VergleichsWert,
                anpassungsFaktor: null,
                anpassungsBegruendung: null,
                abweichungsursache: null);
        }

        Context.VerbrauchsDaten.Add(daten);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Verbindung.DisposeAsync();
    }
}
