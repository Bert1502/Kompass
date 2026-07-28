namespace Kompass.Domain.Economics;

/// <summary>
/// Ergebnis der Wirtschaftlichkeitsberechnung für eine Modernisierungsalternative.
/// Alle Geldbeträge in Euro, alle Zeiträume in Jahren.
/// </summary>
public sealed record Wirtschaftlichkeitsergebnis(
    decimal Eigenanteil,
    decimal JaehrlicheEnergiekosteneinsparungJahr1,
    decimal KumulierteEnergiekosteneinsparung,
    decimal? AmortisationsdauerStatisch,
    decimal? AmortisationsdauerDynamisch,
    decimal Kapitalwert,
    decimal? KostenNutzenVerhaeltnis);
