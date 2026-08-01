using Kompass.Application.Referenzdaten;
using Kompass.Domain.Common;
using Kompass.Domain.Referenzdaten;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Referenzdaten;

[ApiController]
[Route("api/referenzdaten")]
public sealed class ReferenzdatenController : ControllerBase
{
    private readonly IReferenzdatenService _service;
    private readonly ILogger<ReferenzdatenController> _logger;

    public ReferenzdatenController(
        IReferenzdatenService service,
        ILogger<ReferenzdatenController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Referenzdatensatz>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Referenzdatensatz>>> ListenAsync(
        CancellationToken cancellationToken)
    {
        var daten = await _service.ListenAsync(cancellationToken);
        return Ok(daten);
    }

    [HttpGet("aufloesung")]
    [ProducesResponseType(typeof(ReferenzwertAufloesungResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReferenzwertAufloesungResponse>> AufloesenAsync(
        [FromQuery] string parameterart,
        [FromQuery] Guid? projektId,
        [FromQuery] Guid? unternehmenId,
        [FromQuery] DateOnly? stichtag,
        [FromQuery] string? bezugsgroesse,
        [FromQuery] string? energietraegerOderKategorie,
        CancellationToken cancellationToken)
    {
        var wert = await _service.WertAufloesenAsync(
            new ReferenzwertAnfrage(
                parameterart,
                stichtag,
                projektId,
                unternehmenId,
                bezugsgroesse,
                energietraegerOderKategorie),
            cancellationToken);

        if (wert is null)
        {
            return NotFound();
        }

        return Ok(new ReferenzwertAufloesungResponse(
            wert.Datensatz.Id,
            wert.Datensatz.Parameterart,
            wert.Datensatz.Wert,
            wert.Datensatz.Einheit,
            wert.Prioritaet.ToString(),
            wert.Datensatz.Quelle,
            wert.Datensatz.Herausgeber,
            wert.Datensatz.GueltigAb,
            wert.Datensatz.GueltigBis));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Referenzdatensatz), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Referenzdatensatz>> SpeichernAsync(
        [FromBody] ReferenzdatensatzSpeichernRequest anfrage,
        CancellationToken cancellationToken)
    {
        try
        {
            var datensatz = new Referenzdatensatz(
                Guid.NewGuid(),
                anfrage.FachlicheBezeichnung,
                anfrage.Parameterart,
                anfrage.Wert,
                anfrage.Ebene,
                anfrage.Quelle,
                anfrage.Herausgeber,
                anfrage.QuellenVerweis,
                anfrage.GueltigAb,
                anfrage.GueltigBis,
                anfrage.Versionsstand,
                anfrage.Datenstatus,
                anfrage.Qualitaetsstatus,
                anfrage.Importart,
                DateTimeOffset.UtcNow,
                anfrage.Einheit,
                anfrage.Bezugsgroesse,
                anfrage.EnergietraegerOderKategorie,
                anfrage.Veroeffentlichungsdatum,
                anfrage.Abrufdatum,
                anfrage.ProjektId,
                anfrage.UnternehmenId);

            var gespeichert = await _service.SpeichernAsync(datensatz, cancellationToken);

            return CreatedAtAction(nameof(ListenAsync), new { }, gespeichert);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Referenzdatensatz konnte nicht gespeichert werden.");
            return BadRequest(new { Nachricht = ex.Message });
        }
    }

    [HttpPost("synchronisieren")]
    [ProducesResponseType(typeof(ReferenzdatenSynchronisationsErgebnis), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReferenzdatenSynchronisationsErgebnis>> SynchronisierenAsync(
        CancellationToken cancellationToken)
    {
        var result = await _service.SynchronisierenAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("abweichungen")]
    [ProducesResponseType(typeof(ReferenzwertAbweichung), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReferenzwertAbweichung>> AbweichungSetzenAsync(
        [FromBody] ProjektabweichungRequest anfrage,
        CancellationToken cancellationToken)
    {
        try
        {
            var gespeichert = await _service.ProjektabweichungSetzenAsync(
                new ProjektabweichungAnfrage(
                    anfrage.ProjektId,
                    anfrage.Parameterart,
                    anfrage.VerwendeterProjektwert,
                    anfrage.Begruendung,
                    anfrage.Benutzer,
                    anfrage.Bezugsgroesse,
                    anfrage.EnergietraegerOderKategorie),
                cancellationToken);

            return CreatedAtAction(nameof(ListenAsync), new { }, gespeichert);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Projektabweichung konnte nicht gespeichert werden.");
            return BadRequest(new { Nachricht = ex.Message });
        }
    }

    [HttpGet("abweichungen/{projektId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ReferenzwertAbweichung>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReferenzwertAbweichung>>> ProjektabweichungenAsync(
        Guid projektId,
        CancellationToken cancellationToken)
    {
        var daten = await _service.ProjektabweichungenListenAsync(projektId, cancellationToken);
        return Ok(daten);
    }
}
