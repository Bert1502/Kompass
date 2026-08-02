using Kompass.Application.FachdatenImport;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Fachdaten;

[ApiController]
[Route("api/fachdatenbanken")]
public sealed class FachdatenbankenController : ControllerBase
{
    private readonly IFachdatenbankImportService _service;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public FachdatenbankenController(
        IFachdatenbankImportService service,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _service = service;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet("pruefen")]
    public async Task<ActionResult<FachdatenimportErgebnis>> Pruefen(CancellationToken cancellationToken)
    {
        var verzeichnis = KonfiguriertesVerzeichnis();
        if (verzeichnis is null) return Problem("Das Fachdatenbankverzeichnis ist nicht konfiguriert.", statusCode: StatusCodes.Status503ServiceUnavailable);
        return Ok(await _service.PruefenAsync(verzeichnis, cancellationToken));
    }

    [HttpPost("importieren")]
    public async Task<ActionResult<FachdatenimportErgebnis>> Importieren(CancellationToken cancellationToken)
    {
        var verzeichnis = KonfiguriertesVerzeichnis();
        if (verzeichnis is null) return Problem("Das Fachdatenbankverzeichnis ist nicht konfiguriert.", statusCode: StatusCodes.Status503ServiceUnavailable);
        var pruefung = await _service.PruefenAsync(verzeichnis, cancellationToken);
        if (!pruefung.IstGueltig) return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["Fachdatenbanken"] = pruefung.Datenbanken.SelectMany(x => x.Fehler).ToArray() }));
        return Ok(await _service.ImportierenAsync(verzeichnis, cancellationToken));
    }

    private string? KonfiguriertesVerzeichnis()
    {
        var value = _configuration["Fachdatenbanken:Verzeichnis"];
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Path.GetFullPath(
            Path.IsPathRooted(value)
                ? value
                : Path.Combine(_environment.ContentRootPath, value));
    }
}
