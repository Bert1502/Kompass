using Kompass.Api.Contracts;
using Kompass.Application.Projects;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Controllers;

[ApiController]
[Route("api/projekte")]
public sealed class ProjekteController : ControllerBase
{
    private readonly IProjektService _projektService;

    public ProjekteController(IProjektService projektService)
    {
        _projektService = projektService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjektAntwort>>> AlleAbrufen(
        CancellationToken cancellationToken)
    {
        var projekte =
            await _projektService.AlleAbrufenAsync(cancellationToken);

        var antwort = projekte
            .Select(Map)
            .ToList();

        return Ok(antwort);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjektAntwort>> NachIdAbrufen(
        Guid id,
        CancellationToken cancellationToken)
    {
        var projekt =
            await _projektService.NachIdAbrufenAsync(
                id,
                cancellationToken);

        if (projekt is null)
        {
            return NotFound();
        }

        return Ok(Map(projekt));
    }

    [HttpPost]
    public async Task<ActionResult<ProjektAntwort>> Erstellen(
        ProjektErstellenAnfrage anfrage,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(anfrage.Name))
        {
            return BadRequest("Der Projektname darf nicht leer sein.");
        }

        var projekt =
            await _projektService.ErstellenAsync(
                anfrage.Name,
                cancellationToken);

        var antwort = Map(projekt);

        return CreatedAtAction(
            nameof(NachIdAbrufen),
            new { id = antwort.Id },
            antwort);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Loeschen(
    Guid id,
    CancellationToken cancellationToken)
    {
        var geloescht = await _projektService.LoeschenAsync(
            id,
            cancellationToken);

        if (!geloescht)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static ProjektAntwort Map(
        ProjektUebersicht projekt)
    {
        return new ProjektAntwort(
            projekt.Id,
            projekt.Name,
            projekt.AnzahlAlternativen);
    }
}