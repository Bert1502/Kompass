namespace Kompass.Application.B56Import;

public interface IB56ImportService
{
    Task<B56ImportErgebnis> ImportierenAsync(
        B56ImportAnfrage anfrage,
        CancellationToken cancellationToken = default);
}
