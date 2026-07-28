using Kompass.Api.Funding;
using Kompass.Application.Funding;
using Kompass.Domain.Common;
using Kompass.Domain.Funding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kompass.Tests.Api;

public sealed class FoerderprogrammeControllerTests
{
    [Fact]
    public async Task Listen_liefert_200_mit_Programmen()
    {
        var programme =
            new List<Foerderprogramm>
            {
                ErzeugeFoerderprogramm()
            };

        var controller = new FoerderprogrammeController(
            new FoerderprogrammServiceFake(programme),
            NullLogger<FoerderprogrammeController>.Instance);

        var antwort =
            await controller.ListenAsync(
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet =
            Assert.IsAssignableFrom<IReadOnlyList<Foerderprogramm>>(ok.Value);

        Assert.Single(gesendet);
    }

    [Fact]
    public async Task Anlegen_liefert_201_mit_gueltigen_Daten()
    {
        var controller = new FoerderprogrammeController(
            new FoerderprogrammServiceFake([]),
            NullLogger<FoerderprogrammeController>.Instance);

        var anfrage = ErzeugeAnfrage();

        var antwort =
            await controller.AnlegenAsync(
                anfrage,
                CancellationToken.None);

        var created =
            Assert.IsType<CreatedAtActionResult>(antwort.Result);

        Assert.Equal(
            StatusCodes.Status201Created,
            created.StatusCode);
        Assert.IsType<Foerderprogramm>(created.Value);
    }

    [Fact]
    public async Task Anlegen_liefert_400_bei_ungueltigem_Zeitraum()
    {
        var controller = new FoerderprogrammeController(
            new FoerderprogrammServiceFake([]),
            NullLogger<FoerderprogrammeController>.Instance);

        var anfrage = ErzeugeAnfrage();
        anfrage.GueltigBis = anfrage.GueltigAb.AddDays(-1);

        var antwort =
            await controller.AnlegenAsync(
                anfrage,
                CancellationToken.None);

        var bad =
            Assert.IsType<BadRequestObjectResult>(antwort.Result);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            bad.StatusCode);
    }

    [Fact]
    public async Task Anlegen_leitet_Servicefehler_als_400_weiter()
    {
        var controller = new FoerderprogrammeController(
            new FoerderprogrammServiceFake(
                [],
                new DomainException("Duplikat")),
            NullLogger<FoerderprogrammeController>.Instance);

        var antwort =
            await controller.AnlegenAsync(
                ErzeugeAnfrage(),
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
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

    private static FoerderprogrammAnlegenRequest ErzeugeAnfrage()
    {
        return new FoerderprogrammAnlegenRequest
        {
            Programmkennung = "BEG EM",
            Version = 1,
            GueltigAb = new DateOnly(2026, 1, 1),
            GueltigBis = new DateOnly(2026, 12, 31),
            Zielgruppe = "Eigentümer",
            Foerdergegenstand = "Fenstertausch",
            TechnischeMindestanforderungen = "U-Wert ≤ 0,95",
            Foerdersatz = 0.15m,
            Hoechstbetrag = 30_000m,
            Kumulierbarkeit = "Nicht mit Programm X kumulierbar",
            Pflichtnachweise = "Fachunternehmererklärung",
            Quellenstand = "BEG 2026"
        };
    }

    private sealed class FoerderprogrammServiceFake(
        IReadOnlyList<Foerderprogramm> programme,
        DomainException? exception = null)
        : IFoerderprogrammService
    {
        public Task<IReadOnlyList<Foerderprogramm>> ListenAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(programme);
        }

        public Task<Foerderprogramm> AnlegenAsync(
            Foerderprogramm foerderprogramm,
            CancellationToken cancellationToken = default)
        {
            if (exception is not null)
            {
                throw exception;
            }

            return Task.FromResult(foerderprogramm);
        }
    }
}
