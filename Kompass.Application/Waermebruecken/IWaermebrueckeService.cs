using Kompass.Domain.Waermebruecken;

namespace Kompass.Application.Waermebruecken;

public interface IWaermebrueckeService
{
    /// <summary>
    /// Gibt alle Wärmebrücken eines Projekts zurück.
    /// </summary>
    Task<IReadOnlyList<Waermebruecke>> ListenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gibt eine einzelne Wärmebrücke zurück.
    /// Gibt <see langword="null"/> zurück, wenn die Wärmebrücke im Projekt nicht gefunden wurde.
    /// </summary>
    Task<Waermebruecke?> AbrufenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Legt eine neue Wärmebrücke an.
    /// Gibt <see langword="null"/> zurück, wenn das Projekt nicht gefunden wurde.
    /// Wirft <see cref="Domain.Common.DomainException"/> wenn die interne Nummer im Projekt bereits vergeben ist.
    /// </summary>
    Task<Waermebruecke?> AnlegenAsync(
        Waermebruecke waermebruecke,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualisiert eine bestehende Wärmebrücke.
    /// Gibt <see langword="false"/> zurück, wenn die Wärmebrücke im Projekt nicht gefunden wurde.
    /// </summary>
    Task<bool> AktualisierenAsync(
        Waermebruecke waermebruecke,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Löscht eine Wärmebrücke.
    /// Gibt <see langword="false"/> zurück, wenn die Wärmebrücke im Projekt nicht gefunden wurde.
    /// </summary>
    Task<bool> LoeschenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default);
}
