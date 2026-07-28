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

    Task<bool> LoeschenAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sucht eine Modernisierungsalternative anhand ihrer ID innerhalb eines
    /// bestimmten Projekts. Gibt <c>null</c> zurück, wenn das Projekt oder
    /// die Alternative nicht gefunden wurde.
    /// </summary>
    Task<AlternativeKurzinfo?> AlternativeNachIdAbrufenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default);
}