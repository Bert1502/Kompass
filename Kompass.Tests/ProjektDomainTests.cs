using Kompass.Domain.Common;
using Kompass.Domain.Projects;

namespace Kompass.Tests.Domain;

public sealed class ProjektDomainTests
{
    [Fact]
    public void Konstruktor_bereinigt_den_Projektnamen()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "  Rathaus  ");

        Assert.Equal(
            "Rathaus",
            projekt.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Konstruktor_lehnt_leeren_Projektnamen_ab(
        string name)
    {
        Assert.Throws<DomainException>(
            () => new Projekt(
                Guid.NewGuid(),
                name));
    }

    [Fact]
    public void Konstruktor_lehnt_Projektnamen_ueber_200_Zeichen_ab()
    {
        Assert.Throws<DomainException>(
            () => new Projekt(
                Guid.NewGuid(),
                new string('P', 201)));
    }

    [Fact]
    public void Umbenennen_bereinigt_und_aktualisiert_den_Namen()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Alt");

        projekt.Umbenennen(
            "  Neu  ");

        Assert.Equal(
            "Neu",
            projekt.Name);
    }

    [Fact]
    public void AlternativeHinzufuegen_ordnet_Alternative_dem_Projekt_zu()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        var alternative =
            new Modernisierungsalternative(
                Guid.NewGuid(),
                "Fenster",
                "Fenstertausch");

        projekt.AlternativeHinzufuegen(
            alternative);

        Assert.Same(
            alternative,
            Assert.Single(
                projekt.Alternativen));
    }

    [Fact]
    public void AlternativeHinzufuegen_lehnt_doppelte_Identitaet_ab()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        var alternativeId =
            Guid.NewGuid();

        projekt.AlternativeHinzufuegen(
            new Modernisierungsalternative(
                alternativeId,
                "Fenster",
                ""));

        Assert.Throws<DomainException>(
            () => projekt.AlternativeHinzufuegen(
                new Modernisierungsalternative(
                    alternativeId,
                    "Dach",
                    "")));
    }

    [Fact]
    public void B56_Alternative_behaelt_Position_und_kann_als_nicht_vorhanden_markiert_werden()
    {
        var alternative =
            new Modernisierungsalternative(
                Guid.NewGuid(),
                "Fenster",
                "Fenstertausch",
                Guid.NewGuid(),
                3);

        alternative
            .AlsNichtMehrImAktuellenB56SnapshotVorhandenKennzeichnen();

        Assert.Equal(
            3,
            alternative.B56Position);
        Assert.False(
            alternative.IstImAktuellenB56SnapshotVorhanden);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void B56_Alternative_lehnt_ungueltige_Position_ab(
        int position)
    {
        Assert.Throws<DomainException>(
            () => new Modernisierungsalternative(
                Guid.NewGuid(),
                "Fenster",
                "Fenstertausch",
                Guid.NewGuid(),
                position));
    }
}
