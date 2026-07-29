using Kompass.Domain.Funding;

namespace Kompass.Domain.Reports;

/// <summary>
/// Förderinformationen für eine einzelne Modernisierungsalternative
/// im Rahmen der Förderübersicht.
/// </summary>
public sealed record FoerderuebersichtAlternative(
    Guid AlternativeId,
    int? B56Position,
    string Bezeichnung,
    decimal Gesamtkosten,
    IReadOnlyList<Foerderprogramm> ZugeordneteProgramme);
