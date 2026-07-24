namespace Kompass.Application.B56Import;

/// <summary>
/// Führt den fachlichen Import einer bereits geöffneten
/// B56-Arbeitsmappe durch.
/// </summary>
public interface IB56ImportPipeline
{
    Task<B56ImportPipelineErgebnis> ImportierenAsync(
        B56ImportKontext kontext,
        CancellationToken cancellationToken = default);
}
