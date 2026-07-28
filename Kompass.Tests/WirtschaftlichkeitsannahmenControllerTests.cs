using Kompass.Api.Economics;
using Kompass.Application.Economics;
using Kompass.Domain.Common;
using Kompass.Domain.Economics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kompass.Tests.Api;

public sealed class WirtschaftlichkeitsannahmenControllerTests
{
    private static readonly Guid ProjektId = Guid.NewGuid();
    private static readonly Guid AlternativeId = Guid.NewGuid();

    [Fact]
    public async Task AnnahmenAbrufen_liefert_200_mit_vorhandenen_Annahmen()
    {
        var annahmen = ErstelleStandardAnnahmen();

        var controller = new WirtschaftlichkeitsannahmenController(
            new WirtschaftlichkeitsServiceFake(annahmen),
            NullLogger<WirtschaftlichkeitsannahmenController>.Instance);

        var antwort =
            await controller.AnnahmenAbrufenAsync(
                ProjektId,
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);

        Assert.IsType<Wirtschaftlichkeitsannahmen>(ok.Value);
    }

    [Fact]
    public async Task AnnahmenAbrufen_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new WirtschaftlichkeitsannahmenController(
            new WirtschaftlichkeitsServiceFake(null),
            NullLogger<WirtschaftlichkeitsannahmenController>.Instance);

        var antwort =
            await controller.AnnahmenAbrufenAsync(
                ProjektId,
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task AnnahmenSpeichern_liefert_200_mit_gueltigen_Daten()
    {
        var annahmen = ErstelleStandardAnnahmen();

        var controller = new WirtschaftlichkeitsannahmenController(
            new WirtschaftlichkeitsServiceFake(annahmen),
            NullLogger<WirtschaftlichkeitsannahmenController>.Instance);

        var anfrage = new WirtschaftlichkeitsannahmenSetzenAnfrage
        {
            Betrachtungszeitraum = 20,
            Diskontsatz = 0.04m,
            Inflationsrate = 0.02m,
            JaehrlicheWartungsmehrkosten = 0m,
            Nutzungsdauer = 20,
            Foerderung = 0m,
            EnergietraegerAnnahmen = []
        };

        var antwort =
            await controller.AnnahmenSpeichernAsync(
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                anfrage,
                CancellationToken.None);

        Assert.IsType<OkObjectResult>(antwort.Result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task AnnahmenSpeichern_liefert_400_bei_ungueltigem_Betrachtungszeitraum(
        int betrachtungszeitraum)
    {
        var controller = new WirtschaftlichkeitsannahmenController(
            new WirtschaftlichkeitsServiceFake(null),
            NullLogger<WirtschaftlichkeitsannahmenController>.Instance);

        var anfrage = new WirtschaftlichkeitsannahmenSetzenAnfrage
        {
            Betrachtungszeitraum = betrachtungszeitraum,
            Diskontsatz = 0.04m,
            Inflationsrate = 0.02m,
            JaehrlicheWartungsmehrkosten = 0m,
            Nutzungsdauer = 20,
            Foerderung = 0m
        };

        var antwort =
            await controller.AnnahmenSpeichernAsync(
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                anfrage,
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Berechnen_liefert_200_mit_Ergebnis()
    {
        var ergebnis = new Wirtschaftlichkeitsergebnis(
            Eigenanteil: 10_000m,
            JaehrlicheEnergiekosteneinsparungJahr1: 1_000m,
            KumulierteEnergiekosteneinsparung: 20_000m,
            AmortisationsdauerStatisch: 10m,
            AmortisationsdauerDynamisch: 11m,
            Kapitalwert: 3_600m,
            KostenNutzenVerhaeltnis: 2m);

        var controller = new WirtschaftlichkeitsannahmenController(
            new WirtschaftlichkeitsServiceFake(null, ergebnis),
            NullLogger<WirtschaftlichkeitsannahmenController>.Instance);

        var antwort =
            await controller.BerechnenAsync(
                ProjektId,
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);

        var gesendetes =
            Assert.IsType<Wirtschaftlichkeitsergebnis>(ok.Value);

        Assert.Equal(10_000m, gesendetes.Eigenanteil);
        Assert.Equal(1_000m, gesendetes.JaehrlicheEnergiekosteneinsparungJahr1);
        Assert.Equal(2m, gesendetes.KostenNutzenVerhaeltnis);
    }

    [Fact]
    public async Task Berechnen_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new WirtschaftlichkeitsannahmenController(
            new WirtschaftlichkeitsServiceFake(null, null),
            NullLogger<WirtschaftlichkeitsannahmenController>.Instance);

        var antwort =
            await controller.BerechnenAsync(
                ProjektId,
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task AnnahmenSpeichern_leitet_DomainException_als_400_weiter()
    {
        var controller = new WirtschaftlichkeitsannahmenController(
            new WirtschaftlichkeitsServiceFake(null),
            NullLogger<WirtschaftlichkeitsannahmenController>.Instance);

        var anfrage = new WirtschaftlichkeitsannahmenSetzenAnfrage
        {
            Betrachtungszeitraum = 20,
            Diskontsatz = -0.01m,
            Inflationsrate = 0.02m,
            JaehrlicheWartungsmehrkosten = 0m,
            Nutzungsdauer = 20,
            Foerderung = 0m
        };

        var antwort =
            await controller.AnnahmenSpeichernAsync(
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                anfrage,
                CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(antwort.Result);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            bad.StatusCode);
    }

    private static Wirtschaftlichkeitsannahmen ErstelleStandardAnnahmen()
    {
        return new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            AlternativeId,
            WirtschaftlichkeitsBasis.Bilanziert,
            betrachtungszeitraum: 20,
            diskontsatz: 0.04m,
            inflationsrate: 0.02m,
            jaehrlicheWartungsmehrkosten: 0m,
            nutzungsdauer: 20,
            foerderung: 0m);
    }

    private sealed class WirtschaftlichkeitsServiceFake(
        Wirtschaftlichkeitsannahmen? annahmen,
        Wirtschaftlichkeitsergebnis? ergebnis = null)
        : IWirtschaftlichkeitsService
    {
        public Task<Wirtschaftlichkeitsannahmen?> AnnahmenAbrufenAsync(
            Guid projektId,
            Guid alternativeId,
            WirtschaftlichkeitsBasis basis,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(annahmen);
        }

        public Task<Wirtschaftlichkeitsannahmen> AnnahmenSpeichernAsync(
            Wirtschaftlichkeitsannahmen annahmenZuSpeichern,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(annahmenZuSpeichern);
        }

        public Task<Wirtschaftlichkeitsergebnis?> BerechnenAsync(
            Guid projektId,
            Guid alternativeId,
            WirtschaftlichkeitsBasis basis,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ergebnis);
        }
    }
}
