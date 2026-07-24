namespace Kompass.Application.B56Import;

public interface IB56ImportRegister
{
    Task<B56ImportEintrag?> NachHashSuchenAsync(
        Guid projektId,
        string sha256,
        CancellationToken cancellationToken = default);

    Task EintragSpeichernAsync(
        B56ImportEintrag eintrag,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<B56ImportEintrag>>
        AlleFuerProjektAbrufenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default);
}
