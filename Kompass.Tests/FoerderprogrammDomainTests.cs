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
        Assert.Single(programm.Foerderquoten);
        Assert.Single(programm.Kumulierbarkeitsregeln);
        Assert.Single(programm.Pflichtnachweisregeln);
        Assert.Single(programm.Gueltigkeitsregeln);
        Assert.Single(programm.Hoechstbetraege);
    }

    [Fact]
    public void Konstruktor_uebernimmt_feinere_Foerderregeln()
    {
        var programm = new Foerderprogramm(
            Guid.NewGuid(),
            "BEG EM",
            2,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "Eigentümer",
            "Fenstertausch",
            "U-Wert ≤ 0,95",
            0.15m,
            30_000m,
            "Nur mit Landesmitteln",
            "Fachunternehmererklärung",
            "BEG 2026",
            [
                new FoerderquoteRegel(
                    Guid.NewGuid(),
                    "Bonusquote",
                    0.2m,
                    "förderfähige Kosten",
                    new DateOnly(2026, 2, 1),
                    null,
                    "Mit iSFP-Bonus")
            ],
            [
                new HoechstbetragRegel(
                    Guid.NewGuid(),
                    "Deckel",
                    60_000m,
                    "EUR",
                    "je Wohneinheit",
                    new DateOnly(2026, 2, 1),
                    null,
                    "Nur bei Komplettsanierung")
            ],
            [
                new Kumulierbarkeitsregel(
                    Guid.NewGuid(),
                    "Landesprogramm",
                    KumulierbarkeitStatus.BedingtKumulierbar,
                    "Nur mit Landesmitteln kombinierbar.",
                    new DateOnly(2026, 2, 1),
                    null)
            ],
            [
                new PflichtnachweisRegel(
                    Guid.NewGuid(),
                    "iSFP",
                    "Vorlage des Sanierungsfahrplans",
                    Nachweiszeitpunkt.BeiAntrag,
                    true,
                    new DateOnly(2026, 2, 1),
                    null)
            ],
            [
                new Gueltigkeitsregel(
                    Guid.NewGuid(),
                    "Antragsdatum 2026",
                    Gueltigkeitsbezug.Antragsdatum,
                    new DateOnly(2026, 2, 1),
                    new DateOnly(2026, 11, 30),
                    "Für Anträge in 2026.")
            ]);

        var foerderquote = Assert.Single(programm.Foerderquoten);
        var hoechstbetrag = Assert.Single(programm.Hoechstbetraege);
        var kumulierbarkeit = Assert.Single(programm.Kumulierbarkeitsregeln);
        var nachweis = Assert.Single(programm.Pflichtnachweisregeln);
        var gueltigkeit = Assert.Single(programm.Gueltigkeitsregeln);

        Assert.Equal("Bonusquote", foerderquote.Bezeichnung);
        Assert.Equal(60_000m, hoechstbetrag.Betrag);
        Assert.Equal(KumulierbarkeitStatus.BedingtKumulierbar, kumulierbarkeit.Status);
        Assert.Equal(Nachweiszeitpunkt.BeiAntrag, nachweis.Zeitpunkt);
        Assert.Equal(Gueltigkeitsbezug.Antragsdatum, gueltigkeit.Bezug);
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

    [Fact]
    public void FoerderquoteRegel_lehnt_Quoten_ueber_eins_ab()
    {
        Assert.Throws<DomainException>(
            () => new FoerderquoteRegel(
                Guid.NewGuid(),
                "Bonusquote",
                1.1m,
                "förderfähige Kosten",
                new DateOnly(2026, 1, 1),
                null));
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
