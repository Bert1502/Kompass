using Kompass.Application.Reports;
using Kompass.Domain.Economics;
using Kompass.Domain.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Reports;

[ApiController]
[Route("api/projekte/{projektId:guid}/berichte")]
public sealed class BerichteController : ControllerBase
{
    private readonly IBerichtsService _berichtsService;

    public BerichteController(IBerichtsService berichtsService)
    {
        _berichtsService = berichtsService;
    }

    [HttpGet("alternativenvergleich")]
    [ProducesResponseType(
        typeof(AlternativenvergleichBericht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlternativenvergleichBericht>> AlternativenvergleichAsync(
        Guid projektId,
        CancellationToken cancellationToken)
    {
        var bericht =
            await _berichtsService.AlternativenvergleichErzeugenAsync(
                projektId,
                cancellationToken);

        if (bericht is null)
        {
            return NotFound(new
            {
                Nachricht = $"Projekt '{projektId}' nicht gefunden."
            });
        }

        return Ok(bericht);
    }

    [HttpGet("waermebrueckenuebersicht")]
    [ProducesResponseType(
        typeof(WaermebrueckenuebersichtBericht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WaermebrueckenuebersichtBericht>> WaermebrueckenuebersichtAsync(
        Guid projektId,
        CancellationToken cancellationToken)
    {
        var bericht =
            await _berichtsService.WaermebrueckenuebersichtErzeugenAsync(
                projektId,
                cancellationToken);

        if (bericht is null)
        {
            return NotFound(new
            {
                Nachricht = $"Projekt '{projektId}' nicht gefunden."
            });
        }

        return Ok(bericht);
    }

    [HttpGet("wirtschaftlichkeit/{basis}")]
    [ProducesResponseType(
        typeof(WirtschaftlichkeitsberichtBericht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WirtschaftlichkeitsberichtBericht>> WirtschaftlichkeitsberichtAsync(
        Guid projektId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken)
    {
        var bericht =
            await _berichtsService.WirtschaftlichkeitsberichtErzeugenAsync(
                projektId,
                basis,
                cancellationToken);

        if (bericht is null)
        {
            return NotFound(new
            {
                Nachricht = $"Projekt '{projektId}' nicht gefunden."
            });
        }

        return Ok(bericht);
    }

    [HttpGet("foerderuebersicht")]
    [ProducesResponseType(
        typeof(FoerderuebersichtBericht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoerderuebersichtBericht>> FoerderuebersichtAsync(
        Guid projektId,
        CancellationToken cancellationToken)
    {
        var bericht =
            await _berichtsService.FoerderuebersichtErzeugenAsync(
                projektId,
                cancellationToken);

        if (bericht is null)
        {
            return NotFound(new
            {
                Nachricht = $"Projekt '{projektId}' nicht gefunden."
            });
        }

        return Ok(bericht);
    }
}
