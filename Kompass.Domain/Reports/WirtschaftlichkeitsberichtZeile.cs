using Kompass.Domain.Economics;

namespace Kompass.Domain.Reports;

/// <summary>
/// Eine Zeile im Wirtschaftlichkeitsbericht: fasst Annahmen und Berechnungsergebnis
/// für eine Modernisierungsalternative zusammen.
/// </summary>
public sealed record WirtschaftlichkeitsberichtZeile(
    Guid AlternativeId,
    int? B56Position,
    string Bezeichnung,
    WirtschaftlichkeitsBasis Basis,
    decimal Investitionskosten,
    decimal Foerderung,
    int Betrachtungszeitraum,
    decimal Diskontsatz,
    decimal Inflationsrate,
    Wirtschaftlichkeitsergebnis Ergebnis);
