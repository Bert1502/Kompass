using Kompass.Domain.Common;
using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class EfWirtschaftlichkeitsServiceTests
{
    [Fact]
    public async Task AnnahmenAbrufen_liefert_null_wenn_Alternative_nicht_gefunden()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.AnnahmenAbrufenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task AnnahmenAbrufen_liefert_null_wenn_Alternative_anderem_Projekt_gehoert()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var (_, alternativeId) =
            await db.ErzeugeProjektMitAlternativeAsync();

        var ergebnis =
            await db.Service.AnnahmenAbrufenAsync(
                Guid.NewGuid(),
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task AnnahmenSpeichern_und_Abrufen_rundet_Werte_korrekt()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) =
            await db.ErzeugeProjektMitAlternativeAsync();

        var annahmen = ErzeugeAnnahmen(alternativeId);

        await db.Service.AnnahmenSpeichernAsync(annahmen);

        var abgerufen =
            await db.Service.AnnahmenAbrufenAsync(
                projektId,
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.NotNull(abgerufen);
        Assert.Equal(annahmen.Id, abgerufen.Id);
        Assert.Equal(annahmen.Betrachtungszeitraum, abgerufen.Betrachtungszeitraum);
        Assert.Equal(annahmen.Diskontsatz, abgerufen.Diskontsatz);
        Assert.Equal(annahmen.Foerderung, abgerufen.Foerderung);
    }

    [Fact]
    public async Task AnnahmenSpeichern_aktualisiert_vorhandene_Annahmen()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) =
            await db.ErzeugeProjektMitAlternativeAsync();

        var ersteAnnahmen = ErzeugeAnnahmen(alternativeId, foerderung: 0m);

        await db.Service.AnnahmenSpeichernAsync(ersteAnnahmen);

        var aktualisierteAnnahmen =
            new Wirtschaftlichkeitsannahmen(
                Guid.NewGuid(),
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                20, 0.04m, 0.02m, 0m, 20, 3_000m);

        await db.Service.AnnahmenSpeichernAsync(aktualisierteAnnahmen);

        var abgerufen =
            await db.Service.AnnahmenAbrufenAsync(
                projektId,
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.NotNull(abgerufen);
        Assert.Equal(3_000m, abgerufen.Foerderung);

        var anzahl =
            await db.Context.Wirtschaftlichkeitsannahmen
                .CountAsync(
                    a =>
                        a.ModernisierungsalternativeId == alternativeId &&
                        a.Basis == WirtschaftlichkeitsBasis.Bilanziert);

        Assert.Equal(1, anzahl);
    }

    [Fact]
    public async Task AnnahmenSpeichern_speichert_Energietraeger_mit()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) =
            await db.ErzeugeProjektMitAlternativeAsync();

        var annahmen = ErzeugeAnnahmen(alternativeId);

        annahmen.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.08m, 0.03m, 0.2m, 50m, 0.05m,
                20_000m, 10_000m));

        await db.Service.AnnahmenSpeichernAsync(annahmen);

        var abgerufen =
            await db.Service.AnnahmenAbrufenAsync(
                projektId,
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.NotNull(abgerufen);
        Assert.Single(abgerufen.EnergietraegerAnnahmen);
        Assert.Equal(
            Energietraeger.Gas,
            abgerufen.EnergietraegerAnnahmen.Single().Energietraeger);
    }

    [Fact]
    public async Task Berechnen_liefert_null_wenn_Alternative_nicht_gefunden()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.BerechnenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Berechnen_liefert_null_wenn_keine_Annahmen_vorhanden()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) =
            await db.ErzeugeProjektMitAlternativeAsync();

        var ergebnis =
            await db.Service.BerechnenAsync(
                projektId,
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Berechnen_liefert_Ergebnis_mit_Kostenpositionen_und_Energietraeger()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) =
            await db.ErzeugeProjektMitAlternativeAsync();

        await db.ErzeugeKostenpositionAsync(alternativeId, 10_000m);

        var annahmen = ErzeugeAnnahmen(alternativeId);

        annahmen.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                20_000m, 10_000m));

        await db.Service.AnnahmenSpeichernAsync(annahmen);

        var ergebnis =
            await db.Service.BerechnenAsync(
                projektId,
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert);

        Assert.NotNull(ergebnis);
        // Investition 10.000 €, Einsparung 1.000 €/a → Amort. 10 Jahre
        Assert.Equal(10_000m, ergebnis.Eigenanteil);
        Assert.Equal(1_000m, ergebnis.JaehrlicheEnergiekosteneinsparungJahr1);
        Assert.Equal(10m, ergebnis.AmortisationsdauerStatisch);
    }

    [Fact]
    public async Task Berechnen_bilanziert_und_praktisch_sind_unabhaengig()
    {
        await using var db = await WirtschaftlichkeitsTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) =
            await db.ErzeugeProjektMitAlternativeAsync();

        await db.ErzeugeKostenpositionAsync(alternativeId, 5_000m);

        var bilanziert = ErzeugeAnnahmen(
            alternativeId,
            basis: WirtschaftlichkeitsBasis.Bilanziert);

        bilanziert.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                20_000m, 10_000m));

        await db.Service.AnnahmenSpeichernAsync(bilanziert);

        var praktisch =
            new Wirtschaftlichkeitsannahmen(
                Guid.NewGuid(),
                alternativeId,
                WirtschaftlichkeitsBasis.Praktisch,
                20, 0.04m, 0.02m, 0m, 20, 0m);

        praktisch.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                5_000m, 2_000m));

        await db.Service.AnnahmenSpeichernAsync(praktisch);

        var ergebnisBilanziert =
            await db.Service.BerechnenAsync(
                projektId,
                alternativeId,
                WirtschaftlichkeitsBasis.Bilanziert);

        var ergebnisPraktisch =
            await db.Service.BerechnenAsync(
                projektId,
                alternativeId,
                WirtschaftlichkeitsBasis.Praktisch);

        Assert.NotNull(ergebnisBilanziert);
        Assert.NotNull(ergebnisPraktisch);
        Assert.NotEqual(
            ergebnisBilanziert.JaehrlicheEnergiekosteneinsparungJahr1,
            ergebnisPraktisch.JaehrlicheEnergiekosteneinsparungJahr1);
    }

    private static Wirtschaftlichkeitsannahmen ErzeugeAnnahmen(
        Guid alternativeId,
        decimal foerderung = 0m,
        WirtschaftlichkeitsBasis basis = WirtschaftlichkeitsBasis.Bilanziert)
    {
        return new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            alternativeId,
            basis,
            betrachtungszeitraum: 20,
            diskontsatz: 0.04m,
            inflationsrate: 0.02m,
            jaehrlicheWartungsmehrkosten: 0m,
            nutzungsdauer: 20,
            foerderung: foerderung);
    }
}

internal sealed class WirtschaftlichkeitsTestdatenbank : IAsyncDisposable
{
    private WirtschaftlichkeitsTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context,
        EfWirtschaftlichkeitsService service)
    {
        Verbindung = verbindung;
        Context = context;
        Service = service;
    }

    private SqliteConnection Verbindung { get; }

    public KompassDbContext Context { get; }

    public EfWirtschaftlichkeitsService Service { get; }

    public static async Task<WirtschaftlichkeitsTestdatenbank> ErstellenAsync()
    {
        var verbindung =
            new SqliteConnection("Data Source=:memory:");

        await verbindung.OpenAsync();

        var options =
            new DbContextOptionsBuilder<KompassDbContext>()
                .UseSqlite(verbindung)
                .Options;

        var context = new KompassDbContext(options);

        await context.Database.MigrateAsync();

        var service = new EfWirtschaftlichkeitsService(context);

        return new WirtschaftlichkeitsTestdatenbank(
            verbindung,
            context,
            service);
    }

    public async Task<(Guid ProjektId, Guid AlternativeId)>
        ErzeugeProjektMitAlternativeAsync()
    {
        var alternativeId = Guid.NewGuid();

        var projekt = new Projekt(
            Guid.NewGuid(),
            "Testobjekt");

        projekt.AlternativeHinzufuegen(
            new Modernisierungsalternative(
                alternativeId,
                "Dämmung",
                ""));

        Context.Projekte.Add(projekt);

        await Context.SaveChangesAsync();

        return (projekt.Id, alternativeId);
    }

    public async Task ErzeugeKostenpositionAsync(
        Guid alternativeId,
        decimal betrag)
    {
        var kostenposition = new Kostenposition(
            Guid.NewGuid(),
            "Materialkosten",
            betrag,
            Kostenart.Architektur);

        Context.Set<Kostenposition>().Add(kostenposition);

        Context.Entry(kostenposition)
            .Property("ModernisierungsalternativeId")
            .CurrentValue = alternativeId;

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Verbindung.DisposeAsync();
    }
}
