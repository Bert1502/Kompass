using Kompass.Application.B56Import;
using Kompass.Persistence.Services;

namespace Kompass.Tests.Persistence;

public sealed class ProjektB56ImportBeziehungTests
{
    [Fact]
    public async Task Projektloeschung_bewahrt_unveraenderlichen_B56_Snapshot()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var projektService =
            new ProjektService(
                testdatenbank.Context);

        var importRegister =
            new EfB56ImportRegister(
                testdatenbank.Context);

        var projekt =
            await projektService.ErstellenAsync(
                "Rathaus");

        var importEintrag =
            new B56ImportEintrag
            {
                ImportId = Guid.NewGuid(),
                ProjektId = projekt.Id,
                Projektname = projekt.Name,
                Originaldateiname = "rathaus.xlsm",
                Archivdateipfad = "archiv/rathaus.xlsm",
                Sha256 = new string('a', 64),
                DateigroesseBytes = 1024,
                ImportiertAm = DateTimeOffset.Parse(
                    "2026-07-25T08:00:00+02:00"),
                Dateiendung = ".xlsm"
            };

        await importRegister.EintragSpeichernAsync(
            importEintrag);

        var wurdeGeloescht =
            await projektService.LoeschenAsync(
                projekt.Id);

        var snapshots =
            await importRegister.AlleFuerProjektAbrufenAsync(
                projekt.Id);

        Assert.True(
            wurdeGeloescht);
        Assert.Equal(
            importEintrag,
            Assert.Single(
                snapshots));
    }

    [Fact]
    public async Task Snapshot_bleibt_auf_seine_ProjektId_begrenzt()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var projektService =
            new ProjektService(
                testdatenbank.Context);

        var importRegister =
            new EfB56ImportRegister(
                testdatenbank.Context);

        var erstesProjekt =
            await projektService.ErstellenAsync(
                "Rathaus");

        var zweitesProjekt =
            await projektService.ErstellenAsync(
                "Schule");

        var importEintrag =
            new B56ImportEintrag
            {
                ImportId = Guid.NewGuid(),
                ProjektId = erstesProjekt.Id,
                Projektname = erstesProjekt.Name,
                Originaldateiname = "rathaus.xlsm",
                Archivdateipfad = "archiv/rathaus.xlsm",
                Sha256 = new string('b', 64),
                DateigroesseBytes = 1024,
                ImportiertAm = DateTimeOffset.Parse(
                    "2026-07-25T08:00:00+02:00"),
                Dateiendung = ".xlsm"
            };

        await importRegister.EintragSpeichernAsync(
            importEintrag);

        var ersterTreffer =
            await importRegister.NachHashSuchenAsync(
                erstesProjekt.Id,
                importEintrag.Sha256);

        var fremderTreffer =
            await importRegister.NachHashSuchenAsync(
                zweitesProjekt.Id,
                importEintrag.Sha256);

        Assert.Equal(
            importEintrag.ImportId,
            ersterTreffer?.ImportId);
        Assert.Null(
            fremderTreffer);
    }
}
