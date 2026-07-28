namespace Kompass.Application.Projects;

/// <summary>
/// Kurzinformation zu einer Modernisierungsalternative für die Verwendung
/// außerhalb des Projektaggregats.
/// </summary>
public sealed record AlternativeKurzinfo(
    Guid Id,
    string Bezeichnung,
    Guid ProjektId,
    decimal Gesamtkosten);
