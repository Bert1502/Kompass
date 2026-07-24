using Kompass.Application.B56Import;
using System.Security.Cryptography;

namespace Kompass.Persistence.B56Import;

public sealed class Sha256HashService : IB56HashService
{
    public async Task<string> BerechnenAsync(
        string dateipfad,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            dateipfad);

        await using var stream =
            new FileStream(
                dateipfad,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize: 1024 * 128,
                useAsync: true);

        using var sha256 =
            SHA256.Create();

        var hash =
            await sha256.ComputeHashAsync(
                stream,
                cancellationToken);

        return Convert
            .ToHexString(hash)
            .ToLowerInvariant();
    }
}
