using Kompass.Desktop.Models;

namespace Kompass.Desktop.Services;

public interface IProjektApiClient
{
    Task<IReadOnlyList<ProjektUebersichtDto>>
        AlleAbrufenAsync(
            CancellationToken cancellationToken = default);

    Task<ProjektUebersichtDto?>
        NachIdAbrufenAsync(
            Guid id,
            CancellationToken cancellationToken = default);

    Task<ProjektUebersichtDto>
        ErstellenAsync(
            string name,
            CancellationToken cancellationToken = default);

    Task<ProjektUebersichtDto?>
        AktualisierenAsync(
            Guid id,
            string name,
            CancellationToken cancellationToken = default);

    Task<bool>
        LoeschenAsync(
            Guid id,
            CancellationToken cancellationToken = default);
}