namespace Kompass.Domain.Economics;

/// <summary>
/// Energieträger für eine Modernisierungsalternative.
/// </summary>
public enum Energietraeger
{
    Unbekannt = 0,
    Erdgas = 1,
    Heizoel = 2,
    Fernwaerme = 3,
    Strom = 4,
    Holzpellets = 5,
    Holzhackschnitzel = 6,
    Waermepumpe = 7,
    Sonstige = 8
}
