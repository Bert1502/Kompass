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
    private readonly IB56ImportRegister _importRegister;

    public B56ImportController(
        IProjektService projektService,
        IB56ImportService importService,
        IB56ImportRegister importRegister)
    {
        _projektService = projektService;
        _importService = importService;
        _importRegister = importRegister;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<B56ImportHistorieAntwort>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<B56ImportHistorieAntwort>>>
        HistorieAbrufenAsync(
            Guid projektId,
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

        var eintraege =
            await _importRegister
                .AlleFuerProjektAbrufenAsync(
                    projektId,
                    cancellationToken);

        return Ok(
            eintraege.Select(
                B56ImportHistorieAntwort.Aus));
    }

    [HttpGet("{importId:guid}")]
    [ProducesResponseType(
        typeof(B56ImportPipelineAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<B56ImportPipelineAntwort>>
        DetailsAbrufenAsync(
            Guid projektId,
            Guid importId,
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

        var fachdaten =
            await _importRegister.FachdatenAbrufenAsync(
                projektId,
                importId,
                cancellationToken);

        if (fachdaten is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Für den B56-Import mit der ID '{importId}' wurden keine Fachdaten gefunden."
            });
        }

        return Ok(
            B56ImportPipelineAntwort.Aus(
                fachdaten));
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

public sealed record B56ImportHistorieAntwort(
    Guid ImportId,
    Guid ProjektId,
    string Originaldateiname,
    string Sha256,
    long DateigroesseBytes,
    DateTimeOffset ImportiertAm,
    string Dateiendung,
    int SnapshotSchemaVersion,
    string ParserVersion,
    B56SnapshotStatus SnapshotStatus,
    DateTimeOffset? BestaetigtAm,
    DateTimeOffset? VerworfenAm)
{
    public static B56ImportHistorieAntwort Aus(
        B56ImportEintrag eintrag)
    {
        return new B56ImportHistorieAntwort(
            eintrag.ImportId,
            eintrag.ProjektId,
            eintrag.Originaldateiname,
            eintrag.Sha256,
            eintrag.DateigroesseBytes,
            eintrag.ImportiertAm,
            eintrag.Dateiendung,
            eintrag.SnapshotSchemaVersion,
            eintrag.ParserVersion,
            eintrag.SnapshotStatus,
            eintrag.BestaetigtAm,
            eintrag.VerworfenAm);
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
    int? SnapshotSchemaVersion,
    string? ParserVersion,
    B56SnapshotStatus? SnapshotStatus,
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
            eintrag?.SnapshotSchemaVersion,
            eintrag?.ParserVersion,
            eintrag?.SnapshotStatus,
            B56ImportPipelineAntwort.Aus(
                ergebnis.PipelineErgebnis),
            ergebnis.Meldungen);
    }
}

public sealed record B56ImportPipelineAntwort(
    int ImportierteArbeitsblaetter,
    int ErkannteTabellen,
    int ImportierteTabellen,
    int ImportierteBauteile,
    int ImportierteKennwerte,
    int ImportierteModernisierungsalternativen,
    IReadOnlyList<B56BauteilAntwort> Bauteile,
    IReadOnlyList<B56KennwertAntwort> Bestandskennwerte,
    IReadOnlyList<B56ModernisierungsalternativeAntwort>
        Modernisierungsalternativen,
    IReadOnlyList<string> Warnungen,
    IReadOnlyList<string> BlockierendeFehler)
{
    public static B56ImportPipelineAntwort? Aus(
        B56ImportPipelineErgebnis? ergebnis)
    {
        return ergebnis is null
            ? null
            : new B56ImportPipelineAntwort(
                ergebnis.ImportierteArbeitsblaetter,
                ergebnis.ErkannteTabellen,
                ergebnis.ImportierteTabellen,
                ergebnis.ImportierteBauteile,
                ergebnis.ImportierteKennwerte,
                ergebnis.ImportierteModernisierungsalternativen,
                ergebnis.Bauteile
                    .Select(
                        B56BauteilAntwort.Aus)
                    .ToList(),
                ergebnis.Bestandskennwerte
                    .Select(
                        B56KennwertAntwort.Aus)
                    .ToList(),
                ergebnis.Modernisierungsalternativen
                    .Select(
                        B56ModernisierungsalternativeAntwort.Aus)
                    .ToList(),
                ergebnis.Warnungen,
                ergebnis.BlockierendeFehler);
    }
}

public sealed record B56BauteilAntwort(
    string Bauteilcode,
    string Bezeichnung,
    string Nachbarseite,
    double Flaeche,
    double UWert)
{
    public static B56BauteilAntwort Aus(
        B56Bauteil bauteil)
    {
        return new B56BauteilAntwort(
            bauteil.Bauteilcode,
            bauteil.Bezeichnung,
            bauteil.Nachbarseite,
            bauteil.Flaeche,
            bauteil.UWert);
    }
}

public sealed record B56KennwertAntwort(
    string Name,
    string Einheit,
    double Wert)
{
    public static B56KennwertAntwort Aus(
        B56Kennwert kennwert)
    {
        return new B56KennwertAntwort(
            kennwert.Name,
            kennwert.Einheit,
            kennwert.Wert);
    }
}

public sealed record B56ModernisierungsalternativeAntwort(
    string Bezeichnung,
    string Beschreibung,
    IReadOnlyList<B56BauteilAntwort> Bauteile,
    IReadOnlyList<B56KennwertAntwort> Kennwerte)
{
    public static B56ModernisierungsalternativeAntwort Aus(
        B56Modernisierungsalternative alternative)
    {
        return new B56ModernisierungsalternativeAntwort(
            alternative.Bezeichnung,
            alternative.Beschreibung,
            alternative.Bauteile
                .Select(
                    B56BauteilAntwort.Aus)
                .ToList(),
            alternative.Kennwerte
                .Select(
                    B56KennwertAntwort.Aus)
                .ToList());
    }
}
