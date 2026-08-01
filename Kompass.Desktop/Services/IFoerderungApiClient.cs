using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IFoerderungApiClient
{
    Task<IReadOnlyList<FoerderprogrammKatalogDto>> KatalogAbrufenAsync(
        CancellationToken cancellationToken = default);

    Task<FoerderuebersichtBerichtDto?> UebersichtAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);
}
