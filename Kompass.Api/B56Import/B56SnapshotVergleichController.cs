using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
[Route("api/projekte/{projektId:guid}/b56-importe/{nachfolgerImportId:guid}/vergleich")]
public sealed class B56SnapshotVergleichController : ControllerBase
{
    private readonly IB56SnapshotVergleichService _vergleichService;

    public B56SnapshotVergleichController(
        IB56SnapshotVergleichService vergleichService)
    {
        _vergleichService = vergleichService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(B56SnapshotVergleichAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<B56SnapshotVergleichAntwort>>
        VergleichenAsync(
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

        var ergebnis =
            await _vergleichService.VergleichenAsync(
                projektId,
                vorgaenger,
                nachfolgerImportId,
                cancellationToken);

        if (ergebnis.Status ==
            B56SnapshotVergleichStatus.NichtGefunden)
        {
            return NotFound(new
            {
                ergebnis.Nachricht
            });
        }

        return Ok(
            B56SnapshotVergleichAntwort.Aus(
                ergebnis.Vergleich!));
    }
}

public sealed record B56SnapshotVergleichAntwort(
    Guid ProjektId,
    Guid VorgaengerSnapshotId,
    Guid NachfolgerSnapshotId,
    bool HatAenderungen,
    IReadOnlyList<B56KennwertVergleichAntwort> BestandskennwertVergleiche,
    IReadOnlyList<B56AlternativeVergleichAntwort> AlternativVergleiche,
    IReadOnlyList<B56BauteilVergleichAntwort> GesamtbauteilVergleiche,
    IReadOnlyList<B56VergleichskonfliktAntwort> Konflikte)
{
    public static B56SnapshotVergleichAntwort Aus(
        B56SnapshotVergleich vergleich)
    {
        return new B56SnapshotVergleichAntwort(
            vergleich.ProjektId,
            vergleich.VorgaengerSnapshotId,
            vergleich.NachfolgerSnapshotId,
            vergleich.HatAenderungen,
            vergleich.BestandskennwertVergleiche
                .Select(B56KennwertVergleichAntwort.Aus)
                .ToList(),
            vergleich.AlternativVergleiche
                .Select(B56AlternativeVergleichAntwort.Aus)
                .ToList(),
            vergleich.GesamtbauteilVergleiche
                .Select(B56BauteilVergleichAntwort.Aus)
                .ToList(),
            vergleich.Konflikte
                .Select(B56VergleichskonfliktAntwort.Aus)
                .ToList());
    }
}

public sealed record B56KennwertVergleichAntwort(
    string Name,
    string Einheit,
    double? AlterWert,
    double? NeuerWert,
    B56VergleichsAenderung Aenderung)
{
    public static B56KennwertVergleichAntwort Aus(
        B56KennwertVergleich kennwert)
    {
        return new B56KennwertVergleichAntwort(
            kennwert.Name,
            kennwert.Einheit,
            kennwert.AlterWert,
            kennwert.NeuerWert,
            kennwert.Aenderung);
    }
}

public sealed record B56BauteilVergleichAntwort(
    string Bauteilcode,
    string Bezeichnung,
    double? AlterUWert,
    double? NeuerUWert,
    double? AlteFlaeche,
    double? NeueFlaeche,
    B56VergleichsAenderung Aenderung)
{
    public static B56BauteilVergleichAntwort Aus(
        B56BauteilVergleich bauteil)
    {
        return new B56BauteilVergleichAntwort(
            bauteil.Bauteilcode,
            bauteil.Bezeichnung,
            bauteil.AlterUWert,
            bauteil.NeuerUWert,
            bauteil.AlteFlaeche,
            bauteil.NeueFlaeche,
            bauteil.Aenderung);
    }
}

public sealed record B56AlternativeVergleichAntwort(
    int B56Position,
    string AlteBezeichnung,
    string NeueBezeichnung,
    B56VergleichsAenderung Aenderung,
    IReadOnlyList<B56KennwertVergleichAntwort> KennwertVergleiche,
    IReadOnlyList<B56BauteilVergleichAntwort> BauteilVergleiche)
{
    public static B56AlternativeVergleichAntwort Aus(
        B56AlternativeVergleich alternative)
    {
        return new B56AlternativeVergleichAntwort(
            alternative.B56Position,
            alternative.AlteBezeichnung,
            alternative.NeueBezeichnung,
            alternative.Aenderung,
            alternative.KennwertVergleiche
                .Select(B56KennwertVergleichAntwort.Aus)
                .ToList(),
            alternative.BauteilVergleiche
                .Select(B56BauteilVergleichAntwort.Aus)
                .ToList());
    }
}

public sealed record B56VergleichskonfliktAntwort(
    string Bereich,
    string Schluessel,
    string Feld,
    B56VergleichsAenderung Aenderung)
{
    public static B56VergleichskonfliktAntwort Aus(
        B56Vergleichskonflikt konflikt)
    {
        return new B56VergleichskonfliktAntwort(
            konflikt.Bereich,
            konflikt.Schluessel,
            konflikt.Feld,
            konflikt.Aenderung);
    }
}
