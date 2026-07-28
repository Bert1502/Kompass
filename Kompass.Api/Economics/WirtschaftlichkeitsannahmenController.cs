using Kompass.Application.Economics;
using Kompass.Domain.Common;
using Kompass.Domain.Economics;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Economics;

[ApiController]
[Route("api/projekte/{projektId:guid}/alternativen/{alternativeId:guid}/wirtschaftlichkeit")]
public sealed class WirtschaftlichkeitsannahmenController : ControllerBase
{
    private readonly IWirtschaftlichkeitsService _wirtschaftlichkeitsService;
    private readonly ILogger<WirtschaftlichkeitsannahmenController> _logger;

    public WirtschaftlichkeitsannahmenController(
        IWirtschaftlichkeitsService wirtschaftlichkeitsService,
        ILogger<WirtschaftlichkeitsannahmenController> logger)
    {
        _wirtschaftlichkeitsService = wirtschaftlichkeitsService;
        _logger = logger;
    }

    [HttpGet("annahmen/{basis}")]
    [ProducesResponseType(
        typeof(Wirtschaftlichkeitsannahmen),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Wirtschaftlichkeitsannahmen>> AnnahmenAbrufenAsync(
        Guid projektId,
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken)
    {
        var annahmen =
            await _wirtschaftlichkeitsService.AnnahmenAbrufenAsync(
                projektId,
                alternativeId,
                basis,
                cancellationToken);

        if (annahmen is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Keine Wirtschaftlichkeitsannahmen für Alternative '{alternativeId}' " +
                    $"mit Basis '{basis}' gefunden."
            });
        }

        return Ok(annahmen);
    }

    [HttpPut("annahmen/{basis}")]
    [ProducesResponseType(
        typeof(Wirtschaftlichkeitsannahmen),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Wirtschaftlichkeitsannahmen>> AnnahmenSpeichernAsync(
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        [FromBody] WirtschaftlichkeitsannahmenSetzenAnfrage anfrage,
        CancellationToken cancellationToken)
    {
        try
        {
            var annahmen = ErzeugeAnnahmen(
                alternativeId,
                basis,
                anfrage);

            var gespeicherte =
                await _wirtschaftlichkeitsService.AnnahmenSpeichernAsync(
                    annahmen,
                    cancellationToken);

            return Ok(gespeicherte);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Wirtschaftlichkeitsannahmen konnten wegen ungültiger Daten nicht gespeichert werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
    }

    [HttpPost("berechnen/{basis}")]
    [ProducesResponseType(
        typeof(Wirtschaftlichkeitsergebnis),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Wirtschaftlichkeitsergebnis>> BerechnenAsync(
        Guid projektId,
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken)
    {
        var ergebnis =
            await _wirtschaftlichkeitsService.BerechnenAsync(
                projektId,
                alternativeId,
                basis,
                cancellationToken);

        if (ergebnis is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Alternative '{alternativeId}' oder Wirtschaftlichkeitsannahmen " +
                    $"mit Basis '{basis}' nicht gefunden."
            });
        }

        return Ok(ergebnis);
    }

    private static Wirtschaftlichkeitsannahmen ErzeugeAnnahmen(
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        WirtschaftlichkeitsannahmenSetzenAnfrage anfrage)
    {
        var annahmen = new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            alternativeId,
            basis,
            anfrage.Betrachtungszeitraum,
            anfrage.Diskontsatz,
            anfrage.Inflationsrate,
            anfrage.JaehrlicheWartungsmehrkosten,
            anfrage.Nutzungsdauer,
            anfrage.Foerderung);

        foreach (var traegerAnfrage in anfrage.EnergietraegerAnnahmen)
        {
            annahmen.EnergietraegerAnnahmeHinzufuegen(
                new EnergietraegerAnnahme(
                    Guid.NewGuid(),
                    traegerAnfrage.Energietraeger,
                    traegerAnfrage.Preis,
                    traegerAnfrage.Preissteigerungsrate,
                    traegerAnfrage.Co2Faktor,
                    traegerAnfrage.Co2Preis,
                    traegerAnfrage.Co2Preissteigerungsrate,
                    traegerAnfrage.EndenergieIstZustand,
                    traegerAnfrage.EndenergieAlternative));
        }

        return annahmen;
    }
}
