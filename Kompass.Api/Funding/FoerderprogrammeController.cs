using Kompass.Application.Funding;
using Kompass.Domain.Common;
using Kompass.Domain.Funding;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Funding;

[ApiController]
[Route("api/foerderprogramme")]
public sealed class FoerderprogrammeController : ControllerBase
{
    private readonly IFoerderprogrammService _foerderprogrammService;
    private readonly ILogger<FoerderprogrammeController> _logger;

    public FoerderprogrammeController(
        IFoerderprogrammService foerderprogrammService,
        ILogger<FoerderprogrammeController> logger)
    {
        _foerderprogrammService = foerderprogrammService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<Foerderprogramm>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Foerderprogramm>>> ListenAsync(
        CancellationToken cancellationToken)
    {
        var programme =
            await _foerderprogrammService.ListenAsync(
                cancellationToken);

        return Ok(programme);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(Foerderprogramm),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Foerderprogramm>> AnlegenAsync(
        [FromBody] FoerderprogrammAnlegenRequest anfrage,
        CancellationToken cancellationToken)
    {
        try
        {
            var foerderprogramm = new Foerderprogramm(
                Guid.NewGuid(),
                anfrage.Programmkennung,
                anfrage.Version,
                anfrage.GueltigAb,
                anfrage.GueltigBis,
                anfrage.Zielgruppe,
                anfrage.Foerdergegenstand,
                anfrage.TechnischeMindestanforderungen,
                anfrage.Foerdersatz,
                anfrage.Hoechstbetrag,
                anfrage.Kumulierbarkeit,
                anfrage.Pflichtnachweise,
                anfrage.Quellenstand,
                anfrage.Foerderquoten.Select(
                    regel => new FoerderquoteRegel(
                        Guid.NewGuid(),
                        regel.Bezeichnung,
                        regel.Quote,
                        regel.Bezugsbasis,
                        regel.GueltigAb,
                        regel.GueltigBis,
                        regel.Beschreibung)),
                anfrage.Hoechstbetraege.Select(
                    regel => new HoechstbetragRegel(
                        Guid.NewGuid(),
                        regel.Bezeichnung,
                        regel.Betrag,
                        regel.Waehrung,
                        regel.Bezugsbasis,
                        regel.GueltigAb,
                        regel.GueltigBis,
                        regel.Beschreibung)),
                anfrage.Kumulierbarkeitsregeln.Select(
                    regel => new Kumulierbarkeitsregel(
                        Guid.NewGuid(),
                        regel.Bezeichnung,
                        regel.Status,
                        regel.Beschreibung,
                        regel.GueltigAb,
                        regel.GueltigBis)),
                anfrage.Pflichtnachweisregeln.Select(
                    regel => new PflichtnachweisRegel(
                        Guid.NewGuid(),
                        regel.Bezeichnung,
                        regel.Beschreibung,
                        regel.Zeitpunkt,
                        regel.IstPflicht,
                        regel.GueltigAb,
                        regel.GueltigBis)),
                anfrage.Gueltigkeitsregeln.Select(
                    regel => new Gueltigkeitsregel(
                        Guid.NewGuid(),
                        regel.Bezeichnung,
                        regel.Bezug,
                        regel.GueltigAb,
                        regel.GueltigBis,
                        regel.Beschreibung)));

            var gespeichertes =
                await _foerderprogrammService.AnlegenAsync(
                    foerderprogramm,
                    cancellationToken);

            return CreatedAtAction(
                nameof(ListenAsync),
                new { },
                gespeichertes);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Förderprogramm konnte wegen ungültiger Daten nicht gespeichert werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
    }
}
