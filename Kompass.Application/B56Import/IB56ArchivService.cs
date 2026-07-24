namespace Kompass.Application.B56Import;

public interface IB56ArchivService
{
    Task<string> ArchivierenAsync(
        Guid projektId,
        string projektname,
        string quelldateipfad,
        string sha256,
        DateTimeOffset importzeitpunkt,
        CancellationToken cancellationToken = default);
}
