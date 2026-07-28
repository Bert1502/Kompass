namespace Kompass.Application.B56Import;

public interface IB56SnapshotVergleichService
{
<<<<<<< HEAD
    Task<B56SnapshotVergleichErgebnis> VergleichenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default);
}
=======
    /// <summary>
    /// Vergleicht zwei B56-Snapshots desselben Projekts
    /// und liefert die Unterschiede je Alternative,
    /// Bestandskennwert und Bauteil.
    /// </summary>
    Task<B56SnapshotVergleichAktionErgebnis> VergleichenAsync(
        Guid projektId,
        Guid altSnapshotId,
        Guid neuSnapshotId,
        CancellationToken cancellationToken = default);
}

public enum B56SnapshotVergleichStatus
{
    Erfolgreich = 0,
    NichtGefunden = 1
}

public sealed record B56SnapshotVergleichAktionErgebnis(
    B56SnapshotVergleichStatus Status,
    B56SnapshotVergleichErgebnis? Ergebnis,
    string Nachricht);
>>>>>>> origin/main
