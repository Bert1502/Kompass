namespace Kompass.Api.Economics;

public sealed class WirtschaftlichkeitsannahmenSetzenAnfrage
{
    /// <summary>Betrachtungszeitraum in Jahren (1–50).</summary>
    public int Betrachtungszeitraum { get; init; }

    /// <summary>Kalkulationszins als Dezimalzahl (z. B. 0,04 = 4 %).</summary>
    public decimal Diskontsatz { get; init; }

    /// <summary>Allgemeine Inflationsrate als Dezimalzahl (z. B. 0,02 = 2 %).</summary>
    public decimal Inflationsrate { get; init; }

    /// <summary>
    /// Jährliche Wartungs- und Instandhaltungsmehrkosten gegenüber dem Ist-Zustand in Euro/a.
    /// </summary>
    public decimal JaehrlicheWartungsmehrkosten { get; init; }

    /// <summary>Technische Nutzungsdauer der Maßnahme in Jahren.</summary>
    public int Nutzungsdauer { get; init; }

    /// <summary>Förderbetrag in Euro.</summary>
    public decimal Foerderung { get; init; }

    public IList<EnergietraegerAnnahmeAnfrage> EnergietraegerAnnahmen { get; init; } =
        new List<EnergietraegerAnnahmeAnfrage>();
}
