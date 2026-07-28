using Kompass.Domain.Economics;

namespace Kompass.Application.Economics;

/// <summary>
/// Berechnet den Kapitalwert, die statische Amortisation und das
/// Kosten-Nutzen-Verhältnis für eine Modernisierungsalternative.
///
/// Die Berechnung ist zustandslos und hat keine Abhängigkeiten zur
/// Persistenz.
/// </summary>
public sealed class WirtschaftlichkeitsberechnungsService
{
    /// <summary>
    /// Berechnet ein <see cref="Wirtschaftlichkeitsergebnis"/> auf Basis der
    /// übergebenen Annahmen und Eingabewerte.
    /// </summary>
    /// <param name="investition">
    /// Gesamtinvestition in EUR (Summe aller Kostenpositionen).
    /// </param>
    /// <param name="eingabe">
    /// Jährliche Energieeinsparungen je Energieträger und Berechnungsbasis.
    /// </param>
    /// <param name="annahmen">
    /// Wirtschaftliche Rahmendaten (Zinssatz, Nutzungsdauer, etc.).
    /// </param>
    /// <returns>Das berechnete Wirtschaftlichkeitsergebnis.</returns>
    public Wirtschaftlichkeitsergebnis Berechnen(
        decimal investition,
        WirtschaftlichkeitsEingabe eingabe,
        Wirtschaftlichkeitsannahmen annahmen)
    {
        ArgumentNullException.ThrowIfNull(eingabe);
        ArgumentNullException.ThrowIfNull(annahmen);

        var j1Einsparung =
            BerechneJaehrlicheEinsparungErstenJahr(
                eingabe,
                annahmen);

        var diskontsatz =
            annahmen.DiskontsatzProzent / 100m;

        var inflation =
            annahmen.InflationsrateProzent / 100m;

        var kapitalwert =
            BerechneKapitalwert(
                investition,
                eingabe,
                annahmen,
                diskontsatz,
                inflation);

        var restwertNominal =
            investition * annahmen.RestwertProzent / 100m;

        var nettoeinsparungJ1 =
            j1Einsparung - annahmen.WartungUndInstandhaltungProJahr;

        var statischeAmortisationJahre =
            nettoeinsparungJ1 > 0
                ? (decimal?)Math.Round(
                    (double)(investition / nettoeinsparungJ1),
                    2)
                : null;

        var kostenNutzenVerhaeltnis =
            investition > 0
                ? (decimal?)Math.Round(
                    (double)((kapitalwert + investition) / investition),
                    4)
                : null;

        return new Wirtschaftlichkeitsergebnis
        {
            Investition = investition,
            JaehrlicheEnergieeinsparungEur = j1Einsparung,
            StatischeAmortisationJahre = statischeAmortisationJahre,
            Kapitalwert = kapitalwert,
            KostenNutzenVerhaeltnis = kostenNutzenVerhaeltnis,
            Restwert = restwertNominal,
            Basis = eingabe.Basis,
            BerechnungszeitpunktUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Jährliche Energiekosteneinsparung im ersten Betriebsjahr in EUR.
    /// </summary>
    private static decimal BerechneJaehrlicheEinsparungErstenJahr(
        WirtschaftlichkeitsEingabe eingabe,
        Wirtschaftlichkeitsannahmen annahmen)
    {
        return eingabe.EinsparungProEnergiepfad
            .Sum(einsparung =>
            {
                var annahme =
                    annahmen.Energietraeger.FirstOrDefault(
                        a => a.Energietraeger == einsparung.Energietraeger);

                return annahme is null
                    ? 0m
                    : einsparung.JaehrlicheEinsparungKwh * annahme.PreisProKwh;
            });
    }

    /// <summary>
    /// Kapitalwert (Net Present Value) über den Betrachtungszeitraum in EUR.
    /// Formel:
    ///   NPV = –Investition
    ///         + Σ(t=1..N) [ CashFlow(t) / (1+d)^t ]
    ///         + Restwert_nominal / (1+d)^N
    /// mit
    ///   CashFlow(t) = Σ(Energieträger: kWh × Preis × (1+p)^(t–1))
    ///                 – Wartung × (1+inf)^(t–1)
    /// </summary>
    private static decimal BerechneKapitalwert(
        decimal investition,
        WirtschaftlichkeitsEingabe eingabe,
        Wirtschaftlichkeitsannahmen annahmen,
        decimal diskontsatz,
        decimal inflation)
    {
        var npv = -investition;
        var n = annahmen.BetrachtungszeitraumJahre;

        for (var t = 1; t <= n; t++)
        {
            var cashFlow =
                BerechneJaehrlicherCashFlow(
                    t,
                    eingabe,
                    annahmen,
                    inflation);

            var diskontFaktor =
                (decimal)Math.Pow(
                    (double)(1m + diskontsatz),
                    t);

            npv += cashFlow / diskontFaktor;
        }

        // Barwert des Restwerts
        var restwertNominal =
            investition * annahmen.RestwertProzent / 100m;

        if (restwertNominal != 0m)
        {
            var endDiskontFaktor =
                (decimal)Math.Pow(
                    (double)(1m + diskontsatz),
                    n);

            npv += restwertNominal / endDiskontFaktor;
        }

        return Math.Round(npv, 2);
    }

    /// <summary>
    /// Cash-Flow im Jahr t: Summe der Energieeinsparungen (preisbereinigt)
    /// abzüglich inflationsbereinigter Wartungskosten.
    /// </summary>
    private static decimal BerechneJaehrlicherCashFlow(
        int t,
        WirtschaftlichkeitsEingabe eingabe,
        Wirtschaftlichkeitsannahmen annahmen,
        decimal inflation)
    {
        var energieeinsparung = eingabe.EinsparungProEnergiepfad
            .Sum(einsparung =>
            {
                var annahme =
                    annahmen.Energietraeger.FirstOrDefault(
                        a => a.Energietraeger == einsparung.Energietraeger);

                if (annahme is null)
                {
                    return 0m;
                }

                var preissteigerung =
                    annahme.JaehrlicherPreisanstiegProzent / 100m;

                var preisFaktor =
                    (decimal)Math.Pow(
                        (double)(1m + preissteigerung),
                        t - 1);

                return einsparung.JaehrlicheEinsparungKwh
                       * annahme.PreisProKwh
                       * preisFaktor;
            });

        var inflationsFaktor =
            (decimal)Math.Pow(
                (double)(1m + inflation),
                t - 1);

        var wartung =
            annahmen.WartungUndInstandhaltungProJahr * inflationsFaktor;

        return energieeinsparung - wartung;
    }
}
