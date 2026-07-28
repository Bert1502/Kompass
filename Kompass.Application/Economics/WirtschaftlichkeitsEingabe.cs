using Kompass.Domain.Economics;

namespace Kompass.Application.Economics;

/// <summary>
/// Eingabewerte für die Wirtschaftlichkeitsberechnung einer
/// Modernisierungsalternative.
/// </summary>
public sealed record WirtschaftlichkeitsEingabe(
    IReadOnlyList<EnergietraegerEinsparung> EinsparungProEnergiepfad,
    WirtschaftlichkeitsBasis Basis);
