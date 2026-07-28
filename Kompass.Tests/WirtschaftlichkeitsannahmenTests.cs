using Kompass.Domain.Common;
using Kompass.Domain.Economics;

namespace Kompass.Tests.Domain;

public sealed class WirtschaftlichkeitsannahmenTests
{
    [Fact]
    public void Gueltige_Annahmen_koennen_erstellt_werden()
    {
        var annahmen =
            ErstelleGueltigeAnnahmen();

        Assert.Equal(20, annahmen.BetrachtungszeitraumJahre);
        Assert.Equal(3.0m, annahmen.DiskontsatzProzent);
        Assert.Equal(2.0m, annahmen.InflationsrateProzent);
        Assert.Equal(50.0m, annahmen.Co2PreisProTonne);
        Assert.Equal(5.0m, annahmen.JaehrlicherCo2PreisanstiegProzent);
        Assert.Equal(500.0m, annahmen.WartungUndInstandhaltungProJahr);
        Assert.Equal(20, annahmen.NutzungsdauerJahre);
        Assert.Equal(10.0m, annahmen.RestwertProzent);
    }

    [Fact]
    public void Betrachtungszeitraum_kleiner_1_wirft_DomainException()
    {
        Assert.Throws<DomainException>(
            () =>
                new Wirtschaftlichkeitsannahmen(
                    Guid.NewGuid(),
                    betrachtungszeitraumJahre: 0,
                    diskontsatzProzent: 3,
                    inflationsrateProzent: 2,
                    co2PreisProTonne: 50,
                    jaehrlicherCo2PreisanstiegProzent: 5,
                    wartungUndInstandhaltungProJahr: 500,
                    nutzungsdauerJahre: 20,
                    restwertProzent: 10));
    }

    [Fact]
    public void Negativer_Diskontsatz_wirft_DomainException()
    {
        Assert.Throws<DomainException>(
            () =>
                new Wirtschaftlichkeitsannahmen(
                    Guid.NewGuid(),
                    betrachtungszeitraumJahre: 20,
                    diskontsatzProzent: -1,
                    inflationsrateProzent: 2,
                    co2PreisProTonne: 50,
                    jaehrlicherCo2PreisanstiegProzent: 5,
                    wartungUndInstandhaltungProJahr: 500,
                    nutzungsdauerJahre: 20,
                    restwertProzent: 10));
    }

    [Fact]
    public void Restwert_ueber_100_wirft_DomainException()
    {
        Assert.Throws<DomainException>(
            () =>
                new Wirtschaftlichkeitsannahmen(
                    Guid.NewGuid(),
                    betrachtungszeitraumJahre: 20,
                    diskontsatzProzent: 3,
                    inflationsrateProzent: 2,
                    co2PreisProTonne: 50,
                    jaehrlicherCo2PreisanstiegProzent: 5,
                    wartungUndInstandhaltungProJahr: 500,
                    nutzungsdauerJahre: 20,
                    restwertProzent: 101));
    }

    [Fact]
    public void Energietraeger_kann_hinzugefuegt_werden()
    {
        var annahmen = ErstelleGueltigeAnnahmen();

        annahmen.EnergietraegerHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Erdgas,
                preisProKwh: 0.12m,
                jaehrlicherPreisanstiegProzent: 3));

        var eintrag = Assert.Single(annahmen.Energietraeger);
        Assert.Equal(Energietraeger.Erdgas, eintrag.Energietraeger);
        Assert.Equal(0.12m, eintrag.PreisProKwh);
    }

    [Fact]
    public void Gleicher_Energietraeger_zweimal_wirft_DomainException()
    {
        var annahmen = ErstelleGueltigeAnnahmen();

        annahmen.EnergietraegerHinzufuegen(
            new EnergietraegerAnnahme(
                Guid.NewGuid(),
                Energietraeger.Strom,
                preisProKwh: 0.30m,
                jaehrlicherPreisanstiegProzent: 2));

        Assert.Throws<DomainException>(
            () =>
                annahmen.EnergietraegerHinzufuegen(
                    new EnergietraegerAnnahme(
                        Guid.NewGuid(),
                        Energietraeger.Strom,
                        preisProKwh: 0.32m,
                        jaehrlicherPreisanstiegProzent: 2)));
    }

    [Fact]
    public void Annahmen_koennen_geaendert_werden()
    {
        var annahmen = ErstelleGueltigeAnnahmen();

        annahmen.AnnahmenAendern(
            betrachtungszeitraumJahre: 30,
            diskontsatzProzent: 4,
            inflationsrateProzent: 3,
            co2PreisProTonne: 80,
            jaehrlicherCo2PreisanstiegProzent: 6,
            wartungUndInstandhaltungProJahr: 600,
            nutzungsdauerJahre: 30,
            restwertProzent: 0);

        Assert.Equal(30, annahmen.BetrachtungszeitraumJahre);
        Assert.Equal(4m, annahmen.DiskontsatzProzent);
    }

    private static Wirtschaftlichkeitsannahmen ErstelleGueltigeAnnahmen()
    {
        return new Wirtschaftlichkeitsannahmen(
            Guid.NewGuid(),
            betrachtungszeitraumJahre: 20,
            diskontsatzProzent: 3.0m,
            inflationsrateProzent: 2.0m,
            co2PreisProTonne: 50.0m,
            jaehrlicherCo2PreisanstiegProzent: 5.0m,
            wartungUndInstandhaltungProJahr: 500.0m,
            nutzungsdauerJahre: 20,
            restwertProzent: 10.0m);
    }
}
