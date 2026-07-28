using Kompass.Domain.Economics;

namespace Kompass.Application.Economics;

public interface IKostenpositionService
{
    Task<IReadOnlyList<Kostenposition>> ListenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default);

    Task<Kostenposition?> HinzufuegenAsync(
        Guid projektId,
        Guid alternativeId,
        Kostenposition kostenposition,
        CancellationToken cancellationToken = default);

    Task<bool> EntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid kostenpositionId,
        CancellationToken cancellationToken = default);
}
