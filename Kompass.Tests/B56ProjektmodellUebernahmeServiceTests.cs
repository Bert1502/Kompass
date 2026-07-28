using Kompass.Application.B56Import;
using Kompass.Domain.Economics;
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
        Assert.Equal(
            1,
            alternative.B56Position);
        Assert.True(
            alternative.IstImAktuellenB56SnapshotVorhanden);
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

    [Fact]
    public async Task Neuer_Snapshot_kennzeichnet_fehlende_Position_und_erhaelt_Kosten()
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
        var service =
            new B56ProjektmodellUebernahmeService(
                testdatenbank.Context,
                register);

        var ersterSnapshot =
            ErzeugeSnapshot(
                projekt.Id,
                B56SnapshotStatus.FachlichBestaetigt);

        await register.EintragMitFachdatenSpeichernAsync(
            ersterSnapshot,
            ErzeugeFachdaten(
                (1, "Fenster alt"),
                (2, "Dach")));
        await service.UebernehmenAsync(
            projekt.Id,
            ersterSnapshot.ImportId);

        var dach =
            projekt.Alternativen.Single(
                alternative =>
                    alternative.B56Position == 2);

        dach.KostenpositionHinzufuegen(
            new Kostenposition(
                Guid.NewGuid(),
                "Planung",
                500m,
                Kostenart.Fachplanung));
        await testdatenbank.Context.SaveChangesAsync();

        var zweiterSnapshot =
            ErzeugeSnapshot(
                projekt.Id,
                B56SnapshotStatus.FachlichBestaetigt);

        await register.EintragMitFachdatenSpeichernAsync(
            zweiterSnapshot,
            ErzeugeFachdaten(
                (1, "Fenster neu"),
                (3, "Heizung")));

        var ergebnis =
            await service.UebernehmenAsync(
                projekt.Id,
                zweiterSnapshot.ImportId);

        testdatenbank.Context.ChangeTracker.Clear();

        var aktualisiertesProjekt =
            await testdatenbank.Context.Projekte
                .Include(
                    eintrag =>
                        eintrag.Alternativen)
                    .ThenInclude(
                        alternative =>
                            alternative.Kostenpositionen)
                .SingleAsync(
                    eintrag =>
                        eintrag.Id == projekt.Id);

        var alternativenNachPosition =
            aktualisiertesProjekt.Alternativen
                .ToDictionary(
                    alternative =>
                        alternative.B56Position!.Value);

        Assert.Equal(
            B56ProjektmodellUebernahmeStatus.Erfolgreich,
            ergebnis.Status);
        Assert.Equal(
            2,
            aktualisiertesProjekt.ProjektmodellVersion);
        Assert.Equal(
            zweiterSnapshot.ImportId,
            aktualisiertesProjekt.QuellSnapshotId);
        Assert.Equal(
            "Fenster neu",
            alternativenNachPosition[1].Bezeichnung);
        Assert.True(
            alternativenNachPosition[1]
                .IstImAktuellenB56SnapshotVorhanden);
        Assert.False(
            alternativenNachPosition[2]
                .IstImAktuellenB56SnapshotVorhanden);
        Assert.Equal(
            500m,
            alternativenNachPosition[2].Gesamtkosten);
        Assert.True(
            alternativenNachPosition[3]
                .IstImAktuellenB56SnapshotVorhanden);
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

    private static B56ImportPipelineErgebnis ErzeugeFachdaten(
        params (int Position, string Bezeichnung)[] alternativen)
    {
        if (alternativen.Length == 0)
        {
            alternativen =
                [(1, "Fenstertausch")];
        }

        return new B56ImportPipelineErgebnis
        {
            ImportierteModernisierungsalternativen =
                alternativen.Length,
            Modernisierungsalternativen =
                alternativen
                    .Select(
                        alternative =>
                            new B56Modernisierungsalternative
                            {
                                Position =
                                    alternative.Position,
                                Bezeichnung =
                                    alternative.Bezeichnung,
                                Beschreibung =
                                    $"{alternative.Bezeichnung} erneuern",
                                Bauteile =
                                [
                                    new B56Bauteil
                                    {
                                        Bauteilcode = "AF01",
                                        Bezeichnung = "Fenster",
                                        UWert = 0.9
                                    }
                                ]
                            })
                    .ToArray()
        };
    }
}
