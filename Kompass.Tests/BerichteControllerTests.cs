using Kompass.Api.Reports;
using Kompass.Application.Reports;
using Kompass.Domain.Projects;
using Kompass.Domain.Reports;
using Kompass.Domain.Waermebruecken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.Api;

public sealed class BerichteControllerTests
{
    [Fact]
    public async Task Alternativenvergleich_liefert_200_wenn_Projekt_gefunden()
    {
        var bericht = ErstelleAlternativenvergleichBericht();

        var controller = new BerichteController(
            new BerichtsServiceFake(alternativenvergleich: bericht));

        var antwort =
            await controller.AlternativenvergleichAsync(
                bericht.Kopf.ProjektId,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        Assert.IsType<AlternativenvergleichBericht>(ok.Value);
    }

    [Fact]
    public async Task Alternativenvergleich_liefert_404_wenn_Projekt_nicht_gefunden()
    {
        var controller = new BerichteController(
            new BerichtsServiceFake(alternativenvergleich: null));

        var antwort =
            await controller.AlternativenvergleichAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Waermebrueckenuebersicht_liefert_200_wenn_Projekt_gefunden()
    {
        var bericht = ErstelleWaermebrueckenuebersichtBericht();

        var controller = new BerichteController(
            new BerichtsServiceFake(waermebrueckenuebersicht: bericht));

        var antwort =
            await controller.WaermebrueckenuebersichtAsync(
                bericht.Kopf.ProjektId,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        Assert.IsType<WaermebrueckenuebersichtBericht>(ok.Value);
    }

    [Fact]
    public async Task Waermebrueckenuebersicht_liefert_404_wenn_Projekt_nicht_gefunden()
    {
        var controller = new BerichteController(
            new BerichtsServiceFake(waermebrueckenuebersicht: null));

        var antwort =
            await controller.WaermebrueckenuebersichtAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    private static AlternativenvergleichBericht ErstelleAlternativenvergleichBericht()
    {
        var kopf = ErstelleBerichtskopf(Berichtstyp.Alternativenvergleich);
        var zeilen = new List<AlternativenvergleichZeile>
        {
            new(Guid.NewGuid(), 1, "Variante A", "Kurztext A", 10000m, 1, true),
        };

        return new AlternativenvergleichBericht(kopf, zeilen);
    }

    private static WaermebrueckenuebersichtBericht ErstelleWaermebrueckenuebersichtBericht() =>
        new(
            ErstelleBerichtskopf(Berichtstyp.Waermebrueckenuebersicht),
            []);

    private static Berichtskopf ErstelleBerichtskopf(Berichtstyp typ) =>
        new(
            Guid.NewGuid(),
            "Testprojekt",
            null,
            Bearbeitungsstatus.InBearbeitung,
            null,
            DateTimeOffset.UtcNow,
            typ);

    private sealed class BerichtsServiceFake(
        AlternativenvergleichBericht? alternativenvergleich = null,
        WaermebrueckenuebersichtBericht? waermebrueckenuebersicht = null)
        : IBerichtsService
    {
        public Task<AlternativenvergleichBericht?> AlternativenvergleichErzeugenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(alternativenvergleich);
        }

        public Task<WaermebrueckenuebersichtBericht?> WaermebrueckenuebersichtErzeugenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(waermebrueckenuebersicht);
        }
    }
}
