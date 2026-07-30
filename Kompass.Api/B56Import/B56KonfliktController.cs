using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
[Route(
    "api/projekte/{projektId:guid}/b56-importe/{nachfolgerImportId:guid}/konflikte")]
public sealed class B56KonfliktController : ControllerBase
{
    private readonly IB56KonfliktService _konfliktService;

    public B56KonfliktController(
        IB56KonfliktService konfliktService)
    {
        _konfliktService = konfliktService;
    }

    /// <summary>
    /// Listet alle Konflikteinträge für einen Snapshot-Vergleich.
    /// Initialisiert die Einträge automatisch, wenn sie noch nicht
    /// vorhanden sind.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<B56KonfliktAntwort>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<B56KonfliktAntwort>>>
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
            await _konfliktService.ListenAsync(
                projektId,
                vorgaenger,
                nachfolgerImportId,
                cancellationToken);

        return Ok(
            eintraege
                .Select(B56KonfliktAntwort.Aus)
                .ToList());
    }

    /// <summary>
    /// Setzt die Entscheidung für einen einzelnen Konflikteintrag.
    /// </summary>
    [HttpPost("{konfliktId:guid}/entscheiden")]
    [ProducesResponseType(
        typeof(B56KonfliktAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<B56KonfliktAntwort>> EntscheidenAsync(
        Guid projektId,
        Guid nachfolgerImportId,
        Guid konfliktId,
        [FromQuery] Guid vorgaenger,
        [FromBody] B56KonfliktEntscheidungRequest request,
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

        if (request.Entscheidung ==
            B56KonfliktEntscheidungsTyp.Ausstehend)
        {
            return BadRequest(new
            {
                Nachricht =
                    "Die Entscheidung muss 'Akzeptiert' oder 'Abgelehnt' sein."
            });
        }

        var eintrag =
            await _konfliktService.EntscheidenAsync(
                projektId,
                vorgaenger,
                nachfolgerImportId,
                konfliktId,
                request.Entscheidung,
                cancellationToken);

        if (eintrag is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Der Konflikteintrag '{konfliktId}' wurde nicht gefunden."
            });
        }

        return Ok(B56KonfliktAntwort.Aus(eintrag));
    }

    /// <summary>
    /// Akzeptiert alle noch ausstehenden Konflikte eines Vergleichs.
    /// </summary>
    [HttpPost("alle-akzeptieren")]
    [ProducesResponseType(
        typeof(B56AlleAkzeptiertAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<B56AlleAkzeptiertAntwort>>
        AlleAkzeptierenAsync(
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

        var anzahl =
            await _konfliktService.AlleAusstehendAkzeptierenAsync(
                projektId,
                vorgaenger,
                nachfolgerImportId,
                cancellationToken);

        return Ok(new B56AlleAkzeptiertAntwort(anzahl));
    }
}

public sealed record B56KonfliktAntwort(
    Guid KonfliktId,
    Guid ProjektId,
    Guid VorgaengerSnapshotId,
    Guid NachfolgerSnapshotId,
    string Bereich,
    string Schluessel,
    string Feld,
    B56VergleichsAenderung Aenderung,
    string? AlterWert,
    string? NeuerWert,
    B56KonfliktEntscheidungsTyp Entscheidung,
    DateTimeOffset? EntschiedenAm)
{
    public static B56KonfliktAntwort Aus(
        B56KonfliktEintrag eintrag)
    {
        return new B56KonfliktAntwort(
            eintrag.KonfliktId,
            eintrag.ProjektId,
            eintrag.VorgaengerSnapshotId,
            eintrag.NachfolgerSnapshotId,
            eintrag.Bereich,
            eintrag.Schluessel,
            eintrag.Feld,
            eintrag.Aenderung,
            eintrag.AlterWert,
            eintrag.NeuerWert,
            eintrag.Entscheidung,
            eintrag.EntschiedenAm);
    }
}

public sealed record B56KonfliktEntscheidungRequest(
    B56KonfliktEntscheidungsTyp Entscheidung);

public sealed record B56AlleAkzeptiertAntwort(
    int AkzeptierteKonflikte);
