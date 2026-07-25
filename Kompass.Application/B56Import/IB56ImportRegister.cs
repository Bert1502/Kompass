namespace Kompass.Application.B56Import;

public interface IB56ImportRegister
{
    Task<B56ImportEintrag?> NachHashSuchenAsync(
        Guid projektId,
        string sha256,
        CancellationToken cancellationToken = default);

    Task<B56ImportEintrag?> NachIdSuchenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);

    Task EintragSpeichernAsync(
        B56ImportEintrag eintrag,
        CancellationToken cancellationToken = default);

    Task EintragMitFachdatenSpeichernAsync(
        B56ImportEintrag eintrag,
        B56ImportPipelineErgebnis fachdaten,
        CancellationToken cancellationToken = default);

    Task<B56ImportPipelineErgebnis?> FachdatenAbrufenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);

    Task LebenszyklusSpeichernAsync(
        B56ImportEintrag eintrag,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<B56ImportEintrag>>
        AlleFuerProjektAbrufenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default);
}
