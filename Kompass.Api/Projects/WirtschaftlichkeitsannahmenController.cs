using Kompass.Application.Economics;
using Kompass.Application.Projects;
using Kompass.Domain.Common;
using Kompass.Domain.Economics;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Api.Projects;

/// <summary>
/// Verwaltung der Wirtschaftlichkeitsannahmen und -berechnung für eine
/// Modernisierungsalternative.
/// </summary>
[ApiController]
[Route(
    "api/projekte/{projektId:guid}/alternativen/{alternativeId:guid}")]
public sealed class WirtschaftlichkeitsannahmenController : ControllerBase
{
    private readonly IProjektService _projektService;
    private readonly IWirtschaftlichkeitsannahmenRepository _repository;
    private readonly WirtschaftlichkeitsberechnungsService _berechnungsService;

    public WirtschaftlichkeitsannahmenController(
        IProjektService projektService,
        IWirtschaftlichkeitsannahmenRepository repository,
        WirtschaftlichkeitsberechnungsService berechnungsService)
    {
        _projektService = projektService;
        _repository = repository;
        _berechnungsService = berechnungsService;
    }

    /// <summary>
    /// Gibt die Wirtschaftlichkeitsannahmen für eine Modernisierungsalternative zurück.
    /// </summary>
    [HttpGet("wirtschaftlichkeitsannahmen")]
    [ProducesResponseType(
        typeof(WirtschaftlichkeitsannahmenAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WirtschaftlichkeitsannahmenAntwort>>
        AnnahmenAbrufenAsync(
            Guid projektId,
            Guid alternativeId,
            CancellationToken cancellationToken)
    {
        var alternative =
            await _projektService.AlternativeNachIdAbrufenAsync(
                projektId,
                alternativeId,
                cancellationToken);

        if (alternative is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Die Alternative '{alternativeId}' wurde im Projekt '{projektId}' nicht gefunden."
            });
        }

        var annahmen =
            await _repository.NachAlternativeIdAbrufenAsync(
                alternativeId,
                cancellationToken);

        if (annahmen is null)
        {
            return NotFound(new
            {
                Nachricht =
                    "Für diese Modernisierungsalternative sind noch keine Wirtschaftlichkeitsannahmen hinterlegt."
            });
        }

        return Ok(WirtschaftlichkeitsannahmenAntwort.Aus(annahmen));
    }

    /// <summary>
    /// Erstellt oder ersetzt die Wirtschaftlichkeitsannahmen für eine
    /// Modernisierungsalternative.
    /// </summary>
    [HttpPut("wirtschaftlichkeitsannahmen")]
    [ProducesResponseType(
        typeof(WirtschaftlichkeitsannahmenAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WirtschaftlichkeitsannahmenAntwort>>
        AnnahmenSpeichernAsync(
            Guid projektId,
            Guid alternativeId,
            [FromBody] WirtschaftlichkeitsannahmenAnfrage anfrage,
            CancellationToken cancellationToken)
    {
        var alternative =
            await _projektService.AlternativeNachIdAbrufenAsync(
                projektId,
                alternativeId,
                cancellationToken);

        if (alternative is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Die Alternative '{alternativeId}' wurde im Projekt '{projektId}' nicht gefunden."
            });
        }

        Wirtschaftlichkeitsannahmen annahmen;

        try
        {
            annahmen = new Wirtschaftlichkeitsannahmen(
                Guid.NewGuid(),
                anfrage.BetrachtungszeitraumJahre,
                anfrage.DiskontsatzProzent,
                anfrage.InflationsrateProzent,
                anfrage.Co2PreisProTonne,
                anfrage.JaehrlicherCo2PreisanstiegProzent,
                anfrage.WartungUndInstandhaltungProJahr,
                anfrage.NutzungsdauerJahre,
                anfrage.RestwertProzent);

            foreach (var eintrag in anfrage.Energietraeger)
            {
                annahmen.EnergietraegerHinzufuegen(
                    new EnergietraegerAnnahme(
                        Guid.NewGuid(),
                        eintrag.Energietraeger,
                        eintrag.PreisProKwh,
                        eintrag.JaehrlicherPreisanstiegProzent));
            }
        }
        catch (DomainException exception)
        {
            return BadRequest(new { Nachricht = exception.Message });
        }

        var gespeichert =
            await _repository.SpeichernAsync(
                alternativeId,
                annahmen,
                cancellationToken);

        return Ok(WirtschaftlichkeitsannahmenAntwort.Aus(gespeichert));
    }

    /// <summary>
    /// Berechnet die Wirtschaftlichkeit für eine Modernisierungsalternative
    /// auf Basis der hinterlegten Annahmen.
    /// </summary>
    [HttpPost("wirtschaftlichkeit/berechnen")]
    [ProducesResponseType(
        typeof(WirtschaftlichkeitsergebnisAntwort),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WirtschaftlichkeitsergebnisAntwort>>
        BerechnenAsync(
            Guid projektId,
            Guid alternativeId,
            [FromBody] WirtschaftlichkeitsBerechnungsAnfrage anfrage,
            CancellationToken cancellationToken)
    {
        var alternative =
            await _projektService.AlternativeNachIdAbrufenAsync(
                projektId,
                alternativeId,
                cancellationToken);

        if (alternative is null)
        {
            return NotFound(new
            {
                Nachricht =
                    $"Die Alternative '{alternativeId}' wurde im Projekt '{projektId}' nicht gefunden."
            });
        }

        var annahmen =
            await _repository.NachAlternativeIdAbrufenAsync(
                alternativeId,
                cancellationToken);

        if (annahmen is null)
        {
            return BadRequest(new
            {
                Nachricht =
                    "Für diese Modernisierungsalternative müssen zuerst Wirtschaftlichkeitsannahmen hinterlegt werden."
            });
        }

        var eingabe = new WirtschaftlichkeitsEingabe(
            anfrage.EinsparungProEnergiepfad
                .Select(
                    e =>
                        new EnergietraegerEinsparung(
                            e.Energietraeger,
                            e.JaehrlicheEinsparungKwh))
                .ToList(),
            anfrage.Basis);

        var ergebnis =
            _berechnungsService.Berechnen(
                alternative.Gesamtkosten,
                eingabe,
                annahmen);

        return Ok(WirtschaftlichkeitsergebnisAntwort.Aus(ergebnis));
    }
}

// ─── Request / Response DTOs ────────────────────────────────────────────────

public sealed record EnergietraegerAnnahmeAnfrage(
    Energietraeger Energietraeger,
    decimal PreisProKwh,
    decimal JaehrlicherPreisanstiegProzent);

public sealed record WirtschaftlichkeitsannahmenAnfrage(
    int BetrachtungszeitraumJahre,
    decimal DiskontsatzProzent,
    decimal InflationsrateProzent,
    decimal Co2PreisProTonne,
    decimal JaehrlicherCo2PreisanstiegProzent,
    decimal WartungUndInstandhaltungProJahr,
    int NutzungsdauerJahre,
    decimal RestwertProzent,
    IReadOnlyList<EnergietraegerAnnahmeAnfrage> Energietraeger);

public sealed record EnergietraegerAnnahmeAntwort(
    Energietraeger Energietraeger,
    decimal PreisProKwh,
    decimal JaehrlicherPreisanstiegProzent)
{
    public static EnergietraegerAnnahmeAntwort Aus(
        EnergietraegerAnnahme annahme)
    {
        return new EnergietraegerAnnahmeAntwort(
            annahme.Energietraeger,
            annahme.PreisProKwh,
            annahme.JaehrlicherPreisanstiegProzent);
    }
}

public sealed record WirtschaftlichkeitsannahmenAntwort(
    Guid Id,
    int BetrachtungszeitraumJahre,
    decimal DiskontsatzProzent,
    decimal InflationsrateProzent,
    decimal Co2PreisProTonne,
    decimal JaehrlicherCo2PreisanstiegProzent,
    decimal WartungUndInstandhaltungProJahr,
    int NutzungsdauerJahre,
    decimal RestwertProzent,
    IReadOnlyList<EnergietraegerAnnahmeAntwort> Energietraeger)
{
    public static WirtschaftlichkeitsannahmenAntwort Aus(
        Wirtschaftlichkeitsannahmen annahmen)
    {
        return new WirtschaftlichkeitsannahmenAntwort(
            annahmen.Id,
            annahmen.BetrachtungszeitraumJahre,
            annahmen.DiskontsatzProzent,
            annahmen.InflationsrateProzent,
            annahmen.Co2PreisProTonne,
            annahmen.JaehrlicherCo2PreisanstiegProzent,
            annahmen.WartungUndInstandhaltungProJahr,
            annahmen.NutzungsdauerJahre,
            annahmen.RestwertProzent,
            annahmen.Energietraeger
                .Select(EnergietraegerAnnahmeAntwort.Aus)
                .ToList());
    }
}

public sealed record EnergietraegerEinsparungAnfrage(
    Energietraeger Energietraeger,
    decimal JaehrlicheEinsparungKwh);

public sealed record WirtschaftlichkeitsBerechnungsAnfrage(
    IReadOnlyList<EnergietraegerEinsparungAnfrage> EinsparungProEnergiepfad,
    WirtschaftlichkeitsBasis Basis);

public sealed record WirtschaftlichkeitsergebnisAntwort(
    decimal Investition,
    decimal JaehrlicheEnergieeinsparungEur,
    decimal? StatischeAmortisationJahre,
    decimal Kapitalwert,
    decimal? KostenNutzenVerhaeltnis,
    decimal Restwert,
    WirtschaftlichkeitsBasis Basis,
    DateTimeOffset BerechnungszeitpunktUtc)
{
    public static WirtschaftlichkeitsergebnisAntwort Aus(
        Wirtschaftlichkeitsergebnis ergebnis)
    {
        return new WirtschaftlichkeitsergebnisAntwort(
            ergebnis.Investition,
            ergebnis.JaehrlicheEnergieeinsparungEur,
            ergebnis.StatischeAmortisationJahre,
            ergebnis.Kapitalwert,
            ergebnis.KostenNutzenVerhaeltnis,
            ergebnis.Restwert,
            ergebnis.Basis,
            ergebnis.BerechnungszeitpunktUtc);
    }
}
