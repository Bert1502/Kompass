namespace Kompass.Application.B56Import;

/// <summary>
/// Importiert sämtliche Tabellen einer B56-Arbeitsmappe.
/// </summary>
public interface IB56TabellenImportService
{
    Task<B56TabellenImportErgebnis> ImportierenAsync(
        B56ImportKontext kontext,
        CancellationToken cancellationToken = default);
}
