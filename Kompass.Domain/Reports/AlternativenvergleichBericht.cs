namespace Kompass.Domain.Reports;

/// <summary>
/// Bericht "Vergleich von Modernisierungsalternativen" gemäß
/// Fachspezifikation Abschnitt 17.
/// Listet alle Alternativen des Projekts mit ihren Gesamtkosten auf.
/// </summary>
public sealed record AlternativenvergleichBericht(
    Berichtskopf Kopf,
    IReadOnlyList<AlternativenvergleichZeile> Alternativen);
