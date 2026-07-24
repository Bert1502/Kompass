using Kompass.Application.B56Import;
using Kompass.Application.Projects;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.B56Import;

[ApiController]
[Route("api/projekte/{projektId:guid}/b56-importe")]
public sealed class B56ImportController : ControllerBase
{
    private readonly IProjektService _projektService;
    private readonly IB56ImportService _importService;

    public B56ImportController(
        IProjektService projektService,
        IB56ImportService importService)
    {
        _projektService = projektService;
        _importService = importService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(B56ImportAntwort),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(B56ImportAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(B56ImportAntwort),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(B56ImportAntwort),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<B56ImportAntwort>> ImportierenAsync(
        Guid projektId,
        IFormFile datei,
        CancellationToken cancellationToken)
    {
        var projekt =
            await _projektService.NachIdAbrufenAsync(
                projektId,
                cancellationToken);

        if (projekt is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Das Projekt mit der ID '{projektId}' wurde nicht gefunden."
            });
        }

        if (datei is null)
        {
            return BadRequest(new
            {
                Nachricht = "Es wurde keine B56-Datei hochgeladen."
            });
        }

        var temporaererDateipfad =
            ErzeugeTemporaerenDateipfad(
                datei.FileName);

        try
        {
            await using (var dateiStream =
                System.IO.File.Create(
                    temporaererDateipfad))
            {
                await datei.CopyToAsync(
                    dateiStream,
                    cancellationToken);
            }

            var ergebnis =
                await _importService.ImportierenAsync(
                    new B56ImportAnfrage(
                        projekt.Id,
                        projekt.Name,
                        temporaererDateipfad),
                    cancellationToken);

            var antwort =
                B56ImportAntwort.Aus(
                    ergebnis);

            return ergebnis.Status switch
            {
                B56ImportStatus.Erfolgreich =>
                    StatusCode(
                        StatusCodes.Status201Created,
                        antwort),

                B56ImportStatus.BereitsImportiert =>
                    Ok(antwort),

                B56ImportStatus.Abgelehnt =>
                    BadRequest(antwort),

                _ =>
                    StatusCode(
                        StatusCodes.Status500InternalServerError,
                        antwort)
            };
        }
        finally
        {
            VersucheTemporaereDateiZuLoeschen(
                temporaererDateipfad);
        }
    }

    private static string ErzeugeTemporaerenDateipfad(
        string originaldateiname)
    {
        var sichererDateiname =
            Path.GetFileName(
                originaldateiname);

        if (string.IsNullOrWhiteSpace(
                sichererDateiname))
        {
            sichererDateiname =
                "b56-upload";
        }

        var temporaeresVerzeichnis =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            temporaeresVerzeichnis);

        return Path.Combine(
            temporaeresVerzeichnis,
            sichererDateiname);
    }

    private static void VersucheTemporaereDateiZuLoeschen(
        string dateipfad)
    {
        try
        {
            System.IO.File.Delete(
                dateipfad);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        var temporaeresVerzeichnis =
            Path.GetDirectoryName(
                dateipfad);

        if (temporaeresVerzeichnis is null)
        {
            return;
        }

        try
        {
            Directory.Delete(
                temporaeresVerzeichnis);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

public sealed record B56ImportAntwort(
    B56ImportStatus Status,
    Guid ProjektId,
    Guid? ImportId,
    string? Originaldateiname,
    string? Sha256,
    long? DateigroesseBytes,
    DateTimeOffset? ImportiertAm,
    B56ImportPipelineAntwort? Pipeline,
    IReadOnlyList<B56ImportMeldung> Meldungen)
{
    public static B56ImportAntwort Aus(
        B56ImportErgebnis ergebnis)
    {
        var eintrag =
            ergebnis.ImportEintrag;

        return new B56ImportAntwort(
            ergebnis.Status,
            ergebnis.ProjektId,
            eintrag?.ImportId,
            eintrag?.Originaldateiname,
            eintrag?.Sha256,
            eintrag?.DateigroesseBytes,
            eintrag?.ImportiertAm,
            B56ImportPipelineAntwort.Aus(
                ergebnis.PipelineErgebnis),
            ergebnis.Meldungen);
    }
}

public sealed record B56ImportPipelineAntwort(
    int ImportierteArbeitsblaetter,
    int ImportierteTabellen,
    int ImportierteBauteile,
    int ImportierteKennwerte,
    int ImportierteModernisierungsalternativen,
    IReadOnlyList<string> Warnungen)
{
    public static B56ImportPipelineAntwort? Aus(
        B56ImportPipelineErgebnis? ergebnis)
    {
        return ergebnis is null
            ? null
            : new B56ImportPipelineAntwort(
                ergebnis.ImportierteArbeitsblaetter,
                ergebnis.ImportierteTabellen,
                ergebnis.ImportierteBauteile,
                ergebnis.ImportierteKennwerte,
                ergebnis.ImportierteModernisierungsalternativen,
                ergebnis.Warnungen);
    }
}
