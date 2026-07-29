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
}
