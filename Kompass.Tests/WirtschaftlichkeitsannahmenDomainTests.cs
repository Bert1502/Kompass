using Kompass.Domain.Common;
using Kompass.Domain.Economics;

namespace Kompass.Tests.Domain;

public sealed class WirtschaftlichkeitsannahmenDomainTests
{
    private static readonly Guid AlternativeId = Guid.NewGuid();

    [Fact]
    public void Konstruktor_erstellt_Annahmen_mit_gueltigen_Werten()
    {
        var annahmen = ErstelleStandardAnnahmen();

        Assert.Equal(AlternativeId, annahmen.ModernisierungsalternativeId);
        Assert.Equal(WirtschaftlichkeitsBasis.Bilanziert, annahmen.Basis);
        Assert.Equal(20, annahmen.Betrachtungszeitraum);
    }

    [Fact]
    public void Konstruktor_lehnt_leere_AlternativeId_ab()
    {
        Assert.Throws<DomainException>(
            () => new Wirtschaftlichkeitsannahmen(
                Guid.NewGuid(),
                Guid.Empty,
                WirtschaftlichkeitsBasis.Bilanziert,
                20, 0.04m, 0.02m, 0m, 20, 0m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Konstruktor_lehnt_ungueltigen_Betrachtungszeitraum_ab(
        int betrachtungszeitraum)
    {
        Assert.Throws<DomainException>(
            () => new Wirtschaftlichkeitsannahmen(
                Guid.NewGuid(),
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                betrachtungszeitraum, 0.04m, 0.02m, 0m, 20, 0m));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(0.51)]
    public void Konstruktor_lehnt_ungueltigen_Diskontsatz_ab(
        double diskontsatz)
    {
        Assert.Throws<DomainException>(
            () => new Wirtschaftlichkeitsannahmen(
                Guid.NewGuid(),
                AlternativeId,
                WirtschaftlichkeitsBasis.Bilanziert,
                20, (decimal)diskontsatz, 0.02m, 0m, 20, 0m));
    }

    [Fact]
    public void EnergietraegerAnnahmeHinzufuegen_fuegt_Annahme_hinzu()
    {
        var annahmen = ErstelleStandardAnnahmen();

        var traegerAnnahme = new EnergietraegerAnnahme(
            Guid.NewGuid(),
            Energietraeger.Gas,
            0.08m, 0.03m, 0.2m, 50m, 0.05m,
            20000m, 10000m);

        annahmen.EnergietraegerAnnahmeHinzufuegen(traegerAnnahme);

        Assert.Single(annahmen.EnergietraegerAnnahmen);
    }

    [Fact]
    public void EnergietraegerAnnahmeHinzufuegen_lehnt_doppelten_Energietraeger_ab()
    {
        var annahmen = ErstelleStandardAnnahmen();

        annahmen.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.08m, 0.03m, 0.2m, 50m, 0.05m,
                20000m, 10000m));

        Assert.Throws<DomainException>(
            () => annahmen.EnergietraegerAnnahmeHinzufuegen(
                new EnergietraegerAnnahme(
                    Guid.NewGuid(),
                    Energietraeger.Gas,
                    0.09m, 0.02m, 0.2m, 50m, 0.05m,
                    15000m, 8000m)));
    }

    [Fact]
    public void Berechnen_gibt_korrekte_statische_Amortisationsdauer_zurueck()
    {
        var annahmen = ErstelleStandardAnnahmen();

        annahmen.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                20000m, 10000m));

        // Investition 10.000 €, Einsparung Jahr 1: 10.000 kWh × 0,10 €/kWh = 1.000 €/a
        // AmortStat = 10.000 / 1.000 = 10 Jahre
        var ergebnis = annahmen.Berechnen(10_000m);

        Assert.NotNull(ergebnis.AmortisationsdauerStatisch);
        Assert.Equal(10m, ergebnis.AmortisationsdauerStatisch!.Value);
        Assert.Equal(10_000m, ergebnis.Eigenanteil);
        Assert.Equal(1_000m, ergebnis.JaehrlicheEnergiekosteneinsparungJahr1);
    }

    [Fact]
    public void Berechnen_beruecksichtigt_Foerderung_im_Eigenanteil()
    {
        var annahmen = ErstelleStandardAnnahmen();

        annahmen.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                20000m, 10000m));

        // Investition 10.000 €, Förderung 4.000 € → Eigenanteil 6.000 €
        // AmortStat = 6.000 / 1.000 = 6 Jahre
        var annahmenMitFoerderung = new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            AlternativeId,
            WirtschaftlichkeitsBasis.Bilanziert,
            20, 0.04m, 0.02m, 0m, 20, 4_000m);

        annahmenMitFoerderung.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                20000m, 10000m));

        var ergebnis = annahmenMitFoerderung.Berechnen(10_000m);

        Assert.Equal(6_000m, ergebnis.Eigenanteil);
        Assert.Equal(6m, ergebnis.AmortisationsdauerStatisch!.Value);
    }

    [Fact]
    public void Berechnen_ohne_Energietraeger_liefert_keine_Amortisationsdauer()
    {
        var annahmen = ErstelleStandardAnnahmen();

        var ergebnis = annahmen.Berechnen(10_000m);

        Assert.Null(ergebnis.AmortisationsdauerStatisch);
        Assert.Equal(0m, ergebnis.JaehrlicheEnergiekosteneinsparungJahr1);
    }

    [Fact]
    public void Berechnen_mit_positiver_Einsparung_liefert_positives_KostenNutzenVerhaeltnis()
    {
        var annahmen = ErstelleStandardAnnahmen();

        annahmen.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                20000m, 10000m));

        // Investition 5.000 €, jährliche Einsparung 1.000 €, über 20 Jahre = 20.000 €
        // KNV = 20.000 / 5.000 = 4,0
        var ergebnis = annahmen.Berechnen(5_000m);

        Assert.NotNull(ergebnis.KostenNutzenVerhaeltnis);
        Assert.True(ergebnis.KostenNutzenVerhaeltnis > 0);
        Assert.Equal(20_000m, ergebnis.KumulierteEnergiekosteneinsparung);
    }

    [Fact]
    public void Berechnen_lehnt_negative_Investitionskosten_ab()
    {
        var annahmen = ErstelleStandardAnnahmen();

        Assert.Throws<DomainException>(
            () => annahmen.Berechnen(-1m));
    }

    [Fact]
    public void EnergietraegerAnnahme_berechnet_Einsparung_korrekt()
    {
        var annahme = new EnergietraegerAnnahme(
            Guid.NewGuid(),
            Energietraeger.Gas,
            0.10m, 0.03m, 0.2m, 50m, 0.05m,
            20000m, 12000m);

        Assert.Equal(8000m, annahme.Einsparung);
    }

    [Fact]
    public void EnergietraegerAnnahme_lehnt_negativen_Preis_ab()
    {
        Assert.Throws<DomainException>(
            () => new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                -0.01m, 0m, 0m, 0m, 0m,
                20000m, 10000m));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void EnergietraegerAnnahme_lehnt_ungueltige_Preissteigerungsrate_ab(
        double rate)
    {
        Assert.Throws<DomainException>(
            () => new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, (decimal)rate, 0m, 0m, 0m,
                20000m, 10000m));
    }

    [Fact]
    public void Berechnen_dynamische_Amortisationsdauer_liegt_unter_Betrachtungszeitraum()
    {
        var annahmen = ErstelleStandardAnnahmen();

        annahmen.EnergietraegerAnnahmeHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Gas,
                0.10m, 0m, 0m, 0m, 0m,
                20000m, 10000m));

        // Investition 5.000 €, Einsparung 1.000 €/a, Diskontsatz 4 %
        // Statisch: 5 Jahre, dynamisch sollte auch in Betrachtungszeitraum liegen
        var ergebnis = annahmen.Berechnen(5_000m);

        Assert.NotNull(ergebnis.AmortisationsdauerDynamisch);
        Assert.True(ergebnis.AmortisationsdauerDynamisch <= annahmen.Betrachtungszeitraum);
    }

    private static Wirtschaftlichkeitsannahmen ErstelleStandardAnnahmen()
    {
        return new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            AlternativeId,
            WirtschaftlichkeitsBasis.Bilanziert,
            betrachtungszeitraum: 20,
            diskontsatz: 0.04m,
            inflationsrate: 0.02m,
            jaehrlicheWartungsmehrkosten: 0m,
            nutzungsdauer: 20,
            foerderung: 0m);
    }
}
