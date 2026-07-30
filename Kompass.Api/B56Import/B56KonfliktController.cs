using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
[Route("api/projekte/{projektId:guid}/b56-importe/{nachfolgerImportId:guid}/konflikte")]
public sealed class B56KonfliktController : ControllerBase
{
    private readonly IB56KonfliktService _konfliktService;

    public B56KonfliktController(
        IB56KonfliktService konfliktService)
    {
        _konfliktService = konfliktService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<B56KonfliktEintragAntwort>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<B56KonfliktEintragAntwort>>>
        ListenAsync(
            Guid projektId,
            Guid nachfolgerImportId,
            [FromQuery] Guid vorgaenger,
            CancellationToken cancellationToken)
    {
        if (vorgaenger == Guid.Empty)
        {
            return BadRequest(new
            {
                Nachricht =
                    "Der Query-Parameter 'vorgaenger' muss eine gültige Import-ID enthalten."
            });
        }

        var eintraege =
            await _konfliktService.ListenOderErzeugenAsync(
                projektId,
                vorgaenger,
                nachfolgerImportId,
                cancellationToken);

        return Ok(
            eintraege
                .Select(B56KonfliktEintragAntwort.Aus)
                .ToList());
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EntscheidungSetzenAsync(
        Guid projektId,
        Guid nachfolgerImportId,
        Guid id,
        [FromBody] B56KonfliktEntscheidungAnfrage anfrage,
        CancellationToken cancellationToken)
    {
        if (anfrage.Entscheidung == B56KonfliktEntscheidungsTyp.Offen)
        {
            return BadRequest(new
            {
                Nachricht =
                    "Die Entscheidung 'Offen' ist kein zulässiger Zielzustand. " +
                    "Wählen Sie 'Uebernehmen' oder 'Behalten'."
            });
        }

        var gefunden =
            await _konfliktService.EntscheidungSetzenAsync(
                projektId,
                nachfolgerImportId,
                id,
                anfrage.Entscheidung,
                cancellationToken);

        if (!gefunden)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Der Konflikteintrag '{id}' wurde nicht gefunden."
            });
        }

        return NoContent();
    }
}

public sealed record B56KonfliktEintragAntwort(
    Guid Id,
    Guid ProjektId,
    Guid VorgaengerImportId,
    Guid NachfolgerImportId,
    string Bereich,
    string Schluessel,
    string Feld,
    B56VergleichsAenderung Aenderung,
    B56KonfliktEntscheidungsTyp Entscheidung,
    DateTimeOffset? EntschiedenAm,
    DateTimeOffset ErstelltAm)
{
    public static B56KonfliktEintragAntwort Aus(
        B56KonfliktEintrag eintrag)
    {
        return new B56KonfliktEintragAntwort(
            eintrag.Id,
            eintrag.ProjektId,
            eintrag.VorgaengerImportId,
            eintrag.NachfolgerImportId,
            eintrag.Bereich,
            eintrag.Schluessel,
            eintrag.Feld,
            eintrag.Aenderung,
            eintrag.Entscheidung,
            eintrag.EntschiedenAm,
            eintrag.ErstelltAm);
    }
}

public sealed record B56KonfliktEntscheidungAnfrage(
    B56KonfliktEntscheidungsTyp Entscheidung);
