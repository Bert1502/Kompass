using System.Text;
using Kompass.Persistence.B56Import;

namespace Kompass.Tests.B56Import;

public sealed class Sha256HashServiceTests
{
    private readonly Sha256HashService _hashService =
        new();

    [Fact]
    public async Task Berechnet_bekannten_SHA256_Hash_in_Kleinbuchstaben()
    {
        var dateipfad =
            ErzeugeTemporareDatei(
                Encoding.UTF8.GetBytes(
                    "abc"));

        try
        {
            var hash =
                await _hashService.BerechnenAsync(
                    dateipfad);

            Assert.Equal(
                "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
                hash);

            Assert.Equal(
                64,
                hash.Length);
        }
        finally
        {
            File.Delete(
                dateipfad);
        }
    }

    [Fact]
    public async Task Bereits_abgebrochene_Berechnung_wird_weitergegeben()
    {
        var dateipfad =
            ErzeugeTemporareDatei(
                [0x01, 0x02, 0x03]);

        try
        {
            using var cancellationTokenSource =
                new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () =>
                    _hashService.BerechnenAsync(
                        dateipfad,
                        cancellationTokenSource.Token));
        }
        finally
        {
            File.Delete(
                dateipfad);
        }
    }

    private static string ErzeugeTemporareDatei(
        byte[] inhalt)
    {
        var dateipfad =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-hash-{Guid.NewGuid():N}.xlsx");

        File.WriteAllBytes(
            dateipfad,
            inhalt);

        return dateipfad;
    }
}
