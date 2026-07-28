using Kompass.Domain.Common;

namespace Kompass.Domain.Economics;

public sealed class Wirtschaftlichkeitsannahmen : AggregateRoot
{
    private readonly List<EnergietraegerAnnahme> _energietraegerAnnahmen = new();

    private Wirtschaftlichkeitsannahmen()
    {
    }

    public Wirtschaftlichkeitsannahmen(
        Guid id,
        Guid modernisierungsalternativeId,
        WirtschaftlichkeitsBasis basis,
        int betrachtungszeitraum,
        decimal diskontsatz,
        decimal inflationsrate,
        decimal jaehrlicheWartungsmehrkosten,
        int nutzungsdauer,
        decimal foerderung)
        : base(id)
    {
        if (modernisierungsalternativeId == Guid.Empty)
        {
            throw new DomainException(
                "Die Modernisierungsalternative muss angegeben werden.");
        }

        ValidiereBetrachtungszeitraum(betrachtungszeitraum);
        ValidiereDiskontsatz(diskontsatz);
        ValidiereInflationsrate(inflationsrate);
        ValidiereNutzungsdauer(nutzungsdauer);
        ValidiereFoerderung(foerderung);

        ModernisierungsalternativeId = modernisierungsalternativeId;
        Basis = basis;
        Betrachtungszeitraum = betrachtungszeitraum;
        Diskontsatz = diskontsatz;
        Inflationsrate = inflationsrate;
        JaehrlicheWartungsmehrkosten = jaehrlicheWartungsmehrkosten;
        Nutzungsdauer = nutzungsdauer;
        Foerderung = foerderung;
    }

    public Guid ModernisierungsalternativeId { get; private set; }

    public WirtschaftlichkeitsBasis Basis { get; private set; }

    /// <summary>Betrachtungszeitraum in Jahren (1–50).</summary>
    public int Betrachtungszeitraum { get; private set; }

    /// <summary>Kalkulationszins als Dezimalzahl (z. B. 0,04 = 4 %).</summary>
    public decimal Diskontsatz { get; private set; }

    /// <summary>Allgemeine Inflationsrate als Dezimalzahl (z. B. 0,02 = 2 %).</summary>
    public decimal Inflationsrate { get; private set; }

    /// <summary>
    /// Jährliche Wartungs- und Instandhaltungsmehrkosten gegenüber dem Ist-Zustand in Euro/a.
    /// Negative Werte bedeuten Einsparungen bei Wartung.
    /// </summary>
    public decimal JaehrlicheWartungsmehrkosten { get; private set; }

    /// <summary>Technische Nutzungsdauer der Maßnahme in Jahren.</summary>
    public int Nutzungsdauer { get; private set; }

    /// <summary>Förderbetrag in Euro.</summary>
    public decimal Foerderung { get; private set; }

    public IReadOnlyCollection<EnergietraegerAnnahme> EnergietraegerAnnahmen =>
        _energietraegerAnnahmen.AsReadOnly();

    public void AnnahmenAktualisieren(
        int betrachtungszeitraum,
        decimal diskontsatz,
        decimal inflationsrate,
        decimal jaehrlicheWartungsmehrkosten,
        int nutzungsdauer,
        decimal foerderung)
    {
        ValidiereBetrachtungszeitraum(betrachtungszeitraum);
        ValidiereDiskontsatz(diskontsatz);
        ValidiereInflationsrate(inflationsrate);
        ValidiereNutzungsdauer(nutzungsdauer);
        ValidiereFoerderung(foerderung);

        Betrachtungszeitraum = betrachtungszeitraum;
        Diskontsatz = diskontsatz;
        Inflationsrate = inflationsrate;
        JaehrlicheWartungsmehrkosten = jaehrlicheWartungsmehrkosten;
        Nutzungsdauer = nutzungsdauer;
        Foerderung = foerderung;
    }

    public void EnergietraegerAnnahmeHinzufuegen(
        EnergietraegerAnnahme annahme)
    {
        ArgumentNullException.ThrowIfNull(annahme);

        if (_energietraegerAnnahmen.Any(
                vorhandene => vorhandene.Energietraeger == annahme.Energietraeger))
        {
            throw new DomainException(
                $"Für den Energieträger '{annahme.Energietraeger}' ist bereits eine Annahme vorhanden.");
        }

        _energietraegerAnnahmen.Add(annahme);
    }

    public void EnergietraegerAnnahmeEntfernen(
        Energietraeger energietraeger)
    {
        var annahme = _energietraegerAnnahmen
            .SingleOrDefault(
                vorhandene => vorhandene.Energietraeger == energietraeger);

        if (annahme is not null)
        {
            _energietraegerAnnahmen.Remove(annahme);
        }
    }

    /// <summary>
    /// Berechnet das Wirtschaftlichkeitsergebnis auf Basis der gespeicherten Annahmen.
    /// </summary>
    /// <param name="investitionskosten">
    /// Investitionskosten der Modernisierungsalternative in Euro.
    /// </param>
    public Wirtschaftlichkeitsergebnis Berechnen(
        decimal investitionskosten)
    {
        if (investitionskosten < 0)
        {
            throw new DomainException(
                "Die Investitionskosten dürfen nicht negativ sein.");
        }

        var eigenanteil = Math.Max(0m, investitionskosten - Foerderung);

        var einsparungJahr1 = BerechneJaehrlicheEinsparung(1);
        var gesamteinsparungJahr1 =
            einsparungJahr1 - JaehrlicheWartungsmehrkosten;

        var kumulierteEinsparung = 0m;
        var kapitalwert = -eigenanteil;
        decimal? amortisationsdauerDynamisch = null;
        var kumulierterKapitalwert = -eigenanteil;

        for (var t = 1; t <= Betrachtungszeitraum; t++)
        {
            var einsparungT = BerechneJaehrlicheEinsparung(t);
            var wartungsT =
                JaehrlicheWartungsmehrkosten *
                (decimal)Math.Pow(
                    (double)(1 + Inflationsrate),
                    t - 1);

            var nettoEinsparungT = einsparungT - wartungsT;

            kumulierteEinsparung += nettoEinsparungT;

            var diskontfaktor =
                (decimal)Math.Pow(
                    (double)(1 + Diskontsatz),
                    t);

            kapitalwert += nettoEinsparungT / diskontfaktor;

            if (amortisationsdauerDynamisch is null)
            {
                kumulierterKapitalwert += nettoEinsparungT / diskontfaktor;

                if (kumulierterKapitalwert >= 0)
                {
                    amortisationsdauerDynamisch = t;
                }
            }
        }

        decimal? amortisationsdauerStatisch = null;

        if (gesamteinsparungJahr1 > 0)
        {
            amortisationsdauerStatisch =
                eigenanteil / gesamteinsparungJahr1;
        }

        decimal? kostenNutzenVerhaeltnis = null;

        if (eigenanteil > 0)
        {
            kostenNutzenVerhaeltnis =
                kumulierteEinsparung / eigenanteil;
        }

        return new Wirtschaftlichkeitsergebnis(
            eigenanteil,
            gesamteinsparungJahr1,
            kumulierteEinsparung,
            amortisationsdauerStatisch,
            amortisationsdauerDynamisch,
            kapitalwert,
            kostenNutzenVerhaeltnis);
    }

    private decimal BerechneJaehrlicheEinsparung(
        int jahr)
    {
        var einsparung = 0m;

        foreach (var annahme in _energietraegerAnnahmen)
        {
            var energieEinsparungKwh = annahme.Einsparung;

            if (energieEinsparungKwh == 0m)
            {
                continue;
            }

            var preisT =
                annahme.Preis *
                (decimal)Math.Pow(
                    (double)(1 + annahme.Preissteigerungsrate),
                    jahr - 1);

            var co2PreisT =
                annahme.Co2Preis *
                (decimal)Math.Pow(
                    (double)(1 + annahme.Co2Preissteigerungsrate),
                    jahr - 1);

            var energiekostenEinsparung =
                energieEinsparungKwh * preisT;

            var co2KostenEinsparung =
                energieEinsparungKwh * annahme.Co2Faktor / 1000m * co2PreisT;

            einsparung += energiekostenEinsparung + co2KostenEinsparung;
        }

        return einsparung;
    }

    private static void ValidiereBetrachtungszeitraum(
        int betrachtungszeitraum)
    {
        if (betrachtungszeitraum is < 1 or > 50)
        {
            throw new DomainException(
                "Der Betrachtungszeitraum muss zwischen 1 und 50 Jahren liegen.");
        }
    }

    private static void ValidiereDiskontsatz(
        decimal diskontsatz)
    {
        if (diskontsatz < 0 || diskontsatz > 0.5m)
        {
            throw new DomainException(
                "Der Diskontsatz muss zwischen 0 und 0,5 (50 %) liegen.");
        }
    }

    private static void ValidiereInflationsrate(
        decimal inflationsrate)
    {
        if (inflationsrate < 0 || inflationsrate > 0.5m)
        {
            throw new DomainException(
                "Die Inflationsrate muss zwischen 0 und 0,5 (50 %) liegen.");
        }
    }

    private static void ValidiereNutzungsdauer(
        int nutzungsdauer)
    {
        if (nutzungsdauer is < 1 or > 100)
        {
            throw new DomainException(
                "Die Nutzungsdauer muss zwischen 1 und 100 Jahren liegen.");
        }
    }

    private static void ValidiereFoerderung(
        decimal foerderung)
    {
        if (foerderung < 0)
        {
            throw new DomainException(
                "Der Förderbetrag darf nicht negativ sein.");
        }
    }
}
