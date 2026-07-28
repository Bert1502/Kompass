using Kompass.Application.Funding;
using Kompass.Domain.Funding;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Funding;

[ApiController]
[Route("api/projekte/{projektId:guid}/alternativen/{alternativeId:guid}/foerderprogramme")]
public sealed class AlternativeFoerderprogrammeController : ControllerBase
{
    private readonly IAlternativeFoerderungService _alternativeFoerderungService;

    public AlternativeFoerderprogrammeController(
        IAlternativeFoerderungService alternativeFoerderungService)
    {
        _alternativeFoerderungService = alternativeFoerderungService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<Foerderprogramm>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<Foerderprogramm>>> ListenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken)
    {
        var programme =
            await _alternativeFoerderungService.ZugeordneteProgrammeListenAsync(
                projektId,
                alternativeId,
                cancellationToken);

        return Ok(programme);
    }

    [HttpPut("{foerderprogrammId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ZuordnenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid foerderprogrammId,
        CancellationToken cancellationToken)
    {
        var zugeordnet =
            await _alternativeFoerderungService.ProgrammZuordnenAsync(
                projektId,
                alternativeId,
                foerderprogrammId,
                cancellationToken);

        if (!zugeordnet)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Alternative '{alternativeId}' oder Förderprogramm " +
                    $"'{foerderprogrammId}' nicht gefunden, oder Zuordnung besteht bereits."
            });
        }

        return NoContent();
    }

    [HttpDelete("{foerderprogrammId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid foerderprogrammId,
        CancellationToken cancellationToken)
    {
        var entfernt =
            await _alternativeFoerderungService.ProgrammEntfernenAsync(
                projektId,
                alternativeId,
                foerderprogrammId,
                cancellationToken);

        if (!entfernt)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Zuordnung von Förderprogramm '{foerderprogrammId}' " +
                    $"zu Alternative '{alternativeId}' nicht gefunden."
            });
        }

        return NoContent();
    }

    [HttpPost("berechnen")]
    [ProducesResponseType(
        typeof(Foerderberechnungsergebnis),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Foerderberechnungsergebnis>> BerechnenAsync(
        Guid projektId,
        Guid alternativeId,
        [FromQuery] DateOnly? stichtag,
        CancellationToken cancellationToken)
    {
        var berechnungsstichtag = stichtag ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var ergebnis =
            await _alternativeFoerderungService.FoerderungBerechnenAsync(
                projektId,
                alternativeId,
                berechnungsstichtag,
                cancellationToken);

        if (ergebnis is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Alternative '{alternativeId}' nicht in Projekt '{projektId}' gefunden."
            });
        }

        return Ok(ergebnis);
    }
}
