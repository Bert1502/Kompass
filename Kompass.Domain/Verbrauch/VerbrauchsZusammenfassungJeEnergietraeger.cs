using Kompass.Domain.Economics;

namespace Kompass.Domain.Verbrauch;

/// <summary>
/// Zusammenfassung aller Verbrauchsdaten eines Projekts je Energieträger.
/// Dient als Eingabehilfe für die Wirtschaftlichkeitsberechnung auf
/// Basis realer Verbräuche (praktische Basis).
/// </summary>
public sealed record VerbrauchsZusammenfassungJeEnergietraeger(
    Energietraeger Energietraeger,
    int AnzahlAbrechnungsperioden,
    decimal GesamtmengeKwh,
    decimal WitterungsbereinigteGesamtmengeKwh,
    decimal JaehrlicheMengeKwh,
    decimal GesamtkostenEur);
