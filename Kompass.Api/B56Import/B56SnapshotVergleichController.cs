using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
<<<<<<< HEAD
[Route("api/projekte/{projektId:guid}/b56-importe/{nachfolgerImportId:guid}/vergleich")]
=======
[Route("api/projekte/{projektId:guid}/b56-importe")]
>>>>>>> origin/main
public sealed class B56SnapshotVergleichController : ControllerBase
{
    private readonly IB56SnapshotVergleichService _vergleichService;

    public B56SnapshotVergleichController(
        IB56SnapshotVergleichService vergleichService)
    {
        _vergleichService = vergleichService;
    }

<<<<<<< HEAD
    [HttpGet]
=======
    [HttpGet("vergleich")]
>>>>>>> origin/main
    [ProducesResponseType(
        typeof(B56SnapshotVergleichAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<B56SnapshotVergleichAntwort>>
        VergleichenAsync(
            Guid projektId,
<<<<<<< HEAD
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
=======
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
>>>>>>> origin/main
            B56SnapshotVergleichStatus.NichtGefunden)
        {
            return NotFound(new
            {
<<<<<<< HEAD
                ergebnis.Nachricht
=======
                Nachricht = aktionsErgebnis.Nachricht
>>>>>>> origin/main
            });
        }

        return Ok(
            B56SnapshotVergleichAntwort.Aus(
<<<<<<< HEAD
                ergebnis.Vergleich!));
=======
                aktionsErgebnis.Ergebnis!));
>>>>>>> origin/main
    }
}

public sealed record B56SnapshotVergleichAntwort(
<<<<<<< HEAD
    Guid ProjektId,
    Guid VorgaengerSnapshotId,
    Guid NachfolgerSnapshotId,
    bool HatAenderungen,
    IReadOnlyList<B56KennwertVergleichAntwort> BestandskennwertVergleiche,
    IReadOnlyList<B56AlternativeVergleichAntwort> AlternativVergleiche,
    IReadOnlyList<B56BauteilVergleichAntwort> GesamtbauteilVergleiche)
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
=======
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
>>>>>>> origin/main
                .Select(B56BauteilVergleichAntwort.Aus)
                .ToList());
    }
}

public sealed record B56KennwertVergleichAntwort(
    string Name,
    string Einheit,
<<<<<<< HEAD
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
=======
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
>>>>>>> origin/main
    }
}

public sealed record B56BauteilVergleichAntwort(
    string Bauteilcode,
<<<<<<< HEAD
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
=======
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
>>>>>>> origin/main
    }
}
