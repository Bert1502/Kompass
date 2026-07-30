namespace Kompass.Application.B56Import;

public interface IB56KonfliktService
{
    Task<IReadOnlyList<B56KonfliktEintrag>> ListenOderErzeugenAsync(
        Guid projektId,
        Guid vorgaengerImportId,
        Guid nachfolgerImportId,
        CancellationToken cancellationToken = default);

    Task<bool> EntscheidungSetzenAsync(
        Guid projektId,
        Guid nachfolgerImportId,
        Guid id,
        B56KonfliktEntscheidungsTyp entscheidung,
        CancellationToken cancellationToken = default);
}
