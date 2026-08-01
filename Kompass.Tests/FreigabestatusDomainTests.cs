using Kompass.Domain.Common;
using Kompass.Domain.Projects;

namespace Kompass.Tests.Domain;

public sealed class FreigabestatusDomainTests
{
    [Fact]
    public void Neues_Projekt_hat_Freigabestatus_NichtFreigegeben()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        Assert.Equal(
            Freigabestatus.NichtFreigegeben,
            projekt.Freigabestatus);
        Assert.Null(projekt.FreigegebenAm);
    }

    [Fact]
    public void FreigabestatusAktualisieren_setzt_ZurFreigabeEingereicht()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        projekt.FreigabestatusAktualisieren(
            Freigabestatus.ZurFreigabeEingereicht);

        Assert.Equal(
            Freigabestatus.ZurFreigabeEingereicht,
            projekt.Freigabestatus);
        Assert.Null(projekt.FreigegebenAm);
    }

    [Fact]
    public void FreigabestatusAktualisieren_setzt_Freigegeben_nach_Einreichung()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        projekt.FreigabestatusAktualisieren(
            Freigabestatus.ZurFreigabeEingereicht);

        var vorFreigabe = DateTime.UtcNow;

        projekt.FreigabestatusAktualisieren(
            Freigabestatus.Freigegeben);

        Assert.Equal(
            Freigabestatus.Freigegeben,
            projekt.Freigabestatus);
        Assert.NotNull(projekt.FreigegebenAm);
        Assert.True(
            projekt.FreigegebenAm >= vorFreigabe);
    }

    [Fact]
    public void FreigabestatusAktualisieren_lehnt_Freigeben_ohne_Einreichung_ab()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        Assert.Throws<DomainException>(
            () => projekt.FreigabestatusAktualisieren(
                Freigabestatus.Freigegeben));
    }

    [Fact]
    public void FreigabestatusAktualisieren_erlaubt_Zuruecksetzen_auf_NichtFreigegeben()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        projekt.FreigabestatusAktualisieren(
            Freigabestatus.ZurFreigabeEingereicht);

        projekt.FreigabestatusAktualisieren(
            Freigabestatus.NichtFreigegeben);

        Assert.Equal(
            Freigabestatus.NichtFreigegeben,
            projekt.Freigabestatus);
    }

    [Fact]
    public void NotizenAktualisieren_setzt_Notizen()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        projekt.NotizenAktualisieren(
            "  Wichtige Anmerkung  ");

        Assert.Equal(
            "Wichtige Anmerkung",
            projekt.Notizen);
    }

    [Fact]
    public void NotizenAktualisieren_setzt_null_auf_null()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        projekt.NotizenAktualisieren("Erste Notiz");
        projekt.NotizenAktualisieren(null);

        Assert.Null(projekt.Notizen);
    }

    [Fact]
    public void NotizenAktualisieren_setzt_leere_Notizen_auf_null()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        projekt.NotizenAktualisieren("   ");

        Assert.Null(projekt.Notizen);
    }

    [Fact]
    public void NotizenAktualisieren_lehnt_zu_lange_Notizen_ab()
    {
        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        Assert.Throws<DomainException>(
            () => projekt.NotizenAktualisieren(
                new string('N', Projekt.MaxNotizenLaenge + 1)));
    }
}
