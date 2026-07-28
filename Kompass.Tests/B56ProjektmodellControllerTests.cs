using Kompass.Api.B56Import;
using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.Api;

public sealed class B56ProjektmodellControllerTests
{
    [Theory]
    [InlineData(
        B56ProjektmodellUebernahmeStatus.Erfolgreich,
        typeof(OkObjectResult))]
    [InlineData(
        B56ProjektmodellUebernahmeStatus.NichtGefunden,
        typeof(NotFoundObjectResult))]
    [InlineData(
        B56ProjektmodellUebernahmeStatus.NichtZulaessig,
        typeof(ConflictObjectResult))]
    public async Task Uebernehmen_bildet_Anwendungsergebnis_auf_HTTP_ab(
        B56ProjektmodellUebernahmeStatus status,
        Type erwarteterErgebnistyp)
    {
        var projektId =
            Guid.NewGuid();

        var importId =
            Guid.NewGuid();

        var ergebnis =
            new B56ProjektmodellUebernahmeErgebnis(
                status,
                projektId,
                importId,
                ProjektmodellVersion: 1,
                UebernommeneAlternativen: 2,
                Nachricht: "Testnachricht");

        var controller =
            new B56ProjektmodellController(
                new UebernahmeServiceFake(
                    ergebnis));

        var antwort =
            await controller.UebernehmenAsync(
                projektId,
                importId,
                CancellationToken.None);

        Assert.IsType(
            erwarteterErgebnistyp,
            antwort.Result);
    }

    [Fact]
    public async Task Uebernehmen_gibt_Ergebnisdaten_weiter()
    {
        var projektId =
            Guid.NewGuid();

        var importId =
            Guid.NewGuid();

        var ergebnis =
            new B56ProjektmodellUebernahmeErgebnis(
                B56ProjektmodellUebernahmeStatus.Erfolgreich,
                projektId,
                importId,
                ProjektmodellVersion: 3,
                UebernommeneAlternativen: 5,
                Nachricht: "Übernahme erfolgreich.");

        var controller =
            new B56ProjektmodellController(
                new UebernahmeServiceFake(
                    ergebnis));

        var antwort =
            await controller.UebernehmenAsync(
                projektId,
                importId,
                CancellationToken.None);

        var ok =
            Assert.IsType<OkObjectResult>(
                antwort.Result);

        var gesendetes =
            Assert.IsType<B56ProjektmodellUebernahmeErgebnis>(
                ok.Value);

        Assert.Equal(
            projektId,
            gesendetes.ProjektId);

        Assert.Equal(
            importId,
            gesendetes.ImportId);

        Assert.Equal(
            3,
            gesendetes.ProjektmodellVersion);

        Assert.Equal(
            5,
            gesendetes.UebernommeneAlternativen);
    }

    private sealed class UebernahmeServiceFake(
        B56ProjektmodellUebernahmeErgebnis ergebnis)
        : IB56ProjektmodellUebernahmeService
    {
        public Task<B56ProjektmodellUebernahmeErgebnis> UebernehmenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ergebnis);
        }
    }
}
