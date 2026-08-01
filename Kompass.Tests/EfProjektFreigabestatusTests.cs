using Kompass.Domain.Projects;
using Kompass.Persistence.Services;

namespace Kompass.Tests.Persistence;

public sealed class EfProjektFreigabestatusTests
{
    [Fact]
    public async Task FreigabestatusAktualisieren_speichert_und_liest_zurueck()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var erstellt =
            await service.ErstellenAsync("Rathaus");

        await service.FreigabestatusAktualisierenAsync(
            erstellt.Id,
            Freigabestatus.ZurFreigabeEingereicht);

        var gelesen =
            await service.NachIdAbrufenAsync(
                erstellt.Id);

        Assert.Equal(
            Freigabestatus.ZurFreigabeEingereicht,
            gelesen?.Freigabestatus);
        Assert.Null(gelesen?.FreigegebenAm);
    }

    [Fact]
    public async Task FreigabestatusAktualisieren_setzt_FreigegebenAm_bei_Freigabe()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var erstellt =
            await service.ErstellenAsync("Rathaus");

        await service.FreigabestatusAktualisierenAsync(
            erstellt.Id,
            Freigabestatus.ZurFreigabeEingereicht);

        var vorFreigabe = DateTime.UtcNow;

        var freigegeben =
            await service.FreigabestatusAktualisierenAsync(
                erstellt.Id,
                Freigabestatus.Freigegeben);

        Assert.Equal(
            Freigabestatus.Freigegeben,
            freigegeben?.Freigabestatus);
        Assert.NotNull(freigegeben?.FreigegebenAm);
        Assert.True(
            freigegeben?.FreigegebenAm >= vorFreigabe);
    }

    [Fact]
    public async Task FreigabestatusAktualisieren_liefert_null_bei_unbekannter_Id()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var ergebnis =
            await service.FreigabestatusAktualisierenAsync(
                Guid.NewGuid(),
                Freigabestatus.ZurFreigabeEingereicht);

        Assert.Null(ergebnis);
    }

    [Fact]
    public async Task NotizenAktualisieren_speichert_und_liest_zurueck()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var erstellt =
            await service.ErstellenAsync("Rathaus");

        var aktualisiert =
            await service.NotizenAktualisierenAsync(
                erstellt.Id,
                "Projektnotiz zu diesem Gebäude.");

        var gelesen =
            await service.NachIdAbrufenAsync(
                erstellt.Id);

        Assert.Equal(
            "Projektnotiz zu diesem Gebäude.",
            aktualisiert?.Notizen);
        Assert.Equal(aktualisiert, gelesen);
    }

    [Fact]
    public async Task NotizenAktualisieren_loescht_Notiz_bei_null()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var erstellt =
            await service.ErstellenAsync("Rathaus");

        await service.NotizenAktualisierenAsync(
            erstellt.Id,
            "Eine Notiz");

        var geloescht =
            await service.NotizenAktualisierenAsync(
                erstellt.Id,
                null);

        Assert.Null(geloescht?.Notizen);
    }

    [Fact]
    public async Task NotizenAktualisieren_liefert_null_bei_unbekannter_Id()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var service =
            new ProjektService(
                testdatenbank.Context);

        var ergebnis =
            await service.NotizenAktualisierenAsync(
                Guid.NewGuid(),
                "Notiz");

        Assert.Null(ergebnis);
    }
}
