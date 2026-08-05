using Kompass.Application.Funding;
using Kompass.Domain.Funding;
using Kompass.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Funding;

[ApiController]
[Route("api/projekte/{projektId:guid}/foerdervoraussetzungen")]
public sealed class FoerdervoraussetzungenController : ControllerBase
{
    private readonly IFoerdervoraussetzungenService _service;
    public FoerdervoraussetzungenController(IFoerdervoraussetzungenService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<Foerdervoraussetzungen>> Abrufen(Guid projektId, CancellationToken ct)
    {
        var wert = await _service.AbrufenAsync(projektId, ct);
        return wert is null ? NotFound() : Ok(wert);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Foerdervoraussetzungen>> Speichern(Guid projektId, FoerdervoraussetzungenEingabe eingabe, CancellationToken ct)
    {
        try
        {
            var wert = await _service.SpeichernAsync(projektId, eingabe, ct);
            return wert is null ? NotFound() : Ok(wert);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { Nachricht = exception.Message });
        }
    }
}
