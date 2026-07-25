using Kompass.Application.B56Import;
using Kompass.Domain.Projects;
using Kompass.Persistence.Services;
using Microsoft.EntityFrameworkCore;

namespace Kompass.Tests.Persistence;

public sealed class B56ProjektmodellUebernahmeServiceTests
{
    [Fact]
    public async Task Bestaetigter_Snapshot_wird_einmalig_mit_Herkunft_uebernommen()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        testdatenbank.Context.Projekte.Add(
            projekt);

        await testdatenbank.Context.SaveChangesAsync();

        var register =
            new EfB56ImportRegister(
                testdatenbank.Context);

        var snapshot =
            ErzeugeSnapshot(
                projekt.Id,
                B56SnapshotStatus.FachlichBestaetigt);

        await register.EintragMitFachdatenSpeichernAsync(
            snapshot,
            ErzeugeFachdaten());

        var service =
            new B56ProjektmodellUebernahmeService(
                testdatenbank.Context,
                register);

        var ersteUebernahme =
            await service.UebernehmenAsync(
                projekt.Id,
                snapshot.ImportId);

        var zweiteUebernahme =
            await service.UebernehmenAsync(
                projekt.Id,
                snapshot.ImportId);

        testdatenbank.Context.ChangeTracker.Clear();

        var gespeichertesProjekt =
            await testdatenbank.Context.Projekte
                .Include(eintrag => eintrag.Alternativen)
                    .ThenInclude(
                        alternative =>
                            alternative.Bauteile)
                .SingleAsync(
                    eintrag =>
                        eintrag.Id == projekt.Id);

        var gespeicherterSnapshot =
            await register.NachIdSuchenAsync(
                projekt.Id,
                snapshot.ImportId);

        Assert.Equal(
            B56ProjektmodellUebernahmeStatus.Erfolgreich,
            ersteUebernahme.Status);
        Assert.Equal(
            B56ProjektmodellUebernahmeStatus.Erfolgreich,
            zweiteUebernahme.Status);
        Assert.Equal(
            snapshot.ImportId,
            gespeichertesProjekt.QuellSnapshotId);
        Assert.Equal(
            1,
            gespeichertesProjekt.ProjektmodellVersion);

        var alternative =
            Assert.Single(
                gespeichertesProjekt.Alternativen);

        Assert.Equal(
            "Fenstertausch",
            alternative.Bezeichnung);
        Assert.Equal(
            snapshot.ImportId,
            alternative.QuellSnapshotId);
        Assert.Single(
            alternative.Bauteile);
        Assert.Equal(
            B56SnapshotStatus.InProjektmodellUebernommen,
            gespeicherterSnapshot?.SnapshotStatus);
    }

    [Fact]
    public async Task Unbestaetigter_Snapshot_wird_nicht_uebernommen()
    {
        await using var testdatenbank =
            await ProjektTestdatenbank.ErstellenAsync();

        var projekt =
            new Projekt(
                Guid.NewGuid(),
                "Rathaus");

        testdatenbank.Context.Projekte.Add(
            projekt);

        await testdatenbank.Context.SaveChangesAsync();

        var register =
            new EfB56ImportRegister(
                testdatenbank.Context);

        var snapshot =
            ErzeugeSnapshot(
                projekt.Id,
                B56SnapshotStatus.MitWarnungen);

        await register.EintragMitFachdatenSpeichernAsync(
            snapshot,
            ErzeugeFachdaten());

        var service =
            new B56ProjektmodellUebernahmeService(
                testdatenbank.Context,
                register);

        var ergebnis =
            await service.UebernehmenAsync(
                projekt.Id,
                snapshot.ImportId);

        Assert.Equal(
            B56ProjektmodellUebernahmeStatus.NichtZulaessig,
            ergebnis.Status);
        Assert.Null(
            projekt.QuellSnapshotId);
        Assert.Empty(
            projekt.Alternativen);
    }

    private static B56ImportEintrag ErzeugeSnapshot(
        Guid projektId,
        B56SnapshotStatus status)
    {
        return new B56ImportEintrag
        {
            ImportId = Guid.NewGuid(),
            ProjektId = projektId,
            Projektname = "Rathaus",
            Originaldateiname = "rathaus.xlsx",
            Archivdateipfad = "archiv/rathaus.xlsx",
            Sha256 = new string('a', 64),
            DateigroesseBytes = 1024,
            ImportiertAm = DateTimeOffset.UtcNow,
            Dateiendung = ".xlsx",
            SnapshotStatus = status,
            BestaetigtAm =
                status == B56SnapshotStatus.FachlichBestaetigt
                    ? DateTimeOffset.UtcNow
                    : null
        };
    }

    private static B56ImportPipelineErgebnis ErzeugeFachdaten()
    {
        return new B56ImportPipelineErgebnis
        {
            ImportierteModernisierungsalternativen = 1,
            Modernisierungsalternativen =
            [
                new B56Modernisierungsalternative
                {
                    Bezeichnung = "Fenstertausch",
                    Beschreibung = "Fenster erneuern",
                    Bauteile =
                    [
                        new B56Bauteil
                        {
                            Bauteilcode = "AF01",
                            Bezeichnung = "Fenster",
                            UWert = 0.9
                        }
                    ]
                }
            ]
        };
    }
}
