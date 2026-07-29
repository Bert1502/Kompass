using Kompass.Application.Waermebruecken;
using Kompass.Domain.Common;
using Kompass.Domain.Waermebruecken;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Waermebruecken;

[ApiController]
[Route("api/projekte/{projektId:guid}/waermebruecken")]
public sealed class WaermebrueckenController : ControllerBase
{
    private readonly IWaermebrueckeService _waermebrueckeService;

    public WaermebrueckenController(
        IWaermebrueckeService waermebrueckeService)
    {
        _waermebrueckeService = waermebrueckeService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<Waermebruecke>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Waermebruecke>>> ListenAsync(
        Guid projektId,
        CancellationToken cancellationToken)
    {
        var waermebruecken =
            await _waermebrueckeService.ListenAsync(
                projektId,
                cancellationToken);

        return Ok(waermebruecken);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(Waermebruecke),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Waermebruecke>> AbrufenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var waermebruecke =
            await _waermebrueckeService.AbrufenAsync(
                projektId,
                id,
                cancellationToken);

        if (waermebruecke is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Wärmebrücke '{id}' in Projekt '{projektId}' nicht gefunden."
            });
        }

        return Ok(waermebruecke);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(Waermebruecke),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Waermebruecke>> AnlegenAsync(
        Guid projektId,
        [FromBody] WaermebrueckeAnlegenRequest request,
        CancellationToken cancellationToken)
    {
        var waermebruecke = new Waermebruecke(
            Guid.NewGuid(),
            projektId,
            request.InterneNummer,
            request.Bezeichnung,
            request.Typ);

        waermebruecke.DatenAktualisieren(
            request.InterneNummer,
            request.Bezeichnung,
            request.Typ,
            WaermebrueckeStatus.Offen,
            GleichwertigkeitStatus.NichtBewertet,
            lage: request.Lage,
            planreferenz: request.Planreferenz,
            detailreferenz: request.Detailreferenz,
            fremdnummer: request.Fremdnummer,
            laenge: request.Laenge);

        try
        {
            var angelegt =
                await _waermebrueckeService.AnlegenAsync(
                    waermebruecke,
                    cancellationToken);

            if (angelegt is null)
            {
                return NotFound(new
                {
                    Nachricht =
                        $"Projekt '{projektId}' nicht gefunden."
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
        catch (DomainException ex)
        {
            return Conflict(new { Nachricht = ex.Message });
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AktualisierenAsync(
        Guid projektId,
        Guid id,
        [FromBody] WaermebrueckeAktualisierenRequest request,
        CancellationToken cancellationToken)
    {
        var waermebruecke =
            await _waermebrueckeService.AbrufenAsync(
                projektId,
                id,
                cancellationToken);

        if (waermebruecke is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Wärmebrücke '{id}' in Projekt '{projektId}' nicht gefunden."
            });
        }

        try
        {
            waermebruecke.DatenAktualisieren(
                request.InterneNummer,
                request.Bezeichnung,
                request.Typ,
                request.Status,
                request.GleichwertigkeitStatus,
                lage: request.Lage,
                planreferenz: request.Planreferenz,
                detailreferenz: request.Detailreferenz,
                fremdnummer: request.Fremdnummer,
                laenge: request.Laenge,
                beiblatt2Referenz: request.Beiblatt2Referenz,
                thermCadReferenz: request.ThermCadReferenz,
                psiWert: request.PsiWert,
                fRsi: request.FRsi,
                pruefanmerkung: request.Pruefanmerkung,
                berichtsdarstellung: request.Berichtsdarstellung);
        }
        catch (DomainException ex)
        {
            return Conflict(new { Nachricht = ex.Message });
        }

        await _waermebrueckeService.AktualisierenAsync(
            waermebruecke,
            cancellationToken);

        return NoContent();
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
            await _waermebrueckeService.LoeschenAsync(
                projektId,
                id,
                cancellationToken);

        if (!geloescht)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Wärmebrücke '{id}' in Projekt '{projektId}' nicht gefunden."
            });
        }

        return NoContent();
    }
}
