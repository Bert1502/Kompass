using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IB56ImportApiClient
{
    Task<IReadOnlyList<B56ImportHistorieDto>> HistorieAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    Task<B56ImportAntwortDto> ImportierenAsync(
        Guid projektId,
        string dateipfad,
        CancellationToken cancellationToken = default);
}
