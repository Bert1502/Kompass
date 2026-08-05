namespace Kompass.Domain.Funding;

/// <summary>
/// Berechneter Förderanteil für ein einzelnes Förderprogramm.
/// </summary>
public sealed record ProgrammFoerderungsanteil(
    Guid FoerderprogrammId,
    string Programmkennung,
    int Version,
    decimal Foerderbetrag,
    KumulierbarkeitStatus Kumulierbarkeit,
    Foerderpruefstatus Status = Foerderpruefstatus.NichtGeprueft,
    decimal FoerderfaehigeKosten = 0m,
    decimal Foerderhoechstbetrag = 0m,
    decimal Grundfoerderquote = 0m,
    decimal ISfpBonusquote = 0m,
    decimal WpbBonusquote = 0m,
    decimal Grundfoerderung = 0m,
    decimal ISfpBonus = 0m,
    decimal WpbBonus = 0m,
    decimal Eigenanteil = 0m,
    IReadOnlyList<string>? FehlendeVoraussetzungen = null,
    IReadOnlyList<string>? Ausschlussgruende = null);
