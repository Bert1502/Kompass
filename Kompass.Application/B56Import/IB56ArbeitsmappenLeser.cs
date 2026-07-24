namespace Kompass.Application.B56Import;

public interface IB56ArbeitsmappenLeser
{
    Task<B56Arbeitsmappe> LesenAsync(
        string dateipfad,
        CancellationToken cancellationToken = default);
}
