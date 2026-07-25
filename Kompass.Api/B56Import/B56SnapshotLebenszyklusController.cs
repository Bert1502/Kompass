using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
[Route("api/projekte/{projektId:guid}/b56-importe/{importId:guid}")]
public sealed class B56SnapshotLebenszyklusController : ControllerBase
{
    private readonly IB56SnapshotLebenszyklusService
        _lebenszyklusService;

    public B56SnapshotLebenszyklusController(
        IB56SnapshotLebenszyklusService lebenszyklusService)
    {
        _lebenszyklusService = lebenszyklusService;
    }

    [HttpPost("bestaetigen")]
    [ProducesResponseType(
        typeof(B56SnapshotAktionAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(B56SnapshotAktionAntwort),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(B56SnapshotAktionAntwort),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<B56SnapshotAktionAntwort>>
        BestaetigenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken)
    {
        var ergebnis =
            await _lebenszyklusService.BestaetigenAsync(
                projektId,
                importId,
                cancellationToken);

        return ErzeugeAntwort(
            ergebnis);
    }

    [HttpPost("verwerfen")]
    [ProducesResponseType(
        typeof(B56SnapshotAktionAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(B56SnapshotAktionAntwort),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(B56SnapshotAktionAntwort),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<B56SnapshotAktionAntwort>>
        VerwerfenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken)
    {
        var ergebnis =
            await _lebenszyklusService.VerwerfenAsync(
                projektId,
                importId,
                cancellationToken);

        return ErzeugeAntwort(
            ergebnis);
    }

    private ActionResult<B56SnapshotAktionAntwort> ErzeugeAntwort(
        B56SnapshotAktionErgebnis ergebnis)
    {
        var antwort =
            B56SnapshotAktionAntwort.Aus(
                ergebnis);

        return ergebnis.Status switch
        {
            B56SnapshotAktionStatus.Erfolgreich =>
                Ok(antwort),
            B56SnapshotAktionStatus.NichtGefunden =>
                NotFound(antwort),
            _ =>
                Conflict(antwort)
        };
    }
}

public sealed record B56SnapshotAktionAntwort(
    B56SnapshotAktionStatus Status,
    Guid? ImportId,
    B56SnapshotStatus? SnapshotStatus,
    DateTimeOffset? BestaetigtAm,
    DateTimeOffset? VerworfenAm,
    string Nachricht)
{
    public static B56SnapshotAktionAntwort Aus(
        B56SnapshotAktionErgebnis ergebnis)
    {
        return new B56SnapshotAktionAntwort(
            ergebnis.Status,
            ergebnis.Snapshot?.ImportId,
            ergebnis.Snapshot?.SnapshotStatus,
            ergebnis.Snapshot?.BestaetigtAm,
            ergebnis.Snapshot?.VerworfenAm,
            ergebnis.Nachricht);
    }
}
