namespace Kompass.Application.B56Import;

public interface IB56HashService
{
    Task<string> BerechnenAsync(
        string dateipfad,
        CancellationToken cancellationToken = default);
}
