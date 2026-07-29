using Kompass.Application.Verbrauch;
using Kompass.Domain.Common;
using Kompass.Domain.Verbrauch;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Verbrauch;

[ApiController]
[Route("api/projekte/{projektId:guid}/verbrauchsdaten")]
public sealed class VerbrauchsDatenController : ControllerBase
{
    private readonly IVerbrauchsDatenService _verbrauchsDatenService;

    public VerbrauchsDatenController(
        IVerbrauchsDatenService verbrauchsDatenService)
    {
        _verbrauchsDatenService = verbrauchsDatenService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<VerbrauchsDaten>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VerbrauchsDaten>>> ListenAsync(
        Guid projektId,
        CancellationToken cancellationToken)
    {
        var datensaetze =
            await _verbrauchsDatenService.ListenAsync(
                projektId,
                cancellationToken);

        return Ok(datensaetze);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(VerbrauchsDaten),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerbrauchsDaten>> AbrufenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var datensatz =
            await _verbrauchsDatenService.AbrufenAsync(
                projektId,
                id,
                cancellationToken);

        if (datensatz is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Verbrauchsdaten '{id}' in Projekt '{projektId}' nicht gefunden."
            });
        }

        return Ok(datensatz);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(VerbrauchsDaten),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VerbrauchsDaten>> AnlegenAsync(
        Guid projektId,
        [FromBody] VerbrauchsDatenAnlegenRequest request,
        CancellationToken cancellationToken)
    {
        VerbrauchsDaten datensatz;

        try
        {
            datensatz = new VerbrauchsDaten(
                Guid.NewGuid(),
                projektId,
                request.PeriodeVon,
                request.PeriodeBis,
                request.Energietraeger,
                request.Menge,
                request.Kosten);

            datensatz.Aktualisieren(
                request.PeriodeVon,
                request.PeriodeBis,
                request.Energietraeger,
                request.Menge,
                request.Kosten,
                request.WitterungsbereinigungsFaktor,
                request.Flaeche,
                request.B56VergleichsWert,
                request.AnpassungsFaktor,
                request.AnpassungsBegruendung,
                request.Abweichungsursache);
        }
        catch (DomainException ex)
        {
            return Conflict(new { Nachricht = ex.Message });
        }

        var angelegt =
            await _verbrauchsDatenService.AnlegenAsync(
                datensatz,
                cancellationToken);

        if (angelegt is null)
        {
            return NotFound(new
            {
                Nachricht = $"Projekt '{projektId}' nicht gefunden."
            });
        }

        return CreatedAtAction(
            nameof(AbrufenAsync),
            new
            {
                projektId,
                id = angelegt.Id
            },
            angelegt);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(
        typeof(VerbrauchsDaten),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VerbrauchsDaten>> AktualisierenAsync(
        Guid projektId,
        Guid id,
        [FromBody] VerbrauchsDatenAktualisierenRequest request,
        CancellationToken cancellationToken)
    {
        var datensatz =
            await _verbrauchsDatenService.AbrufenAsync(
                projektId,
                id,
                cancellationToken);

        if (datensatz is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Verbrauchsdaten '{id}' in Projekt '{projektId}' nicht gefunden."
            });
        }

        try
        {
            datensatz.Aktualisieren(
                request.PeriodeVon,
                request.PeriodeBis,
                request.Energietraeger,
                request.Menge,
                request.Kosten,
                request.WitterungsbereinigungsFaktor,
                request.Flaeche,
                request.B56VergleichsWert,
                request.AnpassungsFaktor,
                request.AnpassungsBegruendung,
                request.Abweichungsursache);
        }
        catch (DomainException ex)
        {
            return Conflict(new { Nachricht = ex.Message });
        }

        await _verbrauchsDatenService.AktualisierenAsync(
            datensatz,
            cancellationToken);

        return Ok(datensatz);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoeschenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var geloescht =
            await _verbrauchsDatenService.LoeschenAsync(
                projektId,
                id,
                cancellationToken);

        if (!geloescht)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Verbrauchsdaten '{id}' in Projekt '{projektId}' nicht gefunden."
            });
        }

        return NoContent();
    }
}
