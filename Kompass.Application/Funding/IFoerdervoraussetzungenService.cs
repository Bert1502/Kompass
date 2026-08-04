using Kompass.Domain.Funding;

namespace Kompass.Application.Funding;

public interface IFoerdervoraussetzungenService
{
    Task<Foerdervoraussetzungen?> AbrufenAsync(Guid projektId, CancellationToken cancellationToken = default);
    Task<Foerdervoraussetzungen?> SpeichernAsync(Guid projektId, FoerdervoraussetzungenEingabe eingabe, CancellationToken cancellationToken = default);
}

public sealed record FoerdervoraussetzungenEingabe(
    int? Baujahr, DateOnly? Erstnutzung, FoerderGebaeudeart? Gebaeudeart, FoerderNutzung? Nutzung,
    int? Wohneinheiten, Antragstellerart? Eigentuemart, bool? Selbstnutzung, bool? Vermietung,
    bool? Denkmal, bool? BesondersErhaltenswerteBausubstanz, bool? Gemeinnuetzigkeit,
    bool? WirtschaftlicheTaetigkeit, bool? Vorsteuerabzug, bool? ISfp, bool? Energieausweis,
    string? Nachweise, decimal? QpReferenz, string? QpReferenzQuelle, bool? WpbFachlichBestaetigt);
