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
        var foerderprogramm = Assert.IsType<Foerderprogramm>(created.Value);

        Assert.Single(foerderprogramm.Foerderquoten);
        Assert.Single(foerderprogramm.Kumulierbarkeitsregeln);
    }

    [Fact]
    public async Task Anlegen_uebernimmt_feine_Foerderregeln_aus_der_Anfrage()
    {
        var controller = new FoerderprogrammeController(
            new FoerderprogrammServiceFake([]),
            NullLogger<FoerderprogrammeController>.Instance);

        var anfrage = ErzeugeAnfrage();
        anfrage.Foerderquoten =
        [
            new FoerderquoteRegelRequest
            {
                Bezeichnung = "Bonusquote",
                Quote = 0.2m,
                Bezugsbasis = "förderfähige Kosten",
                GueltigAb = new DateOnly(2026, 2, 1),
                Beschreibung = "Mit iSFP-Bonus"
            }
        ];
        anfrage.Hoechstbetraege =
        [
            new HoechstbetragRegelRequest
            {
                Bezeichnung = "Deckel",
                Betrag = 60_000m,
                Waehrung = "EUR",
                Bezugsbasis = "je Wohneinheit",
                GueltigAb = new DateOnly(2026, 2, 1),
                Beschreibung = "Nur bei Komplettsanierung"
            }
        ];
        anfrage.Kumulierbarkeitsregeln =
        [
            new KumulierbarkeitsregelRequest
            {
                Bezeichnung = "Landesprogramm",
                Status = KumulierbarkeitStatus.BedingtKumulierbar,
                Beschreibung = "Nur mit Landesmitteln kombinierbar.",
                GueltigAb = new DateOnly(2026, 2, 1)
            }
        ];
        anfrage.Pflichtnachweisregeln =
        [
            new PflichtnachweisRegelRequest
            {
                Bezeichnung = "iSFP",
                Beschreibung = "Vorlage des Sanierungsfahrplans",
                Zeitpunkt = Nachweiszeitpunkt.BeiAntrag,
                IstPflicht = true,
                GueltigAb = new DateOnly(2026, 2, 1)
            }
        ];
        anfrage.Gueltigkeitsregeln =
        [
            new GueltigkeitsregelRequest
            {
                Bezeichnung = "Antragsfenster 2026",
                Bezug = Gueltigkeitsbezug.Antragsdatum,
                GueltigAb = new DateOnly(2026, 2, 1),
                GueltigBis = new DateOnly(2026, 11, 30),
                Beschreibung = "Nur für 2026."
            }
        ];

        var antwort =
            await controller.AnlegenAsync(
                anfrage,
                CancellationToken.None);

        var created =
            Assert.IsType<CreatedAtActionResult>(antwort.Result);
        var foerderprogramm =
            Assert.IsType<Foerderprogramm>(created.Value);

        Assert.Equal("Bonusquote", Assert.Single(foerderprogramm.Foerderquoten).Bezeichnung);
        Assert.Equal("Deckel", Assert.Single(foerderprogramm.Hoechstbetraege).Bezeichnung);
        Assert.Equal(KumulierbarkeitStatus.BedingtKumulierbar, Assert.Single(foerderprogramm.Kumulierbarkeitsregeln).Status);
        Assert.Equal(Nachweiszeitpunkt.BeiAntrag, Assert.Single(foerderprogramm.Pflichtnachweisregeln).Zeitpunkt);
        Assert.Equal(Gueltigkeitsbezug.Antragsdatum, Assert.Single(foerderprogramm.Gueltigkeitsregeln).Bezug);
    }

    [Fact]
    public async Task Anlegen_behandelt_null_Collections_wie_leere_Regellisten()
    {
        var controller = new FoerderprogrammeController(
            new FoerderprogrammServiceFake([]),
            NullLogger<FoerderprogrammeController>.Instance);

        var anfrage = ErzeugeAnfrage();
        anfrage.Foerderquoten = null!;
        anfrage.Hoechstbetraege = null!;
        anfrage.Kumulierbarkeitsregeln = null!;
        anfrage.Pflichtnachweisregeln = null!;
        anfrage.Gueltigkeitsregeln = null!;

        var antwort =
            await controller.AnlegenAsync(
                anfrage,
                CancellationToken.None);

        var created =
            Assert.IsType<CreatedAtActionResult>(antwort.Result);
        var foerderprogramm =
            Assert.IsType<Foerderprogramm>(created.Value);

        Assert.Single(foerderprogramm.Foerderquoten);
        Assert.Single(foerderprogramm.Kumulierbarkeitsregeln);
        Assert.Single(foerderprogramm.Pflichtnachweisregeln);
        Assert.Single(foerderprogramm.Gueltigkeitsregeln);
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
