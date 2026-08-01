using Kompass.Application.Referenzdaten;
using Kompass.Domain.Referenzdaten;
using Kompass.Persistence.Data;
using Kompass.Persistence.Services.Referenzdaten;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class EfReferenzdatenServiceTests
{
    [Fact]
    public async Task WertAufloesen_bevorzugt_Projektspezifischen_freigegebenen_Wert()
    {
        await using var db = await ReferenzdatenTestdatenbank.ErstellenAsync();

        var projektId = Guid.NewGuid();
        var unternehmenId = Guid.NewGuid();

        db.Context.Referenzdatensaetze.AddRange(
            ErzeugeDatensatz("Diskontierungszinssatz", "0.03", ReferenzdatenEbene.Systemweit, Qualitaetsstatus.OffizielleQuelle),
            ErzeugeDatensatz("Diskontierungszinssatz", "0.04", ReferenzdatenEbene.Unternehmensweit, Qualitaetsstatus.UnternehmensinternerWert, unternehmenId: unternehmenId),
            ErzeugeDatensatz("Diskontierungszinssatz", "0.05", ReferenzdatenEbene.Projektspezifisch, Qualitaetsstatus.ProjektspezifischeAnnahme, projektId: projektId));

        await db.Context.SaveChangesAsync();

        var wert = await db.Service.WertAufloesenAsync(
            new ReferenzwertAnfrage(
                "Diskontierungszinssatz",
                ProjektId: projektId,
                UnternehmenId: unternehmenId));

        Assert.NotNull(wert);
        Assert.Equal("0.05", wert.Datensatz.Wert);
        Assert.Equal(ReferenzdatenPrioritaet.ProjektspezifischFreigegeben, wert.Prioritaet);
    }

    [Fact]
    public async Task WertAufloesen_nimmt_lokalen_Ersatzwert_als_letzten_Fallback()
    {
        await using var db = await ReferenzdatenTestdatenbank.ErstellenAsync();

        db.Context.Referenzdatensaetze.Add(
            ErzeugeDatensatz(
                "CO2-Preis",
                "55",
                ReferenzdatenEbene.Systemweit,
                Qualitaetsstatus.Ersatzwert,
                gueltigAb: new DateOnly(2025, 1, 1),
                gueltigBis: new DateOnly(2025, 12, 31),
                datenstatus: ReferenzdatenStatus.Veraltet));

        await db.Context.SaveChangesAsync();

        var wert = await db.Service.WertAufloesenAsync(
            new ReferenzwertAnfrage(
                "CO2-Preis",
                Stichtag: new DateOnly(2026, 1, 1)));

        Assert.NotNull(wert);
        Assert.Equal("55", wert.Datensatz.Wert);
        Assert.Equal(ReferenzdatenPrioritaet.LokalerErsatzwert, wert.Prioritaet);
    }

    [Fact]
    public async Task Synchronisieren_verwendet_lokalen_Bestand_bei_Providerfehler()
    {
        await using var db = await ReferenzdatenTestdatenbank.ErstellenAsync(
            providers:
            [
                new ThrowingProvider(),
                new StaticProvider([
                    new ReferenzdatenImportEintrag(
                        "Inflation",
                        "Inflationsrate",
                        "0.02",
                        ReferenzdatenEbene.Systemweit,
                        "Quelle",
                        "Herausgeber",
                        "https://example.invalid",
                        new DateOnly(2026, 1, 1),
                        null,
                        "2026-01",
                        ReferenzdatenStatus.Freigegeben,
                        Qualitaetsstatus.OffizielleQuelle,
                        ReferenzdatenImportart.AutomatischerAbruf,
                        DateTimeOffset.UtcNow)
                ])
            ]);

        db.Context.Referenzdatensaetze.Add(
            ErzeugeDatensatz("Bestand", "1", ReferenzdatenEbene.Systemweit, Qualitaetsstatus.NichtVerifiziert));

        await db.Context.SaveChangesAsync();

        var result = await db.Service.SynchronisierenAsync();

        Assert.True(result.LokalerFallbackVerwendet);
        Assert.Equal(2, result.ProviderErgebnisse.Count);
        Assert.True(result.ProviderErgebnisse.Any(e => e.ProviderName == "throwing" && e.Fehler is not null));
        Assert.True(result.ProviderErgebnisse.Any(e => e.ProviderName == "static" && e.ImportierteDatensaetze == 1));
    }

    private static Referenzdatensatz ErzeugeDatensatz(
        string parameterart,
        string wert,
        ReferenzdatenEbene ebene,
        Qualitaetsstatus qualitaetsstatus,
        Guid? projektId = null,
        Guid? unternehmenId = null,
        DateOnly? gueltigAb = null,
        DateOnly? gueltigBis = null,
        ReferenzdatenStatus datenstatus = ReferenzdatenStatus.Freigegeben)
    {
        return new Referenzdatensatz(
            Guid.NewGuid(),
            parameterart,
            parameterart,
            wert,
            ebene,
            "Quelle",
            "Herausgeber",
            "https://example.invalid",
            gueltigAb ?? new DateOnly(2026, 1, 1),
            gueltigBis,
            "v1",
            datenstatus,
            qualitaetsstatus,
            ReferenzdatenImportart.ManuellePflege,
            DateTimeOffset.UtcNow,
            projektId: projektId,
            unternehmenId: unternehmenId);
    }

    private sealed class ThrowingProvider : IReferenzdatenProvider
    {
        public string ProviderName => "throwing";

        public Task<IReadOnlyList<ReferenzdatenImportEintrag>> LadeReferenzdatenAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Quelle nicht erreichbar");
        }
    }

    private sealed class StaticProvider(IReadOnlyList<ReferenzdatenImportEintrag> data) : IReferenzdatenProvider
    {
        public string ProviderName => "static";

        public Task<IReadOnlyList<ReferenzdatenImportEintrag>> LadeReferenzdatenAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(data);
        }
    }
}

internal sealed class ReferenzdatenTestdatenbank : IAsyncDisposable
{
    private ReferenzdatenTestdatenbank(
        SqliteConnection verbindung,
        KompassDbContext context,
        EfReferenzdatenService service)
    {
        Verbindung = verbindung;
        Context = context;
        Service = service;
    }

    private SqliteConnection Verbindung { get; }

    public KompassDbContext Context { get; }

    public EfReferenzdatenService Service { get; }

    public static async Task<ReferenzdatenTestdatenbank> ErstellenAsync(
        IReadOnlyList<IReferenzdatenProvider>? providers = null)
    {
        var verbindung = new SqliteConnection("Data Source=:memory:");
        await verbindung.OpenAsync();

        var options = new DbContextOptionsBuilder<KompassDbContext>()
            .UseSqlite(verbindung)
            .Options;

        var context = new KompassDbContext(options);
        await context.Database.MigrateAsync();

        var service = new EfReferenzdatenService(context, providers ?? []);

        return new ReferenzdatenTestdatenbank(verbindung, context, service);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Verbindung.DisposeAsync();
    }
}
