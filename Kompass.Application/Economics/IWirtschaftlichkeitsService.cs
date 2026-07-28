using Kompass.Domain.Economics;

namespace Kompass.Application.Economics;

public interface IWirtschaftlichkeitsService
{
    Task<Wirtschaftlichkeitsannahmen?> AnnahmenAbrufenAsync(
        Guid projektId,
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken = default);

    Task<Wirtschaftlichkeitsannahmen> AnnahmenSpeichernAsync(
        Wirtschaftlichkeitsannahmen annahmen,
        CancellationToken cancellationToken = default);

    Task<Wirtschaftlichkeitsergebnis?> BerechnenAsync(
        Guid projektId,
        Guid alternativeId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken = default);
}
