using Kompass.Domain.Common;

namespace Kompass.Domain.Economics;

/// <summary>
/// Rahmenannahmen für eine Wirtschaftlichkeitsberechnung einer
/// Modernisierungsalternative.
/// </summary>
public sealed class Wirtschaftlichkeitsannahmen : Entity
{
    private readonly List<EnergietraegerAnnahme> _energietraeger = new();

    private Wirtschaftlichkeitsannahmen()
    {
    }

    public Wirtschaftlichkeitsannahmen(
        Guid id,
        int betrachtungszeitraumJahre,
        decimal diskontsatzProzent,
        decimal inflationsrateProzent,
        decimal co2PreisProTonne,
        decimal jaehrlicherCo2PreisanstiegProzent,
        decimal wartungUndInstandhaltungProJahr,
        int nutzungsdauerJahre,
        decimal restwertProzent)
        : base(id)
    {
        Validiere(
            betrachtungszeitraumJahre,
            diskontsatzProzent,
            inflationsrateProzent,
            co2PreisProTonne,
            jaehrlicherCo2PreisanstiegProzent,
            wartungUndInstandhaltungProJahr,
            nutzungsdauerJahre,
            restwertProzent);

        BetrachtungszeitraumJahre = betrachtungszeitraumJahre;
        DiskontsatzProzent = diskontsatzProzent;
        InflationsrateProzent = inflationsrateProzent;
        Co2PreisProTonne = co2PreisProTonne;
        JaehrlicherCo2PreisanstiegProzent = jaehrlicherCo2PreisanstiegProzent;
        WartungUndInstandhaltungProJahr = wartungUndInstandhaltungProJahr;
        NutzungsdauerJahre = nutzungsdauerJahre;
        RestwertProzent = restwertProzent;
    }

    /// <summary>Betrachtungszeitraum in Jahren (≥ 1).</summary>
    public int BetrachtungszeitraumJahre { get; private set; }

    /// <summary>Kalkulationszinssatz in Prozent.</summary>
    public decimal DiskontsatzProzent { get; private set; }

    /// <summary>Allgemeine Inflationsrate in Prozent.</summary>
    public decimal InflationsrateProzent { get; private set; }

    /// <summary>CO₂-Preis in EUR/t.</summary>
    public decimal Co2PreisProTonne { get; private set; }

    /// <summary>Jährliche CO₂-Preissteigerung in Prozent.</summary>
    public decimal JaehrlicherCo2PreisanstiegProzent { get; private set; }

    /// <summary>Jährliche Wartungs- und Instandhaltungskosten in EUR.</summary>
    public decimal WartungUndInstandhaltungProJahr { get; private set; }

    /// <summary>Technische Nutzungsdauer der Maßnahme in Jahren (≥ 1).</summary>
    public int NutzungsdauerJahre { get; private set; }

    /// <summary>
    /// Restwert der Maßnahme am Ende des Betrachtungszeitraums
    /// als prozentualer Anteil der Investition (0–100).
    /// </summary>
    public decimal RestwertProzent { get; private set; }

    public IReadOnlyCollection<EnergietraegerAnnahme> Energietraeger =>
        _energietraeger.AsReadOnly();

    public void EnergietraegerHinzufuegen(
        EnergietraegerAnnahme annahme)
    {
        ArgumentNullException.ThrowIfNull(annahme);

        if (_energietraeger.Any(
                e => e.Energietraeger == annahme.Energietraeger))
        {
            throw new DomainException(
                $"Für den Energieträger '{annahme.Energietraeger}' ist bereits eine Annahme vorhanden.");
        }

        _energietraeger.Add(annahme);
    }

    public void AnnahmenAendern(
        int betrachtungszeitraumJahre,
        decimal diskontsatzProzent,
        decimal inflationsrateProzent,
        decimal co2PreisProTonne,
        decimal jaehrlicherCo2PreisanstiegProzent,
        decimal wartungUndInstandhaltungProJahr,
        int nutzungsdauerJahre,
        decimal restwertProzent)
    {
        Validiere(
            betrachtungszeitraumJahre,
            diskontsatzProzent,
            inflationsrateProzent,
            co2PreisProTonne,
            jaehrlicherCo2PreisanstiegProzent,
            wartungUndInstandhaltungProJahr,
            nutzungsdauerJahre,
            restwertProzent);

        BetrachtungszeitraumJahre = betrachtungszeitraumJahre;
        DiskontsatzProzent = diskontsatzProzent;
        InflationsrateProzent = inflationsrateProzent;
        Co2PreisProTonne = co2PreisProTonne;
        JaehrlicherCo2PreisanstiegProzent = jaehrlicherCo2PreisanstiegProzent;
        WartungUndInstandhaltungProJahr = wartungUndInstandhaltungProJahr;
        NutzungsdauerJahre = nutzungsdauerJahre;
        RestwertProzent = restwertProzent;
    }

    private static void Validiere(
        int betrachtungszeitraumJahre,
        decimal diskontsatzProzent,
        decimal inflationsrateProzent,
        decimal co2PreisProTonne,
        decimal jaehrlicherCo2PreisanstiegProzent,
        decimal wartungUndInstandhaltungProJahr,
        int nutzungsdauerJahre,
        decimal restwertProzent)
    {
        if (betrachtungszeitraumJahre < 1)
        {
            throw new DomainException(
                "Der Betrachtungszeitraum muss mindestens 1 Jahr betragen.");
        }

        if (diskontsatzProzent < 0)
        {
            throw new DomainException(
                "Der Diskontsatz darf nicht negativ sein.");
        }

        if (co2PreisProTonne < 0)
        {
            throw new DomainException(
                "Der CO₂-Preis darf nicht negativ sein.");
        }

        if (wartungUndInstandhaltungProJahr < 0)
        {
            throw new DomainException(
                "Die Wartungs- und Instandhaltungskosten dürfen nicht negativ sein.");
        }

        if (nutzungsdauerJahre < 1)
        {
            throw new DomainException(
                "Die Nutzungsdauer muss mindestens 1 Jahr betragen.");
        }

        if (restwertProzent is < 0 or > 100)
        {
            throw new DomainException(
                "Der Restwert muss zwischen 0 und 100 % liegen.");
        }
    }
}
