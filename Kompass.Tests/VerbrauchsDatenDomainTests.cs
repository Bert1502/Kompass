using Kompass.Domain.Common;
using Kompass.Domain.Economics;
using Kompass.Domain.Verbrauch;

namespace Kompass.Tests.Domain;

public sealed class VerbrauchsDatenDomainTests
{
    [Fact]
    public void Konstruktor_erzeugt_VerbrauchsDaten_mit_Pflichtfeldern()
    {
        var id = Guid.NewGuid();
        var projektId = Guid.NewGuid();
        var periodeVon = new DateOnly(2024, 1, 1);
        var periodeBis = new DateOnly(2024, 12, 31);

        var daten = new VerbrauchsDaten(
            id,
            projektId,
            periodeVon,
            periodeBis,
            Energietraeger.Gas,
            12000m,
            2400m);

        Assert.Equal(id, daten.Id);
        Assert.Equal(projektId, daten.ProjektId);
        Assert.Equal(periodeVon, daten.PeriodeVon);
        Assert.Equal(periodeBis, daten.PeriodeBis);
        Assert.Equal(Energietraeger.Gas, daten.Energietraeger);
        Assert.Equal(12000m, daten.Menge);
        Assert.Equal(2400m, daten.Kosten);
        Assert.Null(daten.WitterungsbereinigungsFaktor);
        Assert.Null(daten.Flaeche);
        Assert.Null(daten.B56VergleichsWert);
        Assert.Null(daten.AnpassungsFaktor);
        Assert.Null(daten.AnpassungsBegruendung);
        Assert.Null(daten.Abweichungsursache);
    }

    [Fact]
    public void Konstruktor_wirft_bei_leerem_ProjektId()
    {
        Assert.Throws<DomainException>(
            () => new VerbrauchsDaten(
                Guid.NewGuid(),
                Guid.Empty,
                new DateOnly(2024, 1, 1),
                new DateOnly(2024, 12, 31),
                Energietraeger.Gas,
                12000m,
                2400m));
    }

    [Fact]
    public void Konstruktor_wirft_wenn_PeriodeBis_vor_PeriodeVon()
    {
        Assert.Throws<DomainException>(
            () => new VerbrauchsDaten(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2024, 12, 31),
                new DateOnly(2024, 1, 1),
                Energietraeger.Gas,
                12000m,
                2400m));
    }

    [Fact]
    public void Konstruktor_wirft_bei_negativer_Menge()
    {
        Assert.Throws<DomainException>(
            () => new VerbrauchsDaten(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2024, 1, 1),
                new DateOnly(2024, 12, 31),
                Energietraeger.Gas,
                -1m,
                2400m));
    }

    [Fact]
    public void Konstruktor_wirft_bei_negativen_Kosten()
    {
        Assert.Throws<DomainException>(
            () => new VerbrauchsDaten(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2024, 1, 1),
                new DateOnly(2024, 12, 31),
                Energietraeger.Gas,
                12000m,
                -100m));
    }

    [Fact]
    public void Aktualisieren_setzt_alle_optionalen_Felder()
    {
        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m);

        daten.Aktualisieren(
            new DateOnly(2023, 1, 1),
            new DateOnly(2023, 12, 31),
            Energietraeger.Heizoel,
            10000m,
            2000m,
            witterungsbereinigungsFaktor: 1.12m,
            flaeche: 200m,
            b56VergleichsWert: 15000m,
            anpassungsFaktor: 0.9m,
            anpassungsBegruendung: "Leerstand im Sommer",
            abweichungsursache: "Realer Verbrauch deutlich unter Bilanz");

        Assert.Equal(new DateOnly(2023, 1, 1), daten.PeriodeVon);
        Assert.Equal(new DateOnly(2023, 12, 31), daten.PeriodeBis);
        Assert.Equal(Energietraeger.Heizoel, daten.Energietraeger);
        Assert.Equal(10000m, daten.Menge);
        Assert.Equal(2000m, daten.Kosten);
        Assert.Equal(1.12m, daten.WitterungsbereinigungsFaktor);
        Assert.Equal(200m, daten.Flaeche);
        Assert.Equal(15000m, daten.B56VergleichsWert);
        Assert.Equal(0.9m, daten.AnpassungsFaktor);
        Assert.Equal("Leerstand im Sommer", daten.AnpassungsBegruendung);
        Assert.Equal("Realer Verbrauch deutlich unter Bilanz", daten.Abweichungsursache);
    }

    [Fact]
    public void WitterungsbereinigteMenge_verwendet_Faktor_wenn_gesetzt()
    {
        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            10000m,
            2000m);

        daten.Aktualisieren(
            daten.PeriodeVon,
            daten.PeriodeBis,
            daten.Energietraeger,
            10000m,
            2000m,
            witterungsbereinigungsFaktor: 1.1m,
            flaeche: null,
            b56VergleichsWert: null,
            anpassungsFaktor: null,
            anpassungsBegruendung: null,
            abweichungsursache: null);

        Assert.Equal(11000m, daten.WitterungsbereinigteMenge);
    }

    [Fact]
    public void MengeJeFlaeche_liefert_null_wenn_keine_Flaeche_gesetzt()
    {
        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            10000m,
            2000m);

        Assert.Null(daten.MengeJeFlaeche);
    }

    [Fact]
    public void MengeJeFlaeche_berechnet_Flaechenbezug_wenn_Flaeche_gesetzt()
    {
        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            10000m,
            2000m);

        daten.Aktualisieren(
            daten.PeriodeVon,
            daten.PeriodeBis,
            daten.Energietraeger,
            10000m,
            2000m,
            witterungsbereinigungsFaktor: null,
            flaeche: 200m,
            b56VergleichsWert: null,
            anpassungsFaktor: null,
            anpassungsBegruendung: null,
            abweichungsursache: null);

        Assert.Equal(50m, daten.MengeJeFlaeche);
    }

    [Fact]
    public void Aktualisieren_wirft_bei_negativem_Witterungsbereinigungsfaktor()
    {
        var daten = new VerbrauchsDaten(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            10000m,
            2000m);

        Assert.Throws<DomainException>(
            () => daten.Aktualisieren(
                daten.PeriodeVon,
                daten.PeriodeBis,
                daten.Energietraeger,
                10000m,
                2000m,
                witterungsbereinigungsFaktor: -0.5m,
                flaeche: null,
                b56VergleichsWert: null,
                anpassungsFaktor: null,
                anpassungsBegruendung: null,
                abweichungsursache: null));
    }
}
