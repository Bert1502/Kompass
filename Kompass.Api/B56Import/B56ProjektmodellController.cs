using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
[Route("api/projekte/{projektId:guid}/b56-importe/{importId:guid}")]
public sealed class B56ProjektmodellController : ControllerBase
{
    private readonly IB56ProjektmodellUebernahmeService
        _uebernahmeService;

    public B56ProjektmodellController(
        IB56ProjektmodellUebernahmeService uebernahmeService)
    {
        _uebernahmeService = uebernahmeService;
    }

    [HttpPost("in-projektmodell-uebernehmen")]
    [ProducesResponseType(
        typeof(B56ProjektmodellUebernahmeErgebnis),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(B56ProjektmodellUebernahmeErgebnis),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(B56ProjektmodellUebernahmeErgebnis),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<B56ProjektmodellUebernahmeErgebnis>>
        UebernehmenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken)
    {
        var ergebnis =
            await _uebernahmeService.UebernehmenAsync(
                projektId,
                importId,
                cancellationToken);

        return ergebnis.Status switch
        {
            B56ProjektmodellUebernahmeStatus.Erfolgreich =>
                Ok(ergebnis),
            B56ProjektmodellUebernahmeStatus.NichtGefunden =>
                NotFound(ergebnis),
            _ =>
                Conflict(ergebnis)
        };
    }
}
