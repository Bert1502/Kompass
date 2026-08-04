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

    Task<ProjektUebersicht?> StammdatenAktualisierenAsync(
        Guid id,
        string? auftraggeber,
        string? ansprechpartner,
        string? strasse,
        string? ort,
        string? postleitzahl,
        string? gebaeudeart,
        CancellationToken cancellationToken = default);

    Task<ProjektUebersicht?> FreigabestatusAktualisierenAsync(
        Guid id,
        Freigabestatus status,
        CancellationToken cancellationToken = default);

    Task<ProjektUebersicht?> NotizenAktualisierenAsync(
        Guid id,
        string? notizen,
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

    Task<IReadOnlyList<AlternativeKurzinfo>> AlternativenAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);
}
