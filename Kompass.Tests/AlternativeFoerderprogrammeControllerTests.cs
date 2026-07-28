using Kompass.Api.Funding;
using Kompass.Application.Funding;
using Kompass.Domain.Funding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.Api;

public sealed class AlternativeFoerderprogrammeControllerTests
{
    [Fact]
    public async Task Listen_liefert_200_mit_zugeordneten_Programmen()
    {
        var programm = ErzeugeFoerderprogramm();

        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                zugeordneteProgramme: [programm]));

        var antwort =
            await controller.ListenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet =
            Assert.IsAssignableFrom<IReadOnlyList<Foerderprogramm>>(ok.Value);

        Assert.Single(gesendet);
    }

    [Fact]
    public async Task Zuordnen_liefert_204_wenn_erfolgreich()
    {
        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                zuordnenErgebnis: true));

        var antwort =
            await controller.ZuordnenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NoContentResult>(antwort);
    }

    [Fact]
    public async Task Zuordnen_liefert_404_wenn_Alternative_oder_Programm_nicht_gefunden()
    {
        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                zuordnenErgebnis: false));

        var antwort =
            await controller.ZuordnenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        var result = Assert.IsType<NotFoundObjectResult>(antwort);

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    [Fact]
    public async Task Entfernen_liefert_204_wenn_erfolgreich()
    {
        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                entfernenErgebnis: true));

        var antwort =
            await controller.EntfernenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NoContentResult>(antwort);
    }

    [Fact]
    public async Task Entfernen_liefert_404_wenn_Zuordnung_nicht_gefunden()
    {
        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                entfernenErgebnis: false));

        var antwort =
            await controller.EntfernenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort);
    }

    [Fact]
    public async Task Berechnen_liefert_200_mit_Ergebnis()
    {
        var ergebnis = new Foerderberechnungsergebnis(
            new DateOnly(2026, 7, 1),
            100_000m,
            [new ProgrammFoerderungsanteil(
                Guid.NewGuid(),
                "BEG EM",
                1,
                15_000m,
                KumulierbarkeitStatus.Unbestimmt)],
            15_000m,
            85_000m);

        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                berechnungsErgebnis: ergebnis));

        var antwort =
            await controller.BerechnenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2026, 7, 1),
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet = Assert.IsType<Foerderberechnungsergebnis>(ok.Value);

        Assert.Equal(100_000m, gesendet.Investitionskosten);
        Assert.Equal(15_000m, gesendet.GesamtFoerderung);
        Assert.Single(gesendet.Programmfoerderungen);
    }

    [Fact]
    public async Task Berechnen_liefert_404_wenn_Alternative_nicht_gefunden()
    {
        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                berechnungsErgebnis: null));

        var antwort =
            await controller.BerechnenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Berechnen_verwendet_aktuelles_Datum_wenn_kein_Stichtag_angegeben()
    {
        DateOnly? verwendeterStichtag = null;

        var controller = new AlternativeFoerderprogrammeController(
            new AlternativeFoerderungServiceFake(
                berechnungsErgebnis: new Foerderberechnungsergebnis(
                    DateOnly.FromDateTime(DateTime.UtcNow),
                    0m,
                    [],
                    0m,
                    0m),
                stichtagCallback: s => verwendeterStichtag = s));

        await controller.BerechnenAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            CancellationToken.None);

        Assert.NotNull(verwendeterStichtag);

        var heute = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.True(
            verwendeterStichtag >= heute.AddDays(-1) &&
            verwendeterStichtag <= heute.AddDays(1),
            $"Erwarteter Stichtag nahe {heute}, erhalten: {verwendeterStichtag}");
    }

    private static Foerderprogramm ErzeugeFoerderprogramm()
    {
        return new Foerderprogramm(
            Guid.NewGuid(),
            "BEG EM",
            1,
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

    private sealed class AlternativeFoerderungServiceFake(
        IReadOnlyList<Foerderprogramm>? zugeordneteProgramme = null,
        bool zuordnenErgebnis = true,
        bool entfernenErgebnis = true,
        Foerderberechnungsergebnis? berechnungsErgebnis = null,
        Action<DateOnly>? stichtagCallback = null)
        : IAlternativeFoerderungService
    {
        public Task<IReadOnlyList<Foerderprogramm>> ZugeordneteProgrammeListenAsync(
            Guid projektId,
            Guid alternativeId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Foerderprogramm> result = zugeordneteProgramme ?? [];
            return Task.FromResult(result);
        }

        public Task<bool> ProgrammZuordnenAsync(
            Guid projektId,
            Guid alternativeId,
            Guid foerderprogrammId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(zuordnenErgebnis);
        }

        public Task<bool> ProgrammEntfernenAsync(
            Guid projektId,
            Guid alternativeId,
            Guid foerderprogrammId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(entfernenErgebnis);
        }

        public Task<Foerderberechnungsergebnis?> FoerderungBerechnenAsync(
            Guid projektId,
            Guid alternativeId,
            DateOnly stichtag,
            CancellationToken cancellationToken = default)
        {
            stichtagCallback?.Invoke(stichtag);
            return Task.FromResult(berechnungsErgebnis);
        }
    }
}
