using Kompass.Domain.Common;
using Kompass.Domain.Funding;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class EfFoerderprogrammServiceTests
{
    [Fact]
    public async Task Listen_liefert_Programme_sortiert_nach_Kennung_und_Version()
    {
        await using var db = await FoerderprogrammTestdatenbank.ErstellenAsync();

        await db.Service.AnlegenAsync(
            ErzeugeFoerderprogramm("KfW", 2));
        await db.Service.AnlegenAsync(
            ErzeugeFoerderprogramm("BEG EM", 1));
        await db.Service.AnlegenAsync(
            ErzeugeFoerderprogramm("KfW", 1));

        var programme = await db.Service.ListenAsync();

        Assert.Collection(
            programme,
            programm =>
            {
                Assert.Equal("BEG EM", programm.Programmkennung);
                Assert.Equal(1, programm.Version);
            },
            programm =>
            {
                Assert.Equal("KfW", programm.Programmkennung);
                Assert.Equal(1, programm.Version);
            },
            programm =>
            {
                Assert.Equal("KfW", programm.Programmkennung);
                Assert.Equal(2, programm.Version);
            });
    }

    [Fact]
    public async Task Anlegen_speichert_Foerderprogramm()
    {
        await using var db = await FoerderprogrammTestdatenbank.ErstellenAsync();

        var programm = ErzeugeFoerderprogramm();

        var gespeichert = await db.Service.AnlegenAsync(programm);

        var ausDb = await db.Context.Foerderprogramme.SingleAsync();

        Assert.Equal(gespeichert.Id, ausDb.Id);
        Assert.Equal("BEG EM", ausDb.Programmkennung);
        Assert.Equal(0.15m, ausDb.Foerdersatz);
        Assert.Single(ausDb.Foerderquoten);
        Assert.Single(ausDb.Hoechstbetraege);
        Assert.Single(ausDb.Kumulierbarkeitsregeln);
        Assert.Single(ausDb.Pflichtnachweisregeln);
        Assert.Single(ausDb.Gueltigkeitsregeln);
    }

    [Fact]
    public async Task Anlegen_speichert_feinere_Foerderregeln()
    {
        await using var db = await FoerderprogrammTestdatenbank.ErstellenAsync();

        var programm = new Foerderprogramm(
            Guid.NewGuid(),
            "BEG EM",
            3,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "Eigentümer",
            "Fenstertausch",
            "U-Wert ≤ 0,95",
            0.15m,
            30_000m,
            "Nur mit Landesmitteln",
            "Fachunternehmererklärung",
            "BEG 2026",
            [
                new FoerderquoteRegel(
                    Guid.NewGuid(),
                    "Bonusquote",
                    0.2m,
                    "förderfähige Kosten",
                    new DateOnly(2026, 2, 1),
                    null,
                    "Mit iSFP-Bonus")
            ],
            [
                new HoechstbetragRegel(
                    Guid.NewGuid(),
                    "Deckel",
                    60_000m,
                    "EUR",
                    "je Wohneinheit",
                    new DateOnly(2026, 2, 1),
                    null,
                    "Nur bei Komplettsanierung")
            ],
            [
                new Kumulierbarkeitsregel(
                    Guid.NewGuid(),
                    "Landesprogramm",
                    KumulierbarkeitStatus.BedingtKumulierbar,
                    "Nur mit Landesmitteln kombinierbar.",
                    new DateOnly(2026, 2, 1),
                    null)
            ],
            [
                new PflichtnachweisRegel(
                    Guid.NewGuid(),
                    "iSFP",
                    "Vorlage des Sanierungsfahrplans",
                    Nachweiszeitpunkt.BeiAntrag,
                    true,
                    new DateOnly(2026, 2, 1),
                    null)
            ],
            [
                new Gueltigkeitsregel(
                    Guid.NewGuid(),
                    "Antragsfenster 2026",
                    Gueltigkeitsbezug.Antragsdatum,
                    new DateOnly(2026, 2, 1),
                    new DateOnly(2026, 11, 30),
                    "Nur für 2026.")
            ]);

        await db.Service.AnlegenAsync(programm);

        var ausDb = await db.Context.Foerderprogramme.SingleAsync();

        Assert.Equal("Bonusquote", Assert.Single(ausDb.Foerderquoten).Bezeichnung);
        Assert.Equal("je Wohneinheit", Assert.Single(ausDb.Hoechstbetraege).Bezugsbasis);
        Assert.Equal(KumulierbarkeitStatus.BedingtKumulierbar, Assert.Single(ausDb.Kumulierbarkeitsregeln).Status);
        Assert.Equal(Nachweiszeitpunkt.BeiAntrag, Assert.Single(ausDb.Pflichtnachweisregeln).Zeitpunkt);
        Assert.Equal(Gueltigkeitsbezug.Antragsdatum, Assert.Single(ausDb.Gueltigkeitsregeln).Bezug);
    }

    [Fact]
    public async Task Anlegen_lehnt_doppelte_Programmkennung_und_Version_ab()
    {
        await using var db = await FoerderprogrammTestdatenbank.ErstellenAsync();

        await db.Service.AnlegenAsync(
            ErzeugeFoerderprogramm("BEG EM", 1));

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => db.Service.AnlegenAsync(
                ErzeugeFoerderprogramm("BEG EM", 1)));

        Assert.Contains("BEG EM", exception.Message);
    }

    private static Foerderprogramm ErzeugeFoerderprogramm(
        string programmkennung = "BEG EM",
        int version = 1)
    {
        return new Foerderprogramm(
            Guid.NewGuid(),
            programmkennung,
            version,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "Eigentümer",
            "Fenstertausch",
            "U-Wert ≤ 0,95",
            0.15m,
            30_000m,
            "Nicht mit Programm X kumulierbar",
            "Fachunternehmererklärung",
            "BEG 2026");
    }
}

internal sealed class FoerderprogrammTestdatenbank : IAsyncDisposable
{
    private FoerderprogrammTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context,
        EfFoerderprogrammService service)
    {
        Verbindung = verbindung;
        Context = context;
        Service = service;
    }

    private SqliteConnection Verbindung { get; }

    public KompassDbContext Context { get; }

    public EfFoerderprogrammService Service { get; }

    public static async Task<FoerderprogrammTestdatenbank> ErstellenAsync()
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

        var service = new EfFoerderprogrammService(context);

        return new FoerderprogrammTestdatenbank(
            verbindung,
            context,
            service);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Verbindung.DisposeAsync();
    }
}
