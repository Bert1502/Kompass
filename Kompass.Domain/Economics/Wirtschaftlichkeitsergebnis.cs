namespace Kompass.Domain.Economics;

/// <summary>
/// Ergebnis einer Wirtschaftlichkeitsberechnung für eine Modernisierungsalternative.
/// Unveränderlich nach Berechnung – bei geänderten Annahmen wird ein neues
/// Ergebnis erzeugt.
/// </summary>
public sealed record Wirtschaftlichkeitsergebnis
{
    /// <summary>Gesamtinvestition in EUR.</summary>
    public decimal Investition { get; init; }

    /// <summary>Jährliche Energiekosteneinsparung im ersten Jahr in EUR.</summary>
    public decimal JaehrlicheEnergieeinsparungEur { get; init; }

    /// <summary>Statische Amortisationszeit in Jahren.</summary>
    public decimal? StatischeAmortisationJahre { get; init; }

    /// <summary>Kapitalwert über den Betrachtungszeitraum in EUR.</summary>
    public decimal Kapitalwert { get; init; }

    /// <summary>Kosten-Nutzen-Verhältnis (Barwert Nutzen / Investition).</summary>
    public decimal? KostenNutzenVerhaeltnis { get; init; }

    /// <summary>Optionaler Restwert am Ende des Betrachtungszeitraums in EUR.</summary>
    public decimal Restwert { get; init; }

    /// <summary>
    /// Gibt an, ob das Ergebnis auf bilanzierten oder auf realen Verbrauchsdaten basiert.
    /// </summary>
    public WirtschaftlichkeitsBasis Basis { get; init; }

    /// <summary>Zeitpunkt der Berechnung (UTC).</summary>
    public DateTimeOffset BerechnungszeitpunktUtc { get; init; }
}

public enum WirtschaftlichkeitsBasis
{
    Bilanziert = 0,
    Praktisch = 1
}
