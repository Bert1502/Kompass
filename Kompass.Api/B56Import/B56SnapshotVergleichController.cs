using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
[Route("api/projekte/{projektId:guid}/b56-importe")]
public sealed class B56SnapshotVergleichController : ControllerBase
{
    private readonly IB56SnapshotVergleichService _vergleichService;

    public B56SnapshotVergleichController(
        IB56SnapshotVergleichService vergleichService)
    {
        _vergleichService = vergleichService;
    }

    [HttpGet("vergleich")]
    [ProducesResponseType(
        typeof(B56SnapshotVergleichAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<B56SnapshotVergleichAntwort>>
        VergleichenAsync(
            Guid projektId,
            [FromQuery] Guid altSnapshotId,
            [FromQuery] Guid neuSnapshotId,
            CancellationToken cancellationToken)
    {
        var aktionsErgebnis =
            await _vergleichService.VergleichenAsync(
                projektId,
                altSnapshotId,
                neuSnapshotId,
                cancellationToken);

        if (aktionsErgebnis.Status ==
            B56SnapshotVergleichStatus.NichtGefunden)
        {
            return NotFound(new
            {
                Nachricht = aktionsErgebnis.Nachricht
            });
        }

        return Ok(
            B56SnapshotVergleichAntwort.Aus(
                aktionsErgebnis.Ergebnis!));
    }
}

public sealed record B56SnapshotVergleichAntwort(
    Guid AltSnapshotId,
    Guid NeuSnapshotId,
    IReadOnlyList<B56AlternativenVergleichAntwort> Alternativen,
    IReadOnlyList<B56KennwertVergleichAntwort> Bestandskennwerte,
    IReadOnlyList<B56BauteilVergleichAntwort> Bauteile)
{
    public static B56SnapshotVergleichAntwort Aus(
        B56SnapshotVergleichErgebnis ergebnis)
    {
        return new B56SnapshotVergleichAntwort(
            ergebnis.AltSnapshotId,
            ergebnis.NeuSnapshotId,
            ergebnis.Alternativen
                .Select(B56AlternativenVergleichAntwort.Aus)
                .ToList(),
            ergebnis.Bestandskennwerte
                .Select(B56KennwertVergleichAntwort.Aus)
                .ToList(),
            ergebnis.Bauteile
                .Select(B56BauteilVergleichAntwort.Aus)
                .ToList());
    }
}

public sealed record B56AlternativenVergleichAntwort(
    int Position,
    B56VergleichsArt Art,
    string? AlteBezeichnung,
    string? NeueBezeichnung,
    IReadOnlyList<B56KennwertVergleichAntwort> Kennwerte,
    IReadOnlyList<B56BauteilVergleichAntwort> Bauteile)
{
    public static B56AlternativenVergleichAntwort Aus(
        B56AlternativenVergleich vergleich)
    {
        return new B56AlternativenVergleichAntwort(
            vergleich.Position,
            vergleich.Art,
            vergleich.AlteBezeichnung,
            vergleich.NeueBezeichnung,
            vergleich.Kennwerte
                .Select(B56KennwertVergleichAntwort.Aus)
                .ToList(),
            vergleich.Bauteile
                .Select(B56BauteilVergleichAntwort.Aus)
                .ToList());
    }
}

public sealed record B56KennwertVergleichAntwort(
    string Name,
    string Einheit,
    B56VergleichsArt Art,
    double? AlterWert,
    double? NeuerWert)
{
    public static B56KennwertVergleichAntwort Aus(
        B56KennwertVergleich vergleich)
    {
        return new B56KennwertVergleichAntwort(
            vergleich.Name,
            vergleich.Einheit,
            vergleich.Art,
            vergleich.AlterWert,
            vergleich.NeuerWert);
    }
}

public sealed record B56BauteilVergleichAntwort(
    string Bauteilcode,
    B56VergleichsArt Art,
    string? AlteBezeichnung,
    string? NeueBezeichnung,
    double? AlterUWert,
    double? NeuerUWert,
    double? AlteFlaeche,
    double? NeueFlaeche)
{
    public static B56BauteilVergleichAntwort Aus(
        B56BauteilVergleich vergleich)
    {
        return new B56BauteilVergleichAntwort(
            vergleich.Bauteilcode,
            vergleich.Art,
            vergleich.AlteBezeichnung,
            vergleich.NeueBezeichnung,
            vergleich.AlterUWert,
            vergleich.NeuerUWert,
            vergleich.AlteFlaeche,
            vergleich.NeueFlaeche);
    }
}
