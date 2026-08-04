using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IKostenApiClient
{
    Task<IReadOnlyList<KostenAlternativeDto>> AlternativenAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KostenpositionDto>> PositionenAbrufenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default);

    Task<KostenpositionDto> HinzufuegenAsync(
        Guid projektId,
        Guid alternativeId,
        KostenpositionErstellenDto position,
        CancellationToken cancellationToken = default);

    Task<bool> EntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid kostenpositionId,
        CancellationToken cancellationToken = default);
}
