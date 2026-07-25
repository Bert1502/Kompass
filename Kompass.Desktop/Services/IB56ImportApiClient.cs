using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IB56ImportApiClient
{
    Task<IReadOnlyList<B56ImportHistorieDto>> HistorieAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    Task<B56ImportPipelineAntwortDto> DetailsAbrufenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<B56ImportAntwortDto> ImportierenAsync(
        Guid projektId,
        string dateipfad,
        CancellationToken cancellationToken = default);

    Task<B56SnapshotAktionAntwortDto> BestaetigenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<B56SnapshotAktionAntwortDto> VerwerfenAsync(
        Guid projektId,
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<B56ProjektmodellUebernahmeAntwortDto>
        InProjektmodellUebernehmenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default);
}
