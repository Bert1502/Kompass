using Kompass.Api.Economics;
using Kompass.Application.Economics;
using Kompass.Domain.Economics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kompass.Tests;

public sealed class KostenpositionenControllerTests
{
    [Fact]
    public async Task Listen_liefert_gespeicherte_Positionen()
    {
        var position = ErstellePosition();
        var controller = ErstelleController(new KostenpositionServiceFake([position]));

        var antwort = await controller.ListenAsync(
            Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var positionen = Assert.IsAssignableFrom<IReadOnlyList<Kostenposition>>(ok.Value);
        Assert.Equal(position, Assert.Single(positionen));
    }

    [Fact]
    public async Task Hinzufuegen_liefert_201_und_uebergibt_Eingaben()
    {
        Kostenposition? empfangen = null;
        var service = new KostenpositionServiceFake(
            hinzufuegen: position =>
            {
                empfangen = position;
                return position;
            });
        var controller = ErstelleController(service);

        var antwort = await controller.HinzufuegenAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new KostenpositionHinzufuegenAnfrage
            {
                Bezeichnung = "  Planung  ",
                Betrag = 12_500.75m,
                Kostenart = Kostenart.Fachplanung
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtRouteResult>(antwort.Result);
        Assert.Equal("KostenpositionenListen", created.RouteName);
        Assert.Same(empfangen, created.Value);
        Assert.NotNull(empfangen);
        Assert.Equal("Planung", empfangen.Bezeichnung);
        Assert.Equal(12_500.75m, empfangen.Betrag);
        Assert.Equal(Kostenart.Fachplanung, empfangen.Kostenart);
    }

    [Fact]
    public async Task Hinzufuegen_liefert_400_bei_negativem_Betrag()
    {
        var controller = ErstelleController(new KostenpositionServiceFake());

        var antwort = await controller.HinzufuegenAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new KostenpositionHinzufuegenAnfrage
            {
                Bezeichnung = "Planung",
                Betrag = -1m,
                Kostenart = Kostenart.Fachplanung
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Hinzufuegen_liefert_404_bei_unbekannter_Alternative()
    {
        var controller = ErstelleController(
            new KostenpositionServiceFake(hinzufuegen: _ => null));

        var antwort = await controller.HinzufuegenAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new KostenpositionHinzufuegenAnfrage
            {
                Bezeichnung = "Planung",
                Betrag = 100m,
                Kostenart = Kostenart.Fachplanung
            },
            CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Entfernen_liefert_passenden_Status(bool entfernt)
    {
        var controller = ErstelleController(
            new KostenpositionServiceFake(entfernen: entfernt));

        var antwort = await controller.EntfernenAsync(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        if (entfernt)
        {
            Assert.IsType<NoContentResult>(antwort);
        }
        else
        {
            Assert.IsType<NotFoundObjectResult>(antwort);
        }
    }

    private static KostenpositionenController ErstelleController(
        IKostenpositionService service) =>
        new(service, NullLogger<KostenpositionenController>.Instance);

    private static Kostenposition ErstellePosition() =>
        new(Guid.NewGuid(), "Architektur", 1_000m, Kostenart.Architektur);

    private sealed class KostenpositionServiceFake(
        IReadOnlyList<Kostenposition>? liste = null,
        Func<Kostenposition, Kostenposition?>? hinzufuegen = null,
        bool entfernen = false)
        : IKostenpositionService
    {
        public Task<IReadOnlyList<Kostenposition>> ListenAsync(
            Guid projektId,
            Guid alternativeId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(liste ?? (IReadOnlyList<Kostenposition>)[]);

        public Task<Kostenposition?> HinzufuegenAsync(
            Guid projektId,
            Guid alternativeId,
            Kostenposition kostenposition,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(hinzufuegen?.Invoke(kostenposition));

        public Task<bool> EntfernenAsync(
            Guid projektId,
            Guid alternativeId,
            Guid kostenpositionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(entfernen);
    }
}
