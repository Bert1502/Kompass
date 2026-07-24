using Kompass.Application.B56Import;
using Kompass.Persistence.Services;

namespace Kompass.Tests.B56Import;

public sealed class B56ArchivServiceTests
{
    [Fact]
    public async Task Archiviert_Datei_bytegleich_im_Projektordner()
    {
        var testverzeichnis =
            ErzeugeTestverzeichnis();

        try
        {
            var quelldatei =
                Path.Combine(
                    testverzeichnis,
                    "b56-test.xlsx");

            var dateiinhalt =
                new byte[]
                {
                    0x50,
                    0x4B,
                    0x03,
                    0x04,
                    0x01,
                    0x02
                };

            await File.WriteAllBytesAsync(
                quelldatei,
                dateiinhalt);

            var archivBasisverzeichnis =
                Path.Combine(
                    testverzeichnis,
                    "archiv");

            var service =
                new B56ArchivService(
                    new B56ImportOptionen
                    {
                        ArchivBasisverzeichnis =
                            archivBasisverzeichnis
                    });

            var projektId =
                Guid.NewGuid();

            var archivdatei =
                await service.ArchivierenAsync(
                    projektId,
                    "Testprojekt",
                    quelldatei,
                    "1234567890abcdef",
                    new DateTimeOffset(
                        2026,
                        7,
                        24,
                        18,
                        30,
                        45,
                        TimeSpan.Zero));

            Assert.Equal(
                Path.Combine(
                    archivBasisverzeichnis,
                    projektId.ToString("N"),
                    "Testprojekt",
                    "20260724_183045_b56-test_12345678.xlsx"),
                archivdatei);

            Assert.Equal(
                dateiinhalt,
                await File.ReadAllBytesAsync(
                    archivdatei));
        }
        finally
        {
            Directory.Delete(
                testverzeichnis,
                recursive: true);
        }
    }

    [Fact]
    public async Task Vorhandene_Archivdatei_wird_nicht_ueberschrieben()
    {
        var testverzeichnis =
            ErzeugeTestverzeichnis();

        try
        {
            var quelldatei =
                Path.Combine(
                    testverzeichnis,
                    "b56-test.xlsx");

            await File.WriteAllBytesAsync(
                quelldatei,
                [0x50, 0x4B, 0x03, 0x04]);

            var service =
                new B56ArchivService(
                    new B56ImportOptionen
                    {
                        ArchivBasisverzeichnis =
                            Path.Combine(
                                testverzeichnis,
                                "archiv")
                    });

            var projektId =
                Guid.NewGuid();

            var importzeitpunkt =
                new DateTimeOffset(
                    2026,
                    7,
                    24,
                    18,
                    30,
                    45,
                    TimeSpan.Zero);

            var archivdatei =
                await service.ArchivierenAsync(
                    projektId,
                    "Testprojekt",
                    quelldatei,
                    "1234567890abcdef",
                    importzeitpunkt);

            await File.WriteAllBytesAsync(
                quelldatei,
                [0x09, 0x08, 0x07]);

            await Assert.ThrowsAsync<IOException>(
                () =>
                    service.ArchivierenAsync(
                        projektId,
                        "Testprojekt",
                        quelldatei,
                        "1234567890abcdef",
                        importzeitpunkt));

            Assert.Equal(
                new byte[]
                {
                    0x50,
                    0x4B,
                    0x03,
                    0x04
                },
                await File.ReadAllBytesAsync(
                    archivdatei));
        }
        finally
        {
            Directory.Delete(
                testverzeichnis,
                recursive: true);
        }
    }

    private static string ErzeugeTestverzeichnis()
    {
        var testverzeichnis =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-archiv-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            testverzeichnis);

        return testverzeichnis;
    }
}
