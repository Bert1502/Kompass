namespace Kompass.Application.B56Import;

public interface IB56KonfliktService
{
    /// <summary>
    /// Listet alle Konflikteinträge für einen Snapshot-Vergleich.
    /// Initialisiert die Einträge automatisch aus dem gespeicherten
    /// Vergleich, wenn sie noch nicht vorhanden sind.
    /// </summary>
    Task<IReadOnlyList<B56KonfliktEintrag>> ListenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Setzt die Entscheidung für einen einzelnen Konflikteintrag
    /// (Akzeptiert oder Abgelehnt).
    /// Gibt null zurück, wenn der Eintrag nicht gefunden wurde.
    /// </summary>
    Task<B56KonfliktEintrag?> EntscheidenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        Guid konfliktId,
        B56KonfliktEntscheidungsTyp entscheidung,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Setzt alle noch ausstehenden Konflikte eines Vergleichs auf
    /// Akzeptiert. Gibt die Anzahl der aktualisierten Einträge zurück.
    /// </summary>
    Task<int> AlleAusstehendAkzeptierenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default);
}
