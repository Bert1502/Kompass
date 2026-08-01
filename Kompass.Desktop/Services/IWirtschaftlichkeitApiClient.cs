using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IWirtschaftlichkeitApiClient
{
    Task<WirtschaftlichkeitsberichtDto?> BerichtAbrufenAsync(
        Guid projektId,
        string basis,
        CancellationToken cancellationToken = default);
}
