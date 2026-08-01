using Kompass.Domain.Economics;
using Kompass.Domain.Reports;

namespace Kompass.Application.Reports;

/// <summary>
/// Erzeugt projektbezogene Berichte aus dem KOMPASS-Projektmodell.
/// Gemäß ADR-0007 gibt es keine separate Berichtsdatenhaltung;
/// alle Ausgaben entstehen direkt aus dem Domänenmodell.
/// </summary>
public interface IBerichtsService
{
    /// <summary>
    /// Erzeugt den Alternativenvergleich-Bericht für das angegebene Projekt.
    /// Gibt <see langword="null"/> zurück, wenn das Projekt nicht gefunden wurde.
    /// </summary>
    Task<AlternativenvergleichBericht?> AlternativenvergleichErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Erzeugt die Wärmebrückenübersicht für das angegebene Projekt.
    /// Gibt <see langword="null"/> zurück, wenn das Projekt nicht gefunden wurde.
    /// </summary>
    Task<WaermebrueckenuebersichtBericht?> WaermebrueckenuebersichtErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Erzeugt den Wirtschaftlichkeitsbericht für das angegebene Projekt und die
    /// angegebene Berechnungsbasis. Alternativen ohne hinterlegte Annahmen werden
    /// ausgelassen. Gibt <see langword="null"/> zurück, wenn das Projekt nicht
    /// gefunden wurde.
    /// </summary>
    Task<WirtschaftlichkeitsberichtBericht?> WirtschaftlichkeitsberichtErzeugenAsync(
        Guid projektId,
        WirtschaftlichkeitsBasis basis,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Erzeugt die Förderübersicht für das angegebene Projekt.
    /// Gibt <see langword="null"/> zurück, wenn das Projekt nicht gefunden wurde.
    /// </summary>
    Task<FoerderuebersichtBericht?> FoerderuebersichtErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Erzeugt den Verbrauchsvergleichsbericht für das angegebene Projekt.
    /// Stellt reale Verbrauchsdaten den B56-Bilanzwerten gegenüber.
    /// Gibt <see langword="null"/> zurück, wenn das Projekt nicht gefunden wurde.
    /// </summary>
    Task<VerbrauchsvergleichBericht?> VerbrauchsvergleichErzeugenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default);
}
