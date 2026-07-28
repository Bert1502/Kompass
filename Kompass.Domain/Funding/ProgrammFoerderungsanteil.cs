namespace Kompass.Domain.Funding;

/// <summary>
/// Berechneter Förderanteil für ein einzelnes Förderprogramm.
/// </summary>
public sealed record ProgrammFoerderungsanteil(
    Guid FoerderprogrammId,
    string Programmkennung,
    int Version,
    decimal Foerderbetrag,
    KumulierbarkeitStatus Kumulierbarkeit);
