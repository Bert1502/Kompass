using Kompass.Application.B56Import;

namespace Kompass.Tests.B56Import;

public sealed class B56SnapshotLebenszyklusServiceTests
{
    private static readonly DateTimeOffset FesterZeitpunkt =
        DateTimeOffset.Parse(
            "2026-07-25T11:00:00+02:00");

    [Theory]
    [InlineData(B56SnapshotStatus.TechnischGeprueft)]
    [InlineData(B56SnapshotStatus.MitWarnungen)]
    public async Task Pruefbarer_Snapshot_kann_bestaetigt_werden(
        B56SnapshotStatus ausgangsstatus)
    {
        var snapshot =
            ErzeugeSnapshot(
                ausgangsstatus);

        var register =
            new ImportRegisterFake(
                snapshot);

        var service =
            new B56SnapshotLebenszyklusService(
                register,
                new FesterZeitgeber(
                    FesterZeitpunkt));

        var ergebnis =
            await service.BestaetigenAsync(
                snapshot.ProjektId,
                snapshot.ImportId);

        Assert.Equal(
            B56SnapshotAktionStatus.Erfolgreich,
            ergebnis.Status);
        Assert.Equal(
            B56SnapshotStatus.FachlichBestaetigt,
            ergebnis.Snapshot?.SnapshotStatus);
        Assert.Equal(
            FesterZeitpunkt.ToUniversalTime(),
            ergebnis.Snapshot?.BestaetigtAm);
        Assert.Equal(
            ergebnis.Snapshot,
            register.GespeicherterSnapshot);
    }

    [Fact]
    public async Task Blockierter_Snapshot_kann_nicht_bestaetigt_werden()
    {
        var snapshot =
            ErzeugeSnapshot(
                B56SnapshotStatus.Blockiert);

        var register =
            new ImportRegisterFake(
                snapshot);

        var service =
            new B56SnapshotLebenszyklusService(
                register,
                new FesterZeitgeber(
                    FesterZeitpunkt));

        var ergebnis =
            await service.BestaetigenAsync(
                snapshot.ProjektId,
                snapshot.ImportId);

        Assert.Equal(
            B56SnapshotAktionStatus.NichtZulaessig,
            ergebnis.Status);
        Assert.Null(
            register.GespeicherterSnapshot);
    }

    [Fact]
    public async Task Blockierter_Snapshot_kann_verworfen_werden()
    {
        var snapshot =
            ErzeugeSnapshot(
                B56SnapshotStatus.Blockiert);

        var register =
            new ImportRegisterFake(
                snapshot);

        var service =
            new B56SnapshotLebenszyklusService(
                register,
                new FesterZeitgeber(
                    FesterZeitpunkt));

        var ergebnis =
            await service.VerwerfenAsync(
                snapshot.ProjektId,
                snapshot.ImportId);

        Assert.Equal(
            B56SnapshotAktionStatus.Erfolgreich,
            ergebnis.Status);
        Assert.Equal(
            B56SnapshotStatus.Verworfen,
            ergebnis.Snapshot?.SnapshotStatus);
        Assert.Equal(
            FesterZeitpunkt.ToUniversalTime(),
            ergebnis.Snapshot?.VerworfenAm);
    }

    [Fact]
    public async Task Unbekannter_Snapshot_liefert_NichtGefunden()
    {
        var register =
            new ImportRegisterFake(
                null);

        var service =
            new B56SnapshotLebenszyklusService(
                register,
                new FesterZeitgeber(
                    FesterZeitpunkt));

        var ergebnis =
            await service.BestaetigenAsync(
                Guid.NewGuid(),
                Guid.NewGuid());

        Assert.Equal(
            B56SnapshotAktionStatus.NichtGefunden,
            ergebnis.Status);
        Assert.Null(
            register.GespeicherterSnapshot);
    }

    private static B56ImportEintrag ErzeugeSnapshot(
        B56SnapshotStatus status)
    {
        return new B56ImportEintrag
        {
            ImportId = Guid.NewGuid(),
            ProjektId = Guid.NewGuid(),
            Projektname = "Testprojekt",
            Originaldateiname = "b56.xlsx",
            Archivdateipfad = "archiv/b56.xlsx",
            Sha256 = new string('a', 64),
            DateigroesseBytes = 1024,
            ImportiertAm = FesterZeitpunkt.AddHours(-1),
            Dateiendung = ".xlsx",
            SnapshotStatus = status
        };
    }

    private sealed class FesterZeitgeber(
        DateTimeOffset zeitpunkt)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return zeitpunkt.ToUniversalTime();
        }
    }

    private sealed class ImportRegisterFake(
        B56ImportEintrag? snapshot)
        : IB56ImportRegister
    {
        public B56ImportEintrag? GespeicherterSnapshot { get; private set; }

        public Task<B56ImportEintrag?> NachIdSuchenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                snapshot is not null &&
                snapshot.ProjektId == projektId &&
                snapshot.ImportId == importId
                    ? snapshot
                    : null);
        }

        public Task LebenszyklusSpeichernAsync(
            B56ImportEintrag eintrag,
            CancellationToken cancellationToken = default)
        {
            GespeicherterSnapshot =
                eintrag;

            return Task.CompletedTask;
        }

        public Task<B56ImportEintrag?> NachHashSuchenAsync(
            Guid projektId,
            string sha256,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task EintragSpeichernAsync(
            B56ImportEintrag eintrag,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task EintragMitFachdatenSpeichernAsync(
            B56ImportEintrag eintrag,
            B56ImportPipelineErgebnis fachdaten,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<B56ImportPipelineErgebnis?> FachdatenAbrufenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<B56ImportEintrag>>
            AlleFuerProjektAbrufenAsync(
                Guid projektId,
                CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
