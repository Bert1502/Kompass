using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Domain.Verbrauch;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class EfVerbrauchsDatenServiceTests
{
    [Fact]
    public async Task Listen_liefert_leere_Liste_wenn_keine_Daten_vorhanden()
    {
        await using var db = await VerbrauchsTestdatenbank.ErstellenAsync();

        var ergebnis = await db.Service.ListenAsync(Guid.NewGuid());

        Assert.Empty(ergebnis);
    }

    [Fact]
    public async Task Anlegen_und_Listen_gibt_angelegten_Datensatz_zurueck()
    {
        await using var db = await VerbrauchsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            projektId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m);

        var angelegt = await db.Service.AnlegenAsync(daten);

        Assert.NotNull(angelegt);
        Assert.Equal(projektId, angelegt.ProjektId);
        Assert.Equal(Energietraeger.Gas, angelegt.Energietraeger);
        Assert.Equal(12000m, angelegt.Menge);

        var liste = await db.Service.ListenAsync(projektId);

        Assert.Single(liste);
        Assert.Equal(Energietraeger.Gas, liste[0].Energietraeger);
    }

    [Fact]
    public async Task Anlegen_liefert_null_wenn_Projekt_nicht_gefunden()
    {
        await using var db = await VerbrauchsTestdatenbank.ErstellenAsync();

        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m);

        var angelegt = await db.Service.AnlegenAsync(daten);

        Assert.Null(angelegt);
    }

    [Fact]
    public async Task Abrufen_liefert_korrekten_Datensatz()
    {
        await using var db = await VerbrauchsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var id = Guid.NewGuid();
        var daten = new VerbrauchsDaten(
            id,
            projektId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Heizoel,
            8000m,
            900m);

        await db.Service.AnlegenAsync(daten);
        db.Context.ChangeTracker.Clear();

        var gefunden = await db.Service.AbrufenAsync(projektId, id);

        Assert.NotNull(gefunden);
        Assert.Equal(id, gefunden.Id);
        Assert.Equal(Energietraeger.Heizoel, gefunden.Energietraeger);
        Assert.Equal(8000m, gefunden.Menge);
    }

    [Fact]
    public async Task Aktualisieren_speichert_aenderungen()
    {
        await using var db = await VerbrauchsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var id = Guid.NewGuid();
        var daten = new VerbrauchsDaten(
            id,
            projektId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m);

        await db.Service.AnlegenAsync(daten);
        db.Context.ChangeTracker.Clear();

        var zuAktualisieren = await db.Service.AbrufenAsync(projektId, id);
        Assert.NotNull(zuAktualisieren);

        zuAktualisieren.Aktualisieren(
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 12, 31),
            Energietraeger.Heizoel,
            9000m,
            1800m,
            witterungsbereinigungsFaktor: 1.05m,
            flaeche: 150m,
            b56VergleichsWert: 11000m,
            anpassungsFaktor: null,
            anpassungsBegruendung: null,
            abweichungsursache: null);

        var erfolg = await db.Service.AktualisierenAsync(zuAktualisieren);

        Assert.True(erfolg);

        db.Context.ChangeTracker.Clear();
        var aktualisiert = await db.Service.AbrufenAsync(projektId, id);

        Assert.NotNull(aktualisiert);
        Assert.Equal(Energietraeger.Heizoel, aktualisiert.Energietraeger);
        Assert.Equal(9000m, aktualisiert.Menge);
        Assert.Equal(1.05m, aktualisiert.WitterungsbereinigungsFaktor);
        Assert.Equal(150m, aktualisiert.Flaeche);
        Assert.Equal(11000m, aktualisiert.B56VergleichsWert);
    }

    [Fact]
    public async Task Loeschen_entfernt_datensatz()
    {
        await using var db = await VerbrauchsTestdatenbank.ErstellenAsync();

        var projektId = await db.ErzeugeProjektAsync();

        var id = Guid.NewGuid();
        var daten = new VerbrauchsDaten(
            id,
            projektId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m);

        await db.Service.AnlegenAsync(daten);

        var geloescht = await db.Service.LoeschenAsync(projektId, id);

        Assert.True(geloescht);

        var nachLoeschen = await db.Service.AbrufenAsync(projektId, id);

        Assert.Null(nachLoeschen);
    }

    [Fact]
    public async Task Loeschen_liefert_false_wenn_nicht_gefunden()
    {
        await using var db = await VerbrauchsTestdatenbank.ErstellenAsync();

        var geloescht =
            await db.Service.LoeschenAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(geloescht);
    }

    private sealed class VerbrauchsTestdatenbank : IAsyncDisposable
    {
        private VerbrauchsTestdatenbank(
            SqliteConnection verbindung,
            KompassDbContext context,
            EfVerbrauchsDatenService service)
        {
            Verbindung = verbindung;
            Context = context;
            Service = service;
        }

        public KompassDbContext Context { get; }
        public EfVerbrauchsDatenService Service { get; }
        private SqliteConnection Verbindung { get; }

        public static async Task<VerbrauchsTestdatenbank> ErstellenAsync()
        {
            var verbindung = new SqliteConnection("DataSource=:memory:");
            await verbindung.OpenAsync();

            var options =
                new DbContextOptionsBuilder<KompassDbContext>()
                    .UseSqlite(verbindung)
                    .Options;

            var context = new KompassDbContext(options);
            await context.Database.MigrateAsync();

            var service = new EfVerbrauchsDatenService(context);

            return new VerbrauchsTestdatenbank(verbindung, context, service);
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
}
