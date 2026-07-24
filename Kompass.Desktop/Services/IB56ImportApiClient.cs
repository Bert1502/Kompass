using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IB56ImportApiClient
{
    Task<B56ImportAntwortDto> ImportierenAsync(
        Guid projektId,
        string dateipfad,
        CancellationToken cancellationToken = default);
}
