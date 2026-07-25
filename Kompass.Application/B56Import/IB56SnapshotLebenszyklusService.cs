namespace Kompass.Application.B56Import;

public interface IB56SnapshotLebenszyklusService
{
    Task<B56SnapshotAktionErgebnis> BestaetigenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<B56SnapshotAktionErgebnis> VerwerfenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);
}
