using Kompass.Domain.Economics;
using Kompass.Domain.Projects;
using Kompass.Domain.Reports;

namespace Kompass.Tests.Domain;

public sealed class BerichtsDomainTests
{
    [Fact]
    public void Berichtskopf_erzeugt_korrekte_Werte()
    {
        var projektId = Guid.NewGuid();
        var snapshotId = Guid.NewGuid();
        var erstelltAm = DateTimeOffset.UtcNow;

        var kopf = new Berichtskopf(
            projektId,
            "Mustergebäude",
            "INT-001",
            Bearbeitungsstatus.InBearbeitung,
            snapshotId,
            erstelltAm,
            Berichtstyp.Alternativenvergleich);

        Assert.Equal(projektId, kopf.ProjektId);
        Assert.Equal("Mustergebäude", kopf.ProjektName);
        Assert.Equal("INT-001", kopf.InterneBezeichnung);
        Assert.Equal(Bearbeitungsstatus.InBearbeitung, kopf.Bearbeitungsstatus);
        Assert.Equal(snapshotId, kopf.QuellSnapshotId);
        Assert.Equal(erstelltAm, kopf.ErstelltAm);
        Assert.Equal(Berichtstyp.Alternativenvergleich, kopf.Berichtstyp);
    }

    [Fact]
    public void Berichtskopf_erlaubt_null_InterneBezeichnung_und_QuellSnapshotId()
    {
        var kopf = new Berichtskopf(
            Guid.NewGuid(),
            "Projekt",
            null,
            Bearbeitungsstatus.InBearbeitung,
            null,
            DateTimeOffset.UtcNow,
            Berichtstyp.Waermebrueckenuebersicht);

        Assert.Null(kopf.InterneBezeichnung);
        Assert.Null(kopf.QuellSnapshotId);
    }

    [Fact]
    public void AlternativenvergleichZeile_erzeugt_korrekte_Werte()
    {
        var alternativeId = Guid.NewGuid();

        var zeile = new AlternativenvergleichZeile(
            alternativeId,
            3,
            "Vollsanierung",
            "Kurztext",
            48000m,
            2,
            true);

        Assert.Equal(alternativeId, zeile.AlternativeId);
        Assert.Equal(3, zeile.B56Position);
        Assert.Equal("Vollsanierung", zeile.Bezeichnung);
        Assert.Equal(48000m, zeile.Gesamtkosten);
        Assert.Equal(2, zeile.AnzahlKostenpositionen);
        Assert.True(zeile.IstImAktuellenB56SnapshotVorhanden);
    }

    [Fact]
    public void AlternativenvergleichBericht_erzeugt_korrekte_Struktur()
    {
        var kopf = ErstelleBerichtskopf(Berichtstyp.Alternativenvergleich);
        var zeilen = new List<AlternativenvergleichZeile>
        {
            new(Guid.NewGuid(), 1, "Alt A", "Kurztext A", 10000m, 1, true),
            new(Guid.NewGuid(), 2, "Alt B", "Kurztext B", 20000m, 0, false),
        };

        var bericht = new AlternativenvergleichBericht(kopf, zeilen);

        Assert.Equal(kopf, bericht.Kopf);
        Assert.Equal(2, bericht.Alternativen.Count);
        Assert.Equal(Berichtstyp.Alternativenvergleich, bericht.Kopf.Berichtstyp);
    }

    [Fact]
    public void AlternativenvergleichBericht_erlaubt_leere_Alternativenliste()
    {
        var bericht = new AlternativenvergleichBericht(
            ErstelleBerichtskopf(Berichtstyp.Alternativenvergleich),
            []);

        Assert.Empty(bericht.Alternativen);
    }

    [Fact]
    public void WaermebrueckenuebersichtBericht_erzeugt_korrekte_Struktur()
    {
        var kopf = ErstelleBerichtskopf(Berichtstyp.Waermebrueckenuebersicht);

        var bericht = new WaermebrueckenuebersichtBericht(kopf, []);

        Assert.Equal(kopf, bericht.Kopf);
        Assert.Empty(bericht.Waermebruecken);
        Assert.Equal(Berichtstyp.Waermebrueckenuebersicht, bericht.Kopf.Berichtstyp);
    }

    [Fact]
    public void Berichtstyp_enum_enthaelt_alle_erwarteten_Typen()
    {
        var typen = Enum.GetValues<Berichtstyp>();

        Assert.Contains(Berichtstyp.Alternativenvergleich, typen);
        Assert.Contains(Berichtstyp.Foerderuebersicht, typen);
        Assert.Contains(Berichtstyp.Waermebrueckenuebersicht, typen);
        Assert.Contains(Berichtstyp.Wirtschaftlichkeitsbericht, typen);
        Assert.Contains(Berichtstyp.Energieberatungsbericht, typen);
    }

    [Fact]
    public void WirtschaftlichkeitsberichtZeile_erzeugt_korrekte_Werte()
    {
        var alternativeId = Guid.NewGuid();
        var ergebnis = new Wirtschaftlichkeitsergebnis(
            Eigenanteil: 40000m,
            JaehrlicheEnergiekosteneinsparungJahr1: 2000m,
            KumulierteEnergiekosteneinsparung: 30000m,
            AmortisationsdauerStatisch: 20m,
            AmortisationsdauerDynamisch: 22m,
            Kapitalwert: -15000m,
            KostenNutzenVerhaeltnis: 0.75m);

        var zeile = new WirtschaftlichkeitsberichtZeile(
            alternativeId,
            B56Position: 1,
            Bezeichnung: "Vollsanierung",
            Basis: WirtschaftlichkeitsBasis.Bilanziert,
            Investitionskosten: 50000m,
            Foerderung: 10000m,
            Betrachtungszeitraum: 20,
            Diskontsatz: 0.04m,
            Inflationsrate: 0.02m,
            Ergebnis: ergebnis);

        Assert.Equal(alternativeId, zeile.AlternativeId);
        Assert.Equal(1, zeile.B56Position);
        Assert.Equal("Vollsanierung", zeile.Bezeichnung);
        Assert.Equal(WirtschaftlichkeitsBasis.Bilanziert, zeile.Basis);
        Assert.Equal(50000m, zeile.Investitionskosten);
        Assert.Equal(10000m, zeile.Foerderung);
        Assert.Equal(20, zeile.Betrachtungszeitraum);
        Assert.Equal(ergebnis, zeile.Ergebnis);
    }

    [Fact]
    public void WirtschaftlichkeitsberichtBericht_erzeugt_korrekte_Struktur()
    {
        var kopf = ErstelleBerichtskopf(Berichtstyp.Wirtschaftlichkeitsbericht);
        var bericht = new WirtschaftlichkeitsberichtBericht(kopf, []);

        Assert.Equal(kopf, bericht.Kopf);
        Assert.Empty(bericht.Alternativen);
        Assert.Equal(Berichtstyp.Wirtschaftlichkeitsbericht, bericht.Kopf.Berichtstyp);
    }

    [Fact]
    public void FoerderuebersichtAlternative_erzeugt_korrekte_Werte()
    {
        var alternativeId = Guid.NewGuid();

        var zeile = new FoerderuebersichtAlternative(
            alternativeId,
            B56Position: 2,
            Bezeichnung: "Teilsanierung",
            Gesamtkosten: 20000m,
            ZugeordneteProgramme: []);

        Assert.Equal(alternativeId, zeile.AlternativeId);
        Assert.Equal(2, zeile.B56Position);
        Assert.Equal("Teilsanierung", zeile.Bezeichnung);
        Assert.Equal(20000m, zeile.Gesamtkosten);
        Assert.Empty(zeile.ZugeordneteProgramme);
    }

    [Fact]
    public void FoerderuebersichtBericht_erzeugt_korrekte_Struktur()
    {
        var kopf = ErstelleBerichtskopf(Berichtstyp.Foerderuebersicht);
        var bericht = new FoerderuebersichtBericht(kopf, []);

        Assert.Equal(kopf, bericht.Kopf);
        Assert.Empty(bericht.Alternativen);
        Assert.Equal(Berichtstyp.Foerderuebersicht, bericht.Kopf.Berichtstyp);
    }

    private static Berichtskopf ErstelleBerichtskopf(Berichtstyp typ) =>
        new(
            Guid.NewGuid(),
            "Testprojekt",
            null,
            Bearbeitungsstatus.InBearbeitung,
            null,
            DateTimeOffset.UtcNow,
            typ);
}
