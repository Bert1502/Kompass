using Kompass.Application.Economics;
using Kompass.Domain.Economics;

namespace Kompass.Tests.Application;

public sealed class WirtschaftlichkeitsberechnungsServiceTests
{
    private readonly WirtschaftlichkeitsberechnungsService _service = new();

    // ─── Hilfsmethoden ──────────────────────────────────────────────────────

    private static Wirtschaftlichkeitsannahmen ErstelleAnnahmen(
        int betrachtungszeitraum = 20,
        decimal diskontsatz = 3.0m,
        decimal inflation = 2.0m,
        decimal wartung = 0m,
        decimal restwert = 0m)
    {
        return new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            betrachtungszeitraumJahre: betrachtungszeitraum,
            diskontsatzProzent: diskontsatz,
            inflationsrateProzent: inflation,
            co2PreisProTonne: 0m,
            jaehrlicherCo2PreisanstiegProzent: 0m,
            wartungUndInstandhaltungProJahr: wartung,
            nutzungsdauerJahre: betrachtungszeitraum,
            restwertProzent: restwert);
    }

    private static Wirtschaftlichkeitsannahmen ErstelleAnnahmenMitEnergietraeger(
        Energietraeger energietraeger,
        decimal preisProKwh,
        decimal preissteigerung = 0m,
        int betrachtungszeitraum = 20,
        decimal diskontsatz = 0m,
        decimal inflation = 0m,
        decimal wartung = 0m,
        decimal restwert = 0m)
    {
        var annahmen = ErstelleAnnahmen(
            betrachtungszeitraum,
            diskontsatz,
            inflation,
            wartung,
            restwert);

        annahmen.EnergietraegerHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                energietraeger,
                preisProKwh,
                preissteigerung));

        return annahmen;
    }

    private static WirtschaftlichkeitsEingabe ErstelleEingabe(
        Energietraeger energietraeger,
        decimal einsparungKwh,
        WirtschaftlichkeitsBasis basis = WirtschaftlichkeitsBasis.Bilanziert)
    {
        return new WirtschaftlichkeitsEingabe(
            new List<EnergietraegerEinsparung>
            {
                new(energietraeger, einsparungKwh)
            },
            basis);
    }

    // ─── Grundrechenweg ─────────────────────────────────────────────────────

    [Fact]
    public void Ohne_Investition_und_Einsparung_ist_Kapitalwert_null()
    {
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.12m);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 0m);

        var ergebnis = _service.Berechnen(
            investition: 0m,
            eingabe,
            annahmen);

        Assert.Equal(0m, ergebnis.Kapitalwert);
        Assert.Equal(0m, ergebnis.Investition);
        Assert.Equal(0m, ergebnis.JaehrlicheEnergieeinsparungEur);
        Assert.Null(ergebnis.StatischeAmortisationJahre);
        Assert.Null(ergebnis.KostenNutzenVerhaeltnis);
    }

    [Fact]
    public void Jaehrliche_Einsparung_wird_korrekt_berechnet()
    {
        // 1000 kWh × 0.30 EUR/kWh = 300 EUR
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Strom,
            preisProKwh: 0.30m);

        var eingabe = ErstelleEingabe(
            Energietraeger.Strom,
            einsparungKwh: 1000m);

        var ergebnis = _service.Berechnen(
            investition: 5000m,
            eingabe,
            annahmen);

        Assert.Equal(300m, ergebnis.JaehrlicheEnergieeinsparungEur);
    }

    [Fact]
    public void Statische_Amortisation_wird_korrekt_berechnet()
    {
        // 10 000 EUR Investition / 1000 EUR Einsparung = 10 Jahre
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 10_000m);

        var ergebnis = _service.Berechnen(
            investition: 10_000m,
            eingabe,
            annahmen);

        Assert.Equal(10m, ergebnis.StatischeAmortisationJahre);
    }

    [Fact]
    public void Statische_Amortisation_ist_null_wenn_Einsparung_kleiner_Wartung()
    {
        // Wartung übersteigt Einsparung → kein positiver Nettocashflow
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.01m,
            wartung: 1000m);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 100m);  // 100 × 0.01 = 1 EUR < 1000 EUR Wartung

        var ergebnis = _service.Berechnen(
            investition: 10_000m,
            eingabe,
            annahmen);

        Assert.Null(ergebnis.StatischeAmortisationJahre);
    }

    // ─── Kapitalwert (NPV) ──────────────────────────────────────────────────

    [Fact]
    public void Kapitalwert_mit_Diskontsatz_null_entspricht_undiskontierter_Summe()
    {
        // Diskontsatz = 0 % → Kapitalwert = –Investition + N × Jahreseinsparung
        // 5000 kWh × 0.10 EUR/kWh × 20 Jahre = 10 000 EUR; –5000 + 10 000 = 5 000 EUR
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m,
            diskontsatz: 0m,
            betrachtungszeitraum: 20);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 5000m);

        var ergebnis = _service.Berechnen(
            investition: 5000m,
            eingabe,
            annahmen);

        Assert.Equal(5000m, ergebnis.Kapitalwert);
    }

    [Fact]
    public void Kapitalwert_ist_negativ_bei_unrentabler_Massnahme()
    {
        // 1000 kWh × 0.01 EUR/kWh × 5 Jahre = 50 EUR – Investition 10 000 EUR
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.01m,
            diskontsatz: 0m,
            betrachtungszeitraum: 5);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 1000m);

        var ergebnis = _service.Berechnen(
            investition: 10_000m,
            eingabe,
            annahmen);

        Assert.True(ergebnis.Kapitalwert < 0m);
    }

    [Fact]
    public void Restwert_wird_korrekt_berechnet()
    {
        // RestwertProzent = 20 %, Investition = 10 000 EUR → Restwert = 2 000 EUR
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Strom,
            preisProKwh: 0.30m,
            restwert: 20m);

        var eingabe = ErstelleEingabe(
            Energietraeger.Strom,
            einsparungKwh: 1000m);

        var ergebnis = _service.Berechnen(
            investition: 10_000m,
            eingabe,
            annahmen);

        Assert.Equal(2000m, ergebnis.Restwert);
    }

    [Fact]
    public void Restwert_verbessert_Kapitalwert_bei_Diskontsatz_null()
    {
        // Ohne Diskontsatz wird der Nominalrestwert direkt addiert
        var annahmenOhneRestwert = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m,
            diskontsatz: 0m,
            betrachtungszeitraum: 20,
            restwert: 0m);

        var annahmenMitRestwert = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m,
            diskontsatz: 0m,
            betrachtungszeitraum: 20,
            restwert: 10m);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 5000m);

        var ohne = _service.Berechnen(10_000m, eingabe, annahmenOhneRestwert);
        var mit = _service.Berechnen(10_000m, eingabe, annahmenMitRestwert);

        Assert.True(mit.Kapitalwert > ohne.Kapitalwert);
        Assert.Equal(1000m, mit.Restwert);  // 10 % von 10 000
    }

    // ─── Kosten-Nutzen-Verhältnis ────────────────────────────────────────────

    [Fact]
    public void KNV_groesser_1_bei_positiver_Wirtschaftlichkeit()
    {
        // Hohe Einsparung, geringe Investition → KNV > 1
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Strom,
            preisProKwh: 0.30m,
            diskontsatz: 0m,
            betrachtungszeitraum: 20);

        var eingabe = ErstelleEingabe(
            Energietraeger.Strom,
            einsparungKwh: 5000m);   // 1500 EUR/Jahr × 20 = 30 000 EUR

        var ergebnis = _service.Berechnen(
            investition: 5000m,
            eingabe,
            annahmen);

        Assert.True(ergebnis.KostenNutzenVerhaeltnis > 1m);
    }

    [Fact]
    public void KNV_kleiner_1_bei_negativer_Wirtschaftlichkeit()
    {
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.05m,
            diskontsatz: 0m,
            betrachtungszeitraum: 5);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 100m);   // 5 EUR/Jahr × 5 = 25 EUR << Investition

        var ergebnis = _service.Berechnen(
            investition: 10_000m,
            eingabe,
            annahmen);

        Assert.True(ergebnis.KostenNutzenVerhaeltnis < 1m);
    }

    // ─── Preissteigerung ────────────────────────────────────────────────────

    [Fact]
    public void Preissteigerung_erhoeht_Kapitalwert_gegenueber_ohne_Steigerung()
    {
        var annahmenOhneSteigerung = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m,
            preissteigerung: 0m,
            diskontsatz: 0m,
            betrachtungszeitraum: 10);

        var annahmenMitSteigerung = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m,
            preissteigerung: 5m,
            diskontsatz: 0m,
            betrachtungszeitraum: 10);

        var eingabe = ErstelleEingabe(
            Energietraeger.Erdgas,
            einsparungKwh: 1000m);

        var ohne = _service.Berechnen(5000m, eingabe, annahmenOhneSteigerung);
        var mit = _service.Berechnen(5000m, eingabe, annahmenMitSteigerung);

        Assert.True(mit.Kapitalwert > ohne.Kapitalwert);
    }

    // ─── Mehrere Energieträger ───────────────────────────────────────────────

    [Fact]
    public void Mehrere_Energietraeger_werden_korrekt_summiert()
    {
        var annahmen = ErstelleAnnahmen(
            diskontsatz: 0m,
            betrachtungszeitraum: 1);

        annahmen.EnergietraegerHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Erdgas,
                preisProKwh: 0.10m,
                jaehrlicherPreisanstiegProzent: 0m));

        annahmen.EnergietraegerHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Strom,
                preisProKwh: 0.30m,
                jaehrlicherPreisanstiegProzent: 0m));

        var eingabe = new WirtschaftlichkeitsEingabe(
            new List<EnergietraegerEinsparung>
            {
                new(Energietraeger.Erdgas, 1000m),  // 100 EUR
                new(Energietraeger.Strom, 500m)     // 150 EUR
            },
            WirtschaftlichkeitsBasis.Bilanziert);

        var ergebnis = _service.Berechnen(
            investition: 0m,
            eingabe,
            annahmen);

        // 100 + 150 = 250 EUR im ersten Jahr
        Assert.Equal(250m, ergebnis.JaehrlicheEnergieeinsparungEur);
    }

    [Fact]
    public void Nicht_hinterlegter_Energietraeger_traegt_null_bei()
    {
        // Annahmen haben nur Erdgas – Strom wird in der Eingabe übergeben
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m,
            diskontsatz: 0m,
            betrachtungszeitraum: 1);

        var eingabe = new WirtschaftlichkeitsEingabe(
            new List<EnergietraegerEinsparung>
            {
                new(Energietraeger.Strom, 10_000m)  // kein Preis hinterlegt
            },
            WirtschaftlichkeitsBasis.Bilanziert);

        var ergebnis = _service.Berechnen(
            investition: 0m,
            eingabe,
            annahmen);

        Assert.Equal(0m, ergebnis.JaehrlicheEnergieeinsparungEur);
    }

    // ─── Berechnungsbasis ────────────────────────────────────────────────────

    [Fact]
    public void Basis_wird_korrekt_weitergegeben()
    {
        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m);

        var bilanziert = _service.Berechnen(
            500m,
            new WirtschaftlichkeitsEingabe(
                new[] { new EnergietraegerEinsparung(Energietraeger.Erdgas, 100m) },
                WirtschaftlichkeitsBasis.Bilanziert),
            annahmen);

        var praktisch = _service.Berechnen(
            500m,
            new WirtschaftlichkeitsEingabe(
                new[] { new EnergietraegerEinsparung(Energietraeger.Erdgas, 100m) },
                WirtschaftlichkeitsBasis.Praktisch),
            annahmen);

        Assert.Equal(WirtschaftlichkeitsBasis.Bilanziert, bilanziert.Basis);
        Assert.Equal(WirtschaftlichkeitsBasis.Praktisch, praktisch.Basis);
    }

    [Fact]
    public void BerechnungszeitpunktUtc_liegt_in_der_Vergangenheit_oder_Gegenwart()
    {
        var vorBerechnung = DateTimeOffset.UtcNow;

        var annahmen = ErstelleAnnahmenMitEnergietraeger(
            Energietraeger.Erdgas,
            preisProKwh: 0.10m);

        var ergebnis = _service.Berechnen(
            1000m,
            ErstelleEingabe(Energietraeger.Erdgas, 500m),
            annahmen);

        Assert.True(ergebnis.BerechnungszeitpunktUtc >= vorBerechnung);
    }
}
