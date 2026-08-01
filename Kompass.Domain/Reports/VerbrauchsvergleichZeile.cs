using Kompass.Domain.Economics;

namespace Kompass.Domain.Reports;

/// <summary>
/// Eine Zeile im Verbrauchsvergleichsbericht: realer Verbrauch einer Abrechnungsperiode
/// gegenüber dem B56-Vergleichswert.
/// </summary>
public sealed record VerbrauchsvergleichZeile(
    Guid VerbrauchsDatenId,
    DateOnly PeriodeVon,
    DateOnly PeriodeBis,
    Energietraeger Energietraeger,
    decimal Menge,
    decimal WitterungsbereinigteMenge,
    decimal? B56VergleichsWert,
    decimal? Abweichung,
    decimal? AbweichungProzent);
