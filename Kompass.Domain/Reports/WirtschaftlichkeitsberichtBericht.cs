namespace Kompass.Domain.Reports;

/// <summary>
/// Bericht "Wirtschaftlichkeitsbericht" gemäß Fachspezifikation Abschnitt 17.
/// Listet alle Modernisierungsalternativen mit ihren Wirtschaftlichkeitsannahmen
/// und -ergebnissen auf. Alternativen ohne hinterlegte Annahmen werden ausgelassen.
/// </summary>
public sealed record WirtschaftlichkeitsberichtBericht(
    Berichtskopf Kopf,
    IReadOnlyList<WirtschaftlichkeitsberichtZeile> Alternativen);
