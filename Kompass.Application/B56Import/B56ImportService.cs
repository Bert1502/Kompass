namespace Kompass.Application.B56Import;

/// <summary>
/// Führt einen vollständigen Import einer B56-Arbeitsmappe aus.
/// </summary>
public interface IB56ImportService
{
    Task<B56ImportErgebnis> ImportierenAsync(
        Guid projektId,
        string projektname,
        string dateipfad,
        CancellationToken cancellationToken = default);
}