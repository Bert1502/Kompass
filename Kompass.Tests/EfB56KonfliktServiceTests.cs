using Kompass.Application.B56Import;
using Kompass.Domain.Projects;
using Kompass.Persistence.Data;
using Kompass.Persistence.Data.Entities;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kompass.Tests.Persistence;

public sealed class EfB56KonfliktServiceTests
{
    [Fact]
    public async Task ListenOderErzeugen_liefert_leere_Liste_wenn_kein_Vergleich_gespeichert()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var ergebnis = await db.Service.ListenOderErzeugenAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.Empty(ergebnis);
    }

    [Fact]
    public async Task ListenOderErzeugen_erstellt_Eintraege_aus_gespeichertem_Vergleich()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var projektId = Guid.NewGuid();
        var vorgaenger = Guid.NewGuid();
        var nachfolger = Guid.NewGuid();

        var vergleich = new B56SnapshotVergleich
        {
            ProjektId = projektId,
            VorgaengerSnapshotId = vorgaenger,
            NachfolgerSnapshotId = nachfolger,
            Konflikte =
            [
                new B56Vergleichskonflikt(
                    "Bestandskennwert",
                    "Heizwärmebedarf",
                    "Wert",
                    B56VergleichsAenderung.Geaendert),
                new B56Vergleichskonflikt(
                    "Bauteil",
                    "AW01",
                    "UWert/Flaeche",
                    B56VergleichsAenderung.Geaendert)
            ]
        };

        await db.SpeichereVergleichAsync(vergleich);

        var ergebnis = await db.Service.ListenOderErzeugenAsync(
            projektId,
            vorgaenger,
            nachfolger);

        Assert.Equal(2, ergebnis.Count);
        Assert.All(
            ergebnis,
            e =>
            {
                Assert.Equal(projektId, e.ProjektId);
                Assert.Equal(vorgaenger, e.VorgaengerImportId);
                Assert.Equal(nachfolger, e.NachfolgerImportId);
                Assert.Equal(B56KonfliktEntscheidungsTyp.Offen, e.Entscheidung);
                Assert.Null(e.EntschiedenAm);
            });

        var bestandskennwert =
            ergebnis.Single(e => e.Bereich == "Bestandskennwert");
        Assert.Equal("Heizwärmebedarf", bestandskennwert.Schluessel);
    }

    [Fact]
    public async Task ListenOderErzeugen_liefert_vorhandene_Eintraege_ohne_Duplikate()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var projektId = Guid.NewGuid();
        var vorgaenger = Guid.NewGuid();
        var nachfolger = Guid.NewGuid();

        var vergleich = new B56SnapshotVergleich
        {
            ProjektId = projektId,
            VorgaengerSnapshotId = vorgaenger,
            NachfolgerSnapshotId = nachfolger,
            Konflikte =
            [
                new B56Vergleichskonflikt(
                    "Bestandskennwert",
                    "Heizwärmebedarf",
                    "Wert",
                    B56VergleichsAenderung.Geaendert)
            ]
        };

        await db.SpeichereVergleichAsync(vergleich);

        var erstesLaden =
            await db.Service.ListenOderErzeugenAsync(
                projektId,
                vorgaenger,
                nachfolger);

        var zweitesLaden =
            await db.Service.ListenOderErzeugenAsync(
                projektId,
                vorgaenger,
                nachfolger);

        Assert.Single(erstesLaden);
        Assert.Single(zweitesLaden);
        Assert.Equal(erstesLaden[0].Id, zweitesLaden[0].Id);
    }

    [Fact]
    public async Task EntscheidungSetzen_setzt_Entscheidung_korrekt()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var projektId = Guid.NewGuid();
        var vorgaenger = Guid.NewGuid();
        var nachfolger = Guid.NewGuid();

        var vergleich = new B56SnapshotVergleich
        {
            ProjektId = projektId,
            VorgaengerSnapshotId = vorgaenger,
            NachfolgerSnapshotId = nachfolger,
            Konflikte =
            [
                new B56Vergleichskonflikt(
                    "Bestandskennwert",
                    "Heizwärmebedarf",
                    "Wert",
                    B56VergleichsAenderung.Geaendert)
            ]
        };

        await db.SpeichereVergleichAsync(vergleich);

        var eintraege =
            await db.Service.ListenOderErzeugenAsync(
                projektId,
                vorgaenger,
                nachfolger);

        var id = eintraege[0].Id;

        var gesetzt =
            await db.Service.EntscheidungSetzenAsync(
                projektId,
                nachfolger,
                id,
                B56KonfliktEntscheidungsTyp.Uebernehmen);

        Assert.True(gesetzt);

        var aktualisiert =
            await db.Service.ListenOderErzeugenAsync(
                projektId,
                vorgaenger,
                nachfolger);

        Assert.Equal(
            B56KonfliktEntscheidungsTyp.Uebernehmen,
            aktualisiert[0].Entscheidung);
        Assert.NotNull(aktualisiert[0].EntschiedenAm);
    }

    [Fact]
    public async Task EntscheidungSetzen_liefert_false_wenn_nicht_gefunden()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var gefunden =
            await db.Service.EntscheidungSetzenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                B56KonfliktEntscheidungsTyp.Behalten);

        Assert.False(gefunden);
    }

    [Fact]
    public async Task ListenOderErzeugen_liefert_leere_Liste_wenn_Vergleich_keine_Konflikte_hat()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var projektId = Guid.NewGuid();
        var vorgaenger = Guid.NewGuid();
        var nachfolger = Guid.NewGuid();

        var vergleich = new B56SnapshotVergleich
        {
            ProjektId = projektId,
            VorgaengerSnapshotId = vorgaenger,
            NachfolgerSnapshotId = nachfolger,
            Konflikte = []
        };

        await db.SpeichereVergleichAsync(vergleich);

        var ergebnis =
            await db.Service.ListenOderErzeugenAsync(
                projektId,
                vorgaenger,
                nachfolger);

        Assert.Empty(ergebnis);
    }

    private sealed class KonfliktTestdatenbank : IAsyncDisposable
    {
        private static readonly JsonSerializerOptions JsonOptionen =
            new(JsonSerializerDefaults.Web);

        private KonfliktTestdatenbank(
            SqliteConnection verbindung,
            KompassDbContext context,
            EfB56KonfliktService service)
        {
            Verbindung = verbindung;
            Context = context;
            Service = service;
        }

        public KompassDbContext Context { get; }
        public EfB56KonfliktService Service { get; }
        private SqliteConnection Verbindung { get; }

        public static async Task<KonfliktTestdatenbank> ErstellenAsync()
        {
            var verbindung =
                new SqliteConnection("DataSource=:memory:");
            await verbindung.OpenAsync();

            var options =
                new DbContextOptionsBuilder<KompassDbContext>()
                    .UseSqlite(verbindung)
                    .Options;

            var context = new KompassDbContext(options);
            await context.Database.MigrateAsync();

            var service = new EfB56KonfliktService(context);

            return new KonfliktTestdatenbank(
                verbindung, context, service);
        }

        public async Task SpeichereVergleichAsync(
            B56SnapshotVergleich vergleich)
        {
            Context.B56SnapshotVergleiche.Add(
                new B56SnapshotVergleichEntity
                {
                    VergleichId = Guid.NewGuid(),
                    ProjektId = vergleich.ProjektId,
                    VorgaengerSnapshotId =
                        vergleich.VorgaengerSnapshotId,
                    NachfolgerSnapshotId =
                        vergleich.NachfolgerSnapshotId,
                    HatAenderungen = vergleich.HatAenderungen,
                    VergleichJson =
                        JsonSerializer.Serialize(
                            vergleich,
                            JsonOptionen),
                    ErstelltAm = DateTimeOffset.UtcNow
                });

            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Verbindung.DisposeAsync();
        }
    }
}
