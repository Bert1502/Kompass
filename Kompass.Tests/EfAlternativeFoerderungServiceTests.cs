using Kompass.Domain.Economics;
using Kompass.Domain.Funding;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class EfAlternativeFoerderungServiceTests
{
    [Fact]
    public async Task ZugeordneteProgrammeListen_liefert_leere_Liste_wenn_Alternative_nicht_gefunden()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var programme =
            await db.Service.ZugeordneteProgrammeListenAsync(
                Guid.NewGuid(),
                Guid.NewGuid());

        Assert.Empty(programme);
    }

    [Fact]
    public async Task ZugeordneteProgrammeListen_liefert_leere_Liste_wenn_Alternative_anderem_Projekt_gehoert()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (_, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();

        var programme =
            await db.Service.ZugeordneteProgrammeListenAsync(
                Guid.NewGuid(),
                alternativeId);

        Assert.Empty(programme);
    }

    [Fact]
    public async Task ProgrammZuordnen_und_Listen_gibt_zugeordnetes_Programm_zurueck()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        var programm = await db.ErzeugeFoerderprogrammAsync();

        var zugeordnet =
            await db.Service.ProgrammZuordnenAsync(
                projektId,
                alternativeId,
                programm.Id);

        Assert.True(zugeordnet);

        var programme =
            await db.Service.ZugeordneteProgrammeListenAsync(
                projektId,
                alternativeId);

        Assert.Single(programme);
        Assert.Equal(programm.Id, programme[0].Id);
        Assert.Equal("BEG EM", programme[0].Programmkennung);
    }

    [Fact]
    public async Task ProgrammZuordnen_liefert_false_wenn_Programm_nicht_gefunden()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();

        var zugeordnet =
            await db.Service.ProgrammZuordnenAsync(
                projektId,
                alternativeId,
                Guid.NewGuid());

        Assert.False(zugeordnet);
    }

    [Fact]
    public async Task ProgrammZuordnen_liefert_false_wenn_Zuordnung_bereits_besteht()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        var programm = await db.ErzeugeFoerderprogrammAsync();

        await db.Service.ProgrammZuordnenAsync(
            projektId,
            alternativeId,
            programm.Id);

        var erneut =
            await db.Service.ProgrammZuordnenAsync(
                projektId,
                alternativeId,
                programm.Id);

        Assert.False(erneut);
    }

    [Fact]
    public async Task ProgrammEntfernen_entfernt_Zuordnung()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        var programm = await db.ErzeugeFoerderprogrammAsync();

        await db.Service.ProgrammZuordnenAsync(
            projektId,
            alternativeId,
            programm.Id);

        var entfernt =
            await db.Service.ProgrammEntfernenAsync(
                projektId,
                alternativeId,
                programm.Id);

        Assert.True(entfernt);

        var programme =
            await db.Service.ZugeordneteProgrammeListenAsync(
                projektId,
                alternativeId);

        Assert.Empty(programme);
    }

    [Fact]
    public async Task ProgrammEntfernen_liefert_false_wenn_Zuordnung_nicht_vorhanden()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        var programm = await db.ErzeugeFoerderprogrammAsync();

        var entfernt =
            await db.Service.ProgrammEntfernenAsync(
                projektId,
                alternativeId,
                programm.Id);

        Assert.False(entfernt);
    }

    [Fact]
    public async Task FoerderungBerechnen_liefert_null_wenn_Alternative_nicht_gefunden()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.FoerderungBerechnenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2026, 7, 1));

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task FoerderungBerechnen_liefert_Ergebnis_ohne_zugeordnete_Programme()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();

        var ergebnis =
            await db.Service.FoerderungBerechnenAsync(
                projektId,
                alternativeId,
                new DateOnly(2026, 7, 1));

        Assert.NotNull(ergebnis);
        Assert.Equal(0m, ergebnis.Investitionskosten);
        Assert.Equal(0m, ergebnis.GesamtFoerderung);
        Assert.Equal(0m, ergebnis.Eigenanteil);
        Assert.Empty(ergebnis.Programmfoerderungen);
    }

    [Fact]
    public async Task FoerderungBerechnen_berechnet_Foerderbetrag_aus_Quote_und_Investitionskosten()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        await db.ErzeugeKostenpositionAsync(alternativeId, 100_000m);

        var programm = await db.ErzeugeFoerderprogrammAsync(foerdersatz: 0.20m);

        await db.Service.ProgrammZuordnenAsync(
            projektId,
            alternativeId,
            programm.Id);

        var ergebnis =
            await db.Service.FoerderungBerechnenAsync(
                projektId,
                alternativeId,
                new DateOnly(2026, 7, 1));

        Assert.NotNull(ergebnis);
        Assert.Equal(100_000m, ergebnis.Investitionskosten);
        Assert.Single(ergebnis.Programmfoerderungen);
        Assert.Equal(20_000m, ergebnis.Programmfoerderungen[0].Foerderbetrag);
        Assert.Equal(20_000m, ergebnis.GesamtFoerderung);
        Assert.Equal(80_000m, ergebnis.Eigenanteil);
    }

    [Fact]
    public async Task FoerderungBerechnen_wendet_Hoechstbetrag_an()
    {
        await using var db = await FoerderungZuordnungTestdatenbank.ErstellenAsync();

        var (projektId, alternativeId) = await db.ErzeugeProjektMitAlternativeAsync();
        await db.ErzeugeKostenpositionAsync(alternativeId, 100_000m);

        var programm = await db.ErzeugeFoerderprogrammAsync(
            foerdersatz: 0.50m,
            hoechstbetrag: 10_000m);

        await db.Service.ProgrammZuordnenAsync(
            projektId,
            alternativeId,
            programm.Id);

        var ergebnis =
            await db.Service.FoerderungBerechnenAsync(
                projektId,
                alternativeId,
                new DateOnly(2026, 7, 1));

        Assert.NotNull(ergebnis);
        Assert.Equal(10_000m, ergebnis.Programmfoerderungen[0].Foerderbetrag);
        Assert.Equal(10_000m, ergebnis.GesamtFoerderung);
        Assert.Equal(90_000m, ergebnis.Eigenanteil);
    }
}

internal sealed class FoerderungZuordnungTestdatenbank : IAsyncDisposable
{
    private FoerderungZuordnungTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context,
        EfAlternativeFoerderungService service)
    {
        Verbindung = verbindung;
        Context = context;
        Service = service;
    }

    private SqliteConnection Verbindung { get; }

    public KompassDbContext Context { get; }

    public EfAlternativeFoerderungService Service { get; }

    public static async Task<FoerderungZuordnungTestdatenbank> ErstellenAsync()
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

        var service = new EfAlternativeFoerderungService(context);

        return new FoerderungZuordnungTestdatenbank(
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

        Context.ChangeTracker.Clear();

        return (projekt.Id, alternativeId);
    }

    public async Task<Foerderprogramm> ErzeugeFoerderprogrammAsync(
        string programmkennung = "BEG EM",
        decimal foerdersatz = 0.15m,
        decimal? hoechstbetrag = null)
    {
        var programm = new Foerderprogramm(
            Guid.NewGuid(),
            programmkennung,
            1,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "Eigentümer",
            "Fenstertausch",
            "U-Wert ≤ 0,95",
            foerdersatz,
            hoechstbetrag,
            "Nicht mit Programm X kumulierbar",
            "Fachunternehmererklärung",
            "BEG 2026");

        Context.Foerderprogramme.Add(programm);

        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        return programm;
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
