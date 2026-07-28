using Kompass.Domain.Economics;

namespace Kompass.Application.Economics;

/// <summary>
/// Datenzugriff für Wirtschaftlichkeitsannahmen je Modernisierungsalternative.
/// </summary>
public interface IWirtschaftlichkeitsannahmenRepository
{
    /// <summary>
    /// Lädt die Wirtschaftlichkeitsannahmen für die angegebene
    /// Modernisierungsalternative. Gibt <c>null</c> zurück, wenn noch keine
    /// Annahmen hinterlegt wurden.
    /// </summary>
    Task<Wirtschaftlichkeitsannahmen?> NachAlternativeIdAbrufenAsync(
        Guid alternativeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Erstellt oder ersetzt die Wirtschaftlichkeitsannahmen für die
    /// angegebene Modernisierungsalternative.
    /// </summary>
    Task<Wirtschaftlichkeitsannahmen> SpeichernAsync(
        Guid alternativeId,
        Wirtschaftlichkeitsannahmen annahmen,
        CancellationToken cancellationToken = default);
}
