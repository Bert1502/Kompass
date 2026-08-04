using Kompass.Application.Projects;
using Kompass.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Projects;

[ApiController]
[Route("api/projekte")]
public sealed class ProjekteController : ControllerBase
{
    private readonly IProjektService _projektService;
    private readonly ILogger<ProjekteController> _logger;

    public ProjekteController(
        IProjektService projektService,
        ILogger<ProjekteController> logger)
    {
        _projektService = projektService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ProjektUebersicht>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjektUebersicht>>>
        AlleAbrufenAsync(
            CancellationToken cancellationToken)
    {
        var projekte =
            await _projektService.AlleAbrufenAsync(
                cancellationToken);

        return Ok(projekte);
    }

    [HttpGet("{id:guid}", Name = "ProjektNachIdAbrufen")]
    [ProducesResponseType(
        typeof(ProjektUebersicht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjektUebersicht>>
        NachIdAbrufenAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        var projekt =
            await _projektService.NachIdAbrufenAsync(
                id,
                cancellationToken);

        if (projekt is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Das Projekt mit der ID '{id}' wurde nicht gefunden."
            });
        }

        return Ok(projekt);
    }

    [HttpGet("{id:guid}/alternativen")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AlternativeKurzinfo>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlternativeKurzinfo>>>
        AlternativenAbrufenAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        var alternativen =
            await _projektService.AlternativenAbrufenAsync(
                id,
                cancellationToken);

        return Ok(alternativen);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ProjektUebersicht),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjektUebersicht>>
        ErstellenAsync(
            [FromBody] ProjektErstellenRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var projekt =
                await _projektService.ErstellenAsync(
                    request.Name,
                    cancellationToken);

            return CreatedAtRoute(
                "ProjektNachIdAbrufen",
                new { id = projekt.Id },
                projekt);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Projekt konnte wegen eines ungültigen Namens nicht erstellt werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(
                exception,
                "Projekt konnte wegen eines Konflikts nicht erstellt werden.");

            return Conflict(new
            {
                Nachricht = exception.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(ProjektUebersicht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjektUebersicht>>
        AktualisierenAsync(
            Guid id,
            [FromBody] ProjektAktualisierenRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var projekt =
                await _projektService.AktualisierenAsync(
                    id,
                    request.Name,
                    cancellationToken);

            if (projekt is null)
            {
                return NotFound(new
                {
                    Nachricht =
                        $"Das Projekt mit der ID '{id}' wurde nicht gefunden."
                });
            }

            return Ok(projekt);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Projekt konnte wegen ungültiger Daten nicht aktualisiert werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(
                exception,
                "Projekt konnte wegen eines Konflikts nicht aktualisiert werden.");

            return Conflict(new
            {
                Nachricht = exception.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LoeschenAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var wurdeGeloescht =
            await _projektService.LoeschenAsync(
                id,
                cancellationToken);

        if (!wurdeGeloescht)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Das Projekt mit der ID '{id}' wurde nicht gefunden."
            });
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/stammdaten")]
    [ProducesResponseType(
        typeof(ProjektUebersicht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjektUebersicht>>
        StammdatenAktualisierenAsync(
            Guid id,
            [FromBody] ProjektStammdatenAktualisierenRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var projekt =
                await _projektService.StammdatenAktualisierenAsync(
                    id,
                    request.Auftraggeber,
                    request.Ansprechpartner,
                    request.Strasse,
                    request.Ort,
                    request.Postleitzahl,
                    request.Gebaeudeart,
                    cancellationToken);

            if (projekt is null)
            {
                return NotFound(new
                {
                    Nachricht =
                        $"Das Projekt mit der ID '{id}' wurde nicht gefunden."
                });
            }

            return Ok(projekt);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Projektstammdaten konnten wegen ungültiger Daten nicht aktualisiert werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
    }

    [HttpPatch("{id:guid}/projektdaten")]    [ProducesResponseType(
        typeof(ProjektUebersicht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjektUebersicht>>
        ProjektdatenAktualisierenAsync(
            Guid id,
            [FromBody] ProjektdatenAktualisierenRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var projekt =
                await _projektService.ProjektdatenAktualisierenAsync(
                    id,
                    request.InterneBezeichnung,
                    request.Bearbeitungsstatus,
                    cancellationToken);

            if (projekt is null)
            {
                return NotFound(new
                {
                    Nachricht =
                        $"Das Projekt mit der ID '{id}' wurde nicht gefunden."
                });
            }

            return Ok(projekt);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Projektdaten konnten wegen ungültiger Daten nicht aktualisiert werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
    }

    [HttpPatch("{id:guid}/freigabestatus")]
    [ProducesResponseType(
        typeof(ProjektUebersicht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjektUebersicht>>
        FreigabestatusAktualisierenAsync(
            Guid id,
            [FromBody] ProjektFreigabestatusAktualisierenRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var projekt =
                await _projektService.FreigabestatusAktualisierenAsync(
                    id,
                    request.Freigabestatus,
                    cancellationToken);

            if (projekt is null)
            {
                return NotFound(new
                {
                    Nachricht =
                        $"Das Projekt mit der ID '{id}' wurde nicht gefunden."
                });
            }

            return Ok(projekt);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Freigabestatus konnte wegen ungültiger Daten nicht aktualisiert werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
    }

    [HttpPatch("{id:guid}/notizen")]
    [ProducesResponseType(
        typeof(ProjektUebersicht),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjektUebersicht>>
        NotizenAktualisierenAsync(
            Guid id,
            [FromBody] ProjektNotizenAktualisierenRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var projekt =
                await _projektService.NotizenAktualisierenAsync(
                    id,
                    request.Notizen,
                    cancellationToken);

            if (projekt is null)
            {
                return NotFound(new
                {
                    Nachricht =
                        $"Das Projekt mit der ID '{id}' wurde nicht gefunden."
                });
            }

            return Ok(projekt);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning(
                exception,
                "Notizen konnten wegen ungültiger Daten nicht aktualisiert werden.");

            return BadRequest(new
            {
                Nachricht = exception.Message
            });
        }
    }
}
