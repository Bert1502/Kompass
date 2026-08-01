using Kompass.Domain.Verbrauch;

namespace Kompass.Application.Verbrauch;

public interface IVerbrauchsDatenService
{
    /// <summary>
    /// Gibt alle Verbrauchsdatensätze eines Projekts zurück, aufsteigend nach Periodenanfang sortiert.
    /// </summary>
    Task<IReadOnlyList<VerbrauchsDaten>> ListenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gibt einen einzelnen Verbrauchsdatensatz zurück.
    /// Gibt <see langword="null"/> zurück, wenn der Datensatz im Projekt nicht gefunden wurde.
    /// </summary>
    Task<VerbrauchsDaten?> AbrufenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Legt einen neuen Verbrauchsdatensatz an.
    /// Gibt <see langword="null"/> zurück, wenn das Projekt nicht gefunden wurde.
    /// </summary>
    Task<VerbrauchsDaten?> AnlegenAsync(
        VerbrauchsDaten verbrauchsDaten,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualisiert einen bestehenden Verbrauchsdatensatz.
    /// Gibt <see langword="false"/> zurück, wenn der Datensatz im Projekt nicht gefunden wurde.
    /// </summary>
    Task<bool> AktualisierenAsync(
        VerbrauchsDaten verbrauchsDaten,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Löscht einen Verbrauchsdatensatz.
    /// Gibt <see langword="false"/> zurück, wenn der Datensatz im Projekt nicht gefunden wurde.
    /// </summary>
    Task<bool> LoeschenAsync(
        Guid projektId,
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gibt eine Zusammenfassung der Verbrauchsdaten je Energieträger zurück.
    /// Liefert <see langword="null"/>, wenn das Projekt nicht gefunden wurde.
    /// Ist kein Verbrauchsdatensatz vorhanden, wird eine leere Liste zurückgegeben.
    /// </summary>
    Task<IReadOnlyList<VerbrauchsZusammenfassungJeEnergietraeger>?> ZusammenfassenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);
}
