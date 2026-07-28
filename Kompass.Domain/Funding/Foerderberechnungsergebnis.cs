namespace Kompass.Domain.Funding;

/// <summary>
/// Ergebnis der fachlichen Förder-Vorprüfung für eine Modernisierungsalternative.
/// KOMPASS gibt keine Förderzusage, sondern eine fachliche Vorprüfung.
/// </summary>
public sealed record Foerderberechnungsergebnis(
    DateOnly Stichtag,
    decimal Investitionskosten,
    IReadOnlyList<ProgrammFoerderungsanteil> Programmfoerderungen,
    decimal GesamtFoerderung,
    decimal Eigenanteil);
