using Kompass.Application.B56Import;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kompass.Tests.B56Import;

public sealed class B56KonfliktServiceTests
{
    // ─── Listen: keine Konflikte ─────────────────────────────────────────────

    [Fact]
    public async Task Listen_liefert_leere_Liste_wenn_kein_Vergleich_gespeichert()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.ListenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid());

        Assert.Empty(ergebnis);
    }

    [Fact]
    public async Task Listen_liefert_leere_Liste_wenn_Vergleich_keine_Konflikte_hat()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var (projektId, vorgaengerId, nachfolgerId) =
            await db.SpeichereVergleichAsync(
                konflikte: []);

        var ergebnis =
            await db.Service.ListenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.Empty(ergebnis);
    }

    // ─── Listen: Auto-Initialisierung ────────────────────────────────────────

    [Fact]
    public async Task Listen_initialisiert_Konflikte_aus_gespeichertem_Vergleich()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var konflikt =
            new B56Vergleichskonflikt(
                "Bestandskennwert",
                "Heizwärmebedarf",
                "Wert",
                B56VergleichsAenderung.Geaendert);

        var (projektId, vorgaengerId, nachfolgerId) =
            await db.SpeichereVergleichAsync(
                konflikte: [konflikt],
                bestandskennwerte:
                [
                    new B56KennwertVergleich(
                        "Heizwärmebedarf",
                        "kWh/(m²a)",
                        120.5,
                        98.3,
                        B56VergleichsAenderung.Geaendert)
                ]);

        var ergebnis =
            await db.Service.ListenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.Single(ergebnis);

        var eintrag = ergebnis[0];
        Assert.Equal("Bestandskennwert", eintrag.Bereich);
        Assert.Equal("Heizwärmebedarf", eintrag.Schluessel);
        Assert.Equal("Wert", eintrag.Feld);
        Assert.Equal(B56VergleichsAenderung.Geaendert, eintrag.Aenderung);
        Assert.Equal(B56KonfliktEntscheidungsTyp.Ausstehend, eintrag.Entscheidung);
        Assert.NotNull(eintrag.AlterWert);
        Assert.NotNull(eintrag.NeuerWert);
        Assert.Null(eintrag.EntschiedenAm);
    }

    [Fact]
    public async Task Listen_gibt_vorhandene_Eintraege_zurueck_ohne_neu_zu_initialisieren()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var konflikt =
            new B56Vergleichskonflikt(
                "Bauteil",
                "AW01",
                "UWert/Flaeche",
                B56VergleichsAenderung.Geaendert);

        var (projektId, vorgaengerId, nachfolgerId) =
            await db.SpeichereVergleichAsync(
                konflikte: [konflikt],
                bauteile:
                [
                    new B56BauteilVergleich(
                        "AW01",
                        "Außenwand",
                        0.24, 0.18, 45.0, 45.0,
                        B56VergleichsAenderung.Geaendert)
                ]);

        // Ersten Aufruf: initialisiert
        var ersteAbfrage =
            await db.Service.ListenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.Single(ersteAbfrage);

        // Zweiten Aufruf: gibt vorhandene zurück, keine Doppelten
        var zweiteAbfrage =
            await db.Service.ListenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.Single(zweiteAbfrage);
        Assert.Equal(
            ersteAbfrage[0].KonfliktId,
            zweiteAbfrage[0].KonfliktId);
    }

    // ─── Entscheiden ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Entscheiden_setzt_Status_auf_Akzeptiert()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var (projektId, vorgaengerId, nachfolgerId) =
            await db.SpeichereVergleichAsync(
                konflikte:
                [
                    new B56Vergleichskonflikt(
                        "Bestandskennwert",
                        "Heizwärmebedarf",
                        "Wert",
                        B56VergleichsAenderung.Geaendert)
                ]);

        var konflikte =
            await db.Service.ListenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        var konfliktId = konflikte[0].KonfliktId;

        var ergebnis =
            await db.Service.EntscheidenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId,
                konfliktId,
                B56KonfliktEntscheidungsTyp.Akzeptiert);

        Assert.NotNull(ergebnis);
        Assert.Equal(
            B56KonfliktEntscheidungsTyp.Akzeptiert,
            ergebnis.Entscheidung);
        Assert.NotNull(ergebnis.EntschiedenAm);
    }

    [Fact]
    public async Task Entscheiden_liefert_null_wenn_nicht_gefunden()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var ergebnis =
            await db.Service.EntscheidenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                B56KonfliktEntscheidungsTyp.Abgelehnt);

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task Entscheiden_wirft_Exception_fuer_Ausstehend()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        await Assert.ThrowsAsync<ArgumentException>(
            () => db.Service.EntscheidenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                B56KonfliktEntscheidungsTyp.Ausstehend));
    }

    // ─── Alle akzeptieren ────────────────────────────────────────────────────

    [Fact]
    public async Task AlleAkzeptieren_aktualisiert_alle_ausstehenden_Konflikte()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var (projektId, vorgaengerId, nachfolgerId) =
            await db.SpeichereVergleichAsync(
                konflikte:
                [
                    new B56Vergleichskonflikt(
                        "Bestandskennwert",
                        "K1",
                        "Wert",
                        B56VergleichsAenderung.Geaendert),
                    new B56Vergleichskonflikt(
                        "Bestandskennwert",
                        "K2",
                        "Wert",
                        B56VergleichsAenderung.Geaendert)
                ]);

        // Initialisieren durch Listen
        await db.Service.ListenAsync(
            projektId,
            vorgaengerId,
            nachfolgerId);

        var anzahl =
            await db.Service.AlleAusstehendAkzeptierenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.Equal(2, anzahl);

        var nachher =
            await db.Service.ListenAsync(
                projektId,
                vorgaengerId,
                nachfolgerId);

        Assert.All(
            nachher,
            e => Assert.Equal(
                B56KonfliktEntscheidungsTyp.Akzeptiert,
                e.Entscheidung));
    }

    [Fact]
    public async Task AlleAkzeptieren_gibt_null_zurueck_wenn_keine_ausstehend()
    {
        await using var db = await KonfliktTestdatenbank.ErstellenAsync();

        var anzahl =
            await db.Service.AlleAusstehendAkzeptierenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid());

        Assert.Equal(0, anzahl);
    }

    // ─── Hilfsmethoden ────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private sealed class KonfliktTestdatenbank : IAsyncDisposable
    {
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

            return new KonfliktTestdatenbank(
                verbindung,
                context,
                new EfB56KonfliktService(context));
        }

        public async Task<(Guid ProjektId, Guid VorgaengerId, Guid NachfolgerId)>
            SpeichereVergleichAsync(
                IReadOnlyList<B56Vergleichskonflikt> konflikte,
                IReadOnlyList<B56KennwertVergleich>? bestandskennwerte = null,
                IReadOnlyList<B56BauteilVergleich>? bauteile = null,
                IReadOnlyList<B56AlternativeVergleich>? alternativen = null)
        {
            var projektId = Guid.NewGuid();
            var vorgaengerId = Guid.NewGuid();
            var nachfolgerId = Guid.NewGuid();

            var vergleich = new B56SnapshotVergleich
            {
                ProjektId = projektId,
                VorgaengerSnapshotId = vorgaengerId,
                NachfolgerSnapshotId = nachfolgerId,
                BestandskennwertVergleiche =
                    bestandskennwerte ?? [],
                GesamtbauteilVergleiche =
                    bauteile ?? [],
                AlternativVergleiche =
                    alternativen ?? [],
                Konflikte = konflikte
            };

            Context.B56SnapshotVergleiche.Add(
                new Kompass.Persistence.Data.Entities
                    .B56SnapshotVergleichEntity
                {
                    VergleichId = Guid.NewGuid(),
                    ProjektId = projektId,
                    VorgaengerSnapshotId = vorgaengerId,
                    NachfolgerSnapshotId = nachfolgerId,
                    HatAenderungen = konflikte.Count > 0,
                    VergleichJson =
                        JsonSerializer.Serialize(
                            vergleich,
                            JsonOptionen),
                    ErstelltAm = DateTimeOffset.UtcNow
                });

            await Context.SaveChangesAsync();

            return (projektId, vorgaengerId, nachfolgerId);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Verbindung.DisposeAsync();
        }
    }
}
