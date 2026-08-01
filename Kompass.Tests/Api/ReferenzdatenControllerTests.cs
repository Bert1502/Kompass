using Kompass.Api.Referenzdaten;
using Kompass.Application.Referenzdaten;
using Kompass.Domain.Referenzdaten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kompass.Tests.Api;

public sealed class ReferenzdatenControllerTests
{
    [Fact]
    public async Task Aufloesen_liefert_404_wenn_kein_Wert_gefunden()
    {
        var controller = new ReferenzdatenController(
            new ReferenzdatenServiceFake(null),
            NullLogger<ReferenzdatenController>.Instance);

        var response = await controller.AufloesenAsync(
            "Diskontierungszinssatz",
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task Aufloesen_liefert_200_mit_Prioritaet()
    {
        var datensatz = new Referenzdatensatz(
            Guid.NewGuid(),
            "Diskontierungszinssatz",
            "Diskontierungszinssatz",
            "0.04",
            ReferenzdatenEbene.Systemweit,
            "Quelle",
            "Herausgeber",
            "https://example.invalid",
            new DateOnly(2026, 1, 1),
            null,
            "v1",
            ReferenzdatenStatus.Freigegeben,
            Qualitaetsstatus.OffizielleQuelle,
            ReferenzdatenImportart.AutomatischerAbruf,
            DateTimeOffset.UtcNow,
            einheit: "%");

        var controller = new ReferenzdatenController(
            new ReferenzdatenServiceFake(new ReferenzwertAufloesung(datensatz, ReferenzdatenPrioritaet.OffiziellerReferenzwert)),
            NullLogger<ReferenzdatenController>.Instance);

        var response = await controller.AufloesenAsync(
            "Diskontierungszinssatz",
            null,
            null,
            null,
            null,
            null,
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<ReferenzwertAufloesungResponse>(ok.Value);

        Assert.Equal("0.04", payload.Wert);
        Assert.Equal("OffiziellerReferenzwert", payload.Prioritaet);
    }

    private sealed class ReferenzdatenServiceFake(
        ReferenzwertAufloesung? aufloesung) : IReferenzdatenService
    {
        public Task<IReadOnlyList<Referenzdatensatz>> ListenAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Referenzdatensatz>>([]);
        }

        public Task<Referenzdatensatz> SpeichernAsync(Referenzdatensatz datensatz, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(datensatz);
        }

        public Task<ReferenzwertAufloesung?> WertAufloesenAsync(ReferenzwertAnfrage anfrage, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(aufloesung);
        }

        public Task<Kompass.Domain.Referenzdaten.ReferenzwertAbweichung> ProjektabweichungSetzenAsync(ProjektabweichungAnfrage anfrage, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Kompass.Domain.Referenzdaten.ReferenzwertAbweichung>> ProjektabweichungenListenAsync(Guid projektId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Kompass.Domain.Referenzdaten.ReferenzwertAbweichung>>([]);
        }

        public Task<ReferenzdatenSynchronisationsErgebnis> SynchronisierenAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ReferenzdatenSynchronisationsErgebnis([], 0, false));
        }
    }
}
