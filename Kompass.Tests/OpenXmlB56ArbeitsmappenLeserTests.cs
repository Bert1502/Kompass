using Kompass.Persistence.Services;
using Xunit;

namespace Kompass.Tests.B56Import;

public sealed class OpenXmlB56ArbeitsmappenLeserTests
{
    [Fact]
    public async Task Datei_nicht_vorhanden_wirft_Exception()
    {
        var leser =
            new OpenXmlB56ArbeitsmappenLeser();

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => leser.LesenAsync(@"C:\Test\ExistiertNicht.xlsx"));
    }
}
