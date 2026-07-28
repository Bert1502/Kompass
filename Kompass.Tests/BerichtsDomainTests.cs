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
