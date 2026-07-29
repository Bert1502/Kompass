namespace Kompass.Domain.Reports;

/// <summary>
/// Bericht "Förderübersicht" gemäß Fachspezifikation Abschnitt 17.
/// Listet alle Modernisierungsalternativen des Projekts mit ihren
/// zugeordneten Förderprogrammen auf.
/// </summary>
public sealed record FoerderuebersichtBericht(
    Berichtskopf Kopf,
    IReadOnlyList<FoerderuebersichtAlternative> Alternativen);
