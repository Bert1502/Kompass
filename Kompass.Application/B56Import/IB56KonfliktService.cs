namespace Kompass.Application.B56Import;

public interface IB56KonfliktService
{
    /// <summary>
    /// Listet alle Konflikteinträge für einen Snapshot-Vergleich.
    /// Initialisiert die Einträge automatisch aus dem gespeicherten
    /// Vergleich, wenn sie noch nicht vorhanden sind.
    /// </summary>
    Task<IReadOnlyList<B56KonfliktEintrag>> ListenOderErzeugenAsync(
        Guid projektId,
        Guid vorgaengerImportId,
        Guid nachfolgerImportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Setzt die Entscheidung für einen einzelnen Konflikteintrag
    /// (Uebernehmen oder Behalten).
    /// Gibt false zurück, wenn der Eintrag nicht gefunden wurde.
    /// </summary>
    Task<bool> EntscheidungSetzenAsync(
        Guid projektId,
        Guid nachfolgerImportId,
        Guid id,
        B56KonfliktEntscheidungsTyp entscheidung,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Setzt alle noch offenen Konflikte eines Vergleichs auf
    /// Uebernehmen. Gibt die Anzahl der aktualisierten Einträge zurück.
    /// </summary>
    Task<int> AlleOffenenUebernehmenAsync(
        Guid projektId,
        Guid vorgaengerImportId,
        Guid nachfolgerImportId,
        CancellationToken cancellationToken = default);
}
