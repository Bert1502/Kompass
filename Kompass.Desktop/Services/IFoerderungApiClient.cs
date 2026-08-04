using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IFoerderungApiClient
{
    Task<IReadOnlyList<FoerderprogrammKatalogDto>> KatalogAbrufenAsync(
        CancellationToken cancellationToken = default);

    Task<FoerderuebersichtBerichtDto?> UebersichtAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    Task<FoerdervoraussetzungenDto?> VoraussetzungenAbrufenAsync(Guid projektId, CancellationToken cancellationToken = default);
    Task<FoerdervoraussetzungenDto?> VoraussetzungenSpeichernAsync(Guid projektId, FoerdervoraussetzungenDto voraussetzungen, CancellationToken cancellationToken = default);
    Task<FoerderberechnungDto?> BerechnenAsync(Guid projektId, Guid alternativeId, CancellationToken cancellationToken = default);
}
