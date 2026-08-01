namespace Kompass.Domain.Reports;

/// <summary>
/// Bericht "Verbrauchsvergleich" gemäß Fachspezifikation Abschnitt 18.
/// Listet reale Verbrauchsdaten des Projekts auf und stellt sie den
/// B56-Bilanzwerten gegenüber.
/// </summary>
public sealed record VerbrauchsvergleichBericht(
    Berichtskopf Kopf,
    IReadOnlyList<VerbrauchsvergleichZeile> Zeilen);
