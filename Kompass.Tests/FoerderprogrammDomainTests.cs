using Kompass.Domain.Common;
using Kompass.Domain.Funding;

namespace Kompass.Tests.Domain;

public sealed class FoerderprogrammDomainTests
{
    [Fact]
    public void Konstruktor_erstellt_Foerderprogramm_mit_gueltigen_Werten()
    {
        var programm = ErzeugeFoerderprogramm();

        Assert.Equal("BEG EM", programm.Programmkennung);
        Assert.Equal(1, programm.Version);
        Assert.Equal(new DateOnly(2026, 1, 1), programm.GueltigAb);
        Assert.Equal(0.15m, programm.Foerdersatz);
    }

    [Fact]
    public void Konstruktor_lehnt_leere_Programmkennung_ab()
    {
        Assert.Throws<DomainException>(
            () => new Foerderprogramm(
                Guid.NewGuid(),
                "",
                1,
                new DateOnly(2026, 1, 1),
                null,
                "Eigentümer",
                "Fenstertausch",
                "U-Wert ≤ 0,95",
                0.15m,
                30_000m,
                "Nicht mit Programm X kumulierbar",
                "Fachunternehmererklärung",
                "BEG 2026"));
    }

    [Fact]
    public void Konstruktor_lehnt_Version_kleiner_1_ab()
    {
        Assert.Throws<DomainException>(
            () => new Foerderprogramm(
                Guid.NewGuid(),
                "BEG EM",
                0,
                new DateOnly(2026, 1, 1),
                null,
                "Eigentümer",
                "Fenstertausch",
                "U-Wert ≤ 0,95",
                0.15m,
                30_000m,
                "Nicht mit Programm X kumulierbar",
                "Fachunternehmererklärung",
                "BEG 2026"));
    }

    [Fact]
    public void Konstruktor_lehnt_GueltigBis_vor_GueltigAb_ab()
    {
        Assert.Throws<DomainException>(
            () => new Foerderprogramm(
                Guid.NewGuid(),
                "BEG EM",
                1,
                new DateOnly(2026, 1, 1),
                new DateOnly(2025, 12, 31),
                "Eigentümer",
                "Fenstertausch",
                "U-Wert ≤ 0,95",
                0.15m,
                30_000m,
                "Nicht mit Programm X kumulierbar",
                "Fachunternehmererklärung",
                "BEG 2026"));
    }

    [Fact]
    public void Konstruktor_lehnt_negativen_Foerdersatz_ab()
    {
        Assert.Throws<DomainException>(
            () => new Foerderprogramm(
                Guid.NewGuid(),
                "BEG EM",
                1,
                new DateOnly(2026, 1, 1),
                null,
                "Eigentümer",
                "Fenstertausch",
                "U-Wert ≤ 0,95",
                -0.01m,
                30_000m,
                "Nicht mit Programm X kumulierbar",
                "Fachunternehmererklärung",
                "BEG 2026"));
    }

    [Fact]
    public void Konstruktor_lehnt_negativen_Hoechstbetrag_ab()
    {
        Assert.Throws<DomainException>(
            () => new Foerderprogramm(
                Guid.NewGuid(),
                "BEG EM",
                1,
                new DateOnly(2026, 1, 1),
                null,
                "Eigentümer",
                "Fenstertausch",
                "U-Wert ≤ 0,95",
                0.15m,
                -1m,
                "Nicht mit Programm X kumulierbar",
                "Fachunternehmererklärung",
                "BEG 2026"));
    }

    private static Foerderprogramm ErzeugeFoerderprogramm(
        string programmkennung = "BEG EM",
        int version = 1)
    {
        return new Foerderprogramm(
            Guid.NewGuid(),
            programmkennung,
            version,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "Eigentümer",
            "Fenstertausch",
            "U-Wert ≤ 0,95",
            0.15m,
            30_000m,
            "Nicht mit Programm X kumulierbar",
            "Fachunternehmererklärung",
            "BEG 2026");
    }
}
