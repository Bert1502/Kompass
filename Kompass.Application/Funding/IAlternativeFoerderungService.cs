using Kompass.Domain.Funding;

namespace Kompass.Application.Funding;

public interface IAlternativeFoerderungService
{
    /// <summary>
    /// Gibt alle zugeordneten Förderprogramme einer Modernisierungsalternative zurück.
    /// </summary>
    Task<IReadOnlyList<Foerderprogramm>> ZugeordneteProgrammeListenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ordnet ein Förderprogramm einer Modernisierungsalternative zu.
    /// Gibt <see langword="false"/> zurück, wenn Alternative oder Programm nicht gefunden wurden,
    /// oder die Zuordnung bereits besteht.
    /// </summary>
    Task<bool> ProgrammZuordnenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid foerderprogrammId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Entfernt die Zuordnung eines Förderprogramms von einer Modernisierungsalternative.
    /// Gibt <see langword="false"/> zurück, wenn die Zuordnung nicht gefunden wurde.
    /// </summary>
    Task<bool> ProgrammEntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid foerderprogrammId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Berechnet die förderfähigen Kosten und Förderhöhen je zugeordnetem Programm
    /// als fachliche Vorprüfung zum angegebenen Stichtag.
    /// Gibt <see langword="null"/> zurück, wenn die Alternative nicht gefunden wurde.
    /// </summary>
    Task<Foerderberechnungsergebnis?> FoerderungBerechnenAsync(
        Guid projektId,
        Guid alternativeId,
        DateOnly stichtag,
        CancellationToken cancellationToken = default);
}
