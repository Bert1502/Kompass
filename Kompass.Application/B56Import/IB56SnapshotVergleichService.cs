namespace Kompass.Application.B56Import;

public interface IB56SnapshotVergleichService
{
    Task<B56SnapshotVergleichErgebnis> VergleichenAsync(
        Guid projektId,
        Guid vorgaengerSnapshotId,
        Guid nachfolgerSnapshotId,
        CancellationToken cancellationToken = default);
}
