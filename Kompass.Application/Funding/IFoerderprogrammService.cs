using Kompass.Domain.Funding;

namespace Kompass.Application.Funding;

public interface IFoerderprogrammService
{
    Task<IReadOnlyList<Foerderprogramm>> ListenAsync(
        CancellationToken cancellationToken = default);

    Task<Foerderprogramm> AnlegenAsync(
        Foerderprogramm foerderprogramm,
        CancellationToken cancellationToken = default);
}
