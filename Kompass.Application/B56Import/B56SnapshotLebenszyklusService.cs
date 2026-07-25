namespace Kompass.Application.B56Import;

public sealed class B56SnapshotLebenszyklusService
    : IB56SnapshotLebenszyklusService
{
    private readonly IB56ImportRegister _importRegister;
    private readonly TimeProvider _zeitgeber;

    public B56SnapshotLebenszyklusService(
        IB56ImportRegister importRegister,
        TimeProvider zeitgeber)
    {
        _importRegister = importRegister;
        _zeitgeber = zeitgeber;
    }

    public Task<B56SnapshotAktionErgebnis> BestaetigenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        return StatusAendernAsync(
            projektId,
            importId,
            B56SnapshotStatus.FachlichBestaetigt,
            cancellationToken);
    }

    public Task<B56SnapshotAktionErgebnis> VerwerfenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        return StatusAendernAsync(
            projektId,
            importId,
            B56SnapshotStatus.Verworfen,
            cancellationToken);
    }

    private async Task<B56SnapshotAktionErgebnis> StatusAendernAsync(
        Guid projektId,
        Guid importId,
        B56SnapshotStatus zielstatus,
        CancellationToken cancellationToken)
    {
        var snapshot =
            await _importRegister.NachIdSuchenAsync(
                projektId,
                importId,
                cancellationToken);

        if (snapshot is null)
        {
            return new B56SnapshotAktionErgebnis(
                B56SnapshotAktionStatus.NichtGefunden,
                null,
                "Der B56-Snapshot wurde nicht gefunden.");
        }

        if (!IstUebergangZulaessig(
                snapshot.SnapshotStatus,
                zielstatus))
        {
            return new B56SnapshotAktionErgebnis(
                B56SnapshotAktionStatus.NichtZulaessig,
                snapshot,
                ErzeugeKonfliktnachricht(
                    snapshot.SnapshotStatus,
                    zielstatus));
        }

        var zeitpunkt =
            _zeitgeber.GetUtcNow();

        var aktualisierterSnapshot =
            snapshot with
            {
                SnapshotStatus = zielstatus,
                BestaetigtAm =
                    zielstatus ==
                    B56SnapshotStatus.FachlichBestaetigt
                        ? zeitpunkt
                        : snapshot.BestaetigtAm,
                VerworfenAm =
                    zielstatus ==
                    B56SnapshotStatus.Verworfen
                        ? zeitpunkt
                        : snapshot.VerworfenAm
            };

        await _importRegister.LebenszyklusSpeichernAsync(
            aktualisierterSnapshot,
            cancellationToken);

        return new B56SnapshotAktionErgebnis(
            B56SnapshotAktionStatus.Erfolgreich,
            aktualisierterSnapshot,
            zielstatus ==
            B56SnapshotStatus.FachlichBestaetigt
                ? "Der B56-Snapshot wurde fachlich bestätigt."
                : "Der B56-Snapshot wurde verworfen.");
    }

    private static bool IstUebergangZulaessig(
        B56SnapshotStatus ausgangsstatus,
        B56SnapshotStatus zielstatus)
    {
        return zielstatus switch
        {
            B56SnapshotStatus.FachlichBestaetigt =>
                ausgangsstatus is
                    B56SnapshotStatus.TechnischGeprueft or
                    B56SnapshotStatus.MitWarnungen,

            B56SnapshotStatus.Verworfen =>
                ausgangsstatus is
                    B56SnapshotStatus.TechnischGeprueft or
                    B56SnapshotStatus.MitWarnungen or
                    B56SnapshotStatus.Blockiert,

            _ => false
        };
    }

    private static string ErzeugeKonfliktnachricht(
        B56SnapshotStatus ausgangsstatus,
        B56SnapshotStatus zielstatus)
    {
        if (ausgangsstatus == B56SnapshotStatus.Blockiert &&
            zielstatus ==
            B56SnapshotStatus.FachlichBestaetigt)
        {
            return "Ein blockierter B56-Snapshot kann nicht fachlich bestätigt werden.";
        }

        return
            $"Der Statuswechsel von '{ausgangsstatus}' nach '{zielstatus}' ist nicht zulässig.";
    }
}
