using Kompass.Application.Economics;
using Kompass.Domain.Common;
using Kompass.Domain.Economics;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Economics;

[ApiController]
[Route("api/projekte/{projektId:guid}/alternativen/{alternativeId:guid}/kostenpositionen")]
public sealed class KostenpositionenController : ControllerBase
{
    private readonly IKostenpositionService _kostenpositionService;
    private readonly ILogger<KostenpositionenController> _logger;

    public KostenpositionenController(
        IKostenpositionService kostenpositionService,
        ILogger<KostenpositionenController> logger)
    {
        _kostenpositionService = kostenpositionService;
        _logger = logger;
    }

    [HttpGet(Name = KostenpositionenListenRoute)]
    [ProducesResponseType(
        typeof(IReadOnlyList<Kostenposition>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Kostenposition>>> ListenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken)
    {
        var positionen =
            await _kostenpositionService.ListenAsync(
                projektId,
                alternativeId,
                cancellationToken);

        return Ok(positionen);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(Kostenposition),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Kostenposition>> HinzufuegenAsync(
        Guid projektId,
        Guid alternativeId,
        [FromBody] KostenpositionHinzufuegenAnfrage anfrage,
        CancellationToken cancellationToken)
    {
        Kostenposition kostenposition;

        try
        {
            kostenposition = new Kostenposition(
                Guid.NewGuid(),
                anfrage.Bezeichnung,
                anfrage.Betrag,
                anfrage.Kostenart);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Kostenposition konnte wegen ungültiger Daten nicht erstellt werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }

        var gespeicherte =
            await _kostenpositionService.HinzufuegenAsync(
                projektId,
                alternativeId,
                kostenposition,
                cancellationToken);

        if (gespeicherte is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Alternative '{alternativeId}' nicht in Projekt '{projektId}' gefunden."
            });
        }

        return CreatedAtRoute(
            KostenpositionenListenRoute,
            new { projektId, alternativeId },
            gespeicherte);
    }

    [HttpDelete("{kostenpositionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid kostenpositionId,
        CancellationToken cancellationToken)
    {
        var entfernt =
            await _kostenpositionService.EntfernenAsync(
                projektId,
                alternativeId,
                kostenpositionId,
                cancellationToken);

        if (!entfernt)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Kostenposition '{kostenpositionId}' in Alternative '{alternativeId}' nicht gefunden."
            });
        }

        return NoContent();
    }

    private const string KostenpositionenListenRoute =
        "KostenpositionenListen";
}
