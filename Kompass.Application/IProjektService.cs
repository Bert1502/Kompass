using Kompass.Domain.Projects;

namespace Kompass.Application.Projects;

public interface IProjektService
{
    Task<IReadOnlyList<ProjektUebersicht>> AlleAbrufenAsync(
        CancellationToken cancellationToken = default);

    Task<ProjektUebersicht?> NachIdAbrufenAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProjektUebersicht> ErstellenAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<ProjektUebersicht?> AktualisierenAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken = default);

    Task<ProjektUebersicht?> ProjektdatenAktualisierenAsync(
        Guid id,
        string? interneBezeichnung,
        Bearbeitungsstatus bearbeitungsstatus,
        CancellationToken cancellationToken = default);

    Task<bool> LoeschenAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}