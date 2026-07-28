using Kompass.Domain.Common;
using Kompass.Domain.Waermebruecken;

namespace Kompass.Tests.Domain;

public sealed class WaermebrueckeDomainTests
{
    [Fact]
    public void Konstruktor_erzeugt_Waermebruecke_mit_Standardwerten()
    {
        var id = Guid.NewGuid();
        var projektId = Guid.NewGuid();

        var wb = new Waermebruecke(
            id,
            projektId,
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

        Assert.Equal(id, wb.Id);
        Assert.Equal(projektId, wb.ProjektId);
        Assert.Equal("WB01", wb.InterneNummer);
        Assert.Equal("Außenwandecke", wb.Bezeichnung);
        Assert.Equal(WaermebrueckeTyp.Ecke, wb.Typ);
        Assert.Equal(WaermebrueckeStatus.Offen, wb.Status);
        Assert.Equal(GleichwertigkeitStatus.NichtBewertet, wb.GleichwertigkeitStatus);
        Assert.Null(wb.Lage);
        Assert.Null(wb.PsiWert);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Konstruktor_wirft_bei_leerer_InternerNummer(string interneNummer)
    {
        Assert.Throws<DomainException>(
            () => new Waermebruecke(
                Guid.NewGuid(),
                Guid.NewGuid(),
                interneNummer,
                "Bezeichnung",
                WaermebrueckeTyp.Wandanschluss));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Konstruktor_wirft_bei_leerer_Bezeichnung(string bezeichnung)
    {
        Assert.Throws<DomainException>(
            () => new Waermebruecke(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "WB01",
                bezeichnung,
                WaermebrueckeTyp.Wandanschluss));
    }

    [Fact]
    public void Konstruktor_wirft_bei_leerer_ProjektId()
    {
        Assert.Throws<DomainException>(
            () => new Waermebruecke(
                Guid.NewGuid(),
                Guid.Empty,
                "WB01",
                "Bezeichnung",
                WaermebrueckeTyp.Wandanschluss));
    }

    [Fact]
    public void Konstruktor_wirft_bei_zu_langer_InternerNummer()
    {
        var zuLang = new string('X', Waermebruecke.MaxInterneNummerLaenge + 1);

        Assert.Throws<DomainException>(
            () => new Waermebruecke(
                Guid.NewGuid(),
                Guid.NewGuid(),
                zuLang,
                "Bezeichnung",
                WaermebrueckeTyp.Wandanschluss));
    }

    [Fact]
    public void DatenAktualisieren_aktualisiert_alle_Felder()
    {
        var wb = new Waermebruecke(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WB01",
            "Alt",
            WaermebrueckeTyp.Wandanschluss);

        wb.DatenAktualisieren(
            "WB02",
            "Neu",
            WaermebrueckeTyp.Fensteranschluss,
            WaermebrueckeStatus.Berechnet,
            GleichwertigkeitStatus.Beiblatt2Nachgewiesen,
            lage: "EG Nord",
            planreferenz: "Plan-01",
            detailreferenz: "Detail-A",
            fremdnummer: "TCA-001",
            laenge: 3.5m,
            beiblatt2Referenz: "BB2-§7",
            thermCadReferenz: "TC-Projekt-5",
            psiWert: 0.082m,
            fRsi: 0.71m,
            pruefanmerkung: "Prüfung ok",
            berichtsdarstellung: "Abb. 3");

        Assert.Equal("WB02", wb.InterneNummer);
        Assert.Equal("Neu", wb.Bezeichnung);
        Assert.Equal(WaermebrueckeTyp.Fensteranschluss, wb.Typ);
        Assert.Equal(WaermebrueckeStatus.Berechnet, wb.Status);
        Assert.Equal(GleichwertigkeitStatus.Beiblatt2Nachgewiesen, wb.GleichwertigkeitStatus);
        Assert.Equal("EG Nord", wb.Lage);
        Assert.Equal("Plan-01", wb.Planreferenz);
        Assert.Equal("Detail-A", wb.Detailreferenz);
        Assert.Equal("TCA-001", wb.Fremdnummer);
        Assert.Equal(3.5m, wb.Laenge);
        Assert.Equal("BB2-§7", wb.Beiblatt2Referenz);
        Assert.Equal("TC-Projekt-5", wb.ThermCadReferenz);
        Assert.Equal(0.082m, wb.PsiWert);
        Assert.Equal(0.71m, wb.FRsi);
        Assert.Equal("Prüfung ok", wb.Pruefanmerkung);
        Assert.Equal("Abb. 3", wb.Berichtsdarstellung);
    }

    [Fact]
    public void DatenAktualisieren_wirft_bei_negativer_Laenge()
    {
        var wb = new Waermebruecke(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WB01",
            "Bezeichnung",
            WaermebrueckeTyp.Wandanschluss);

        Assert.Throws<DomainException>(
            () => wb.DatenAktualisieren(
                "WB01",
                "Bezeichnung",
                WaermebrueckeTyp.Wandanschluss,
                WaermebrueckeStatus.Offen,
                GleichwertigkeitStatus.NichtBewertet,
                laenge: -1m));
    }

    [Fact]
    public void DatenAktualisieren_setzt_optionale_Felder_auf_null_wenn_null_uebergeben()
    {
        var wb = new Waermebruecke(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WB01",
            "Bezeichnung",
            WaermebrueckeTyp.Wandanschluss);

        wb.DatenAktualisieren(
            "WB01",
            "Bezeichnung",
            WaermebrueckeTyp.Wandanschluss,
            WaermebrueckeStatus.Offen,
            GleichwertigkeitStatus.NichtBewertet,
            lage: null,
            psiWert: null);

        Assert.Null(wb.Lage);
        Assert.Null(wb.PsiWert);
    }
}
