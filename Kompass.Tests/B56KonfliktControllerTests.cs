using Kompass.Api.B56Import;
using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.Api;

public sealed class B56KonfliktControllerTests
{
    [Fact]
    public async Task Listen_liefert_400_wenn_vorgaenger_leer()
    {
        var controller = new B56KonfliktController(
            new B56KonfliktServiceFake());

        var antwort =
            await controller.ListenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty,
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Listen_liefert_200_mit_Konflikten()
    {
        var eintrag = ErstelleKonfliktEintrag();

        var controller = new B56KonfliktController(
            new B56KonfliktServiceFake(konflikte: [eintrag]));

        var antwort =
            await controller.ListenAsync(
                eintrag.ProjektId,
                eintrag.NachfolgerImportId,
                eintrag.VorgaengerImportId,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var liste =
            Assert.IsAssignableFrom<
                IReadOnlyList<B56KonfliktEintragAntwort>>(
                ok.Value);

        Assert.Single(liste);
        Assert.Equal(eintrag.Bereich, liste[0].Bereich);
        Assert.Equal(eintrag.Schluessel, liste[0].Schluessel);
        Assert.Equal(B56KonfliktEntscheidungsTyp.Offen, liste[0].Entscheidung);
    }

    [Fact]
    public async Task Listen_liefert_200_mit_leerer_Liste_wenn_kein_Vergleich_vorhanden()
    {
        var controller = new B56KonfliktController(
            new B56KonfliktServiceFake(konflikte: []));

        var antwort =
            await controller.ListenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var liste =
            Assert.IsAssignableFrom<
                IReadOnlyList<B56KonfliktEintragAntwort>>(
                ok.Value);

        Assert.Empty(liste);
    }

    [Fact]
    public async Task EntscheidungSetzen_liefert_204_bei_Erfolg()
    {
        var id = Guid.NewGuid();

        var controller = new B56KonfliktController(
            new B56KonfliktServiceFake(entscheidungErgebnis: true));

        var antwort =
            await controller.EntscheidungSetzenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                id,
                new B56KonfliktEntscheidungAnfrage(
                    B56KonfliktEntscheidungsTyp.Uebernehmen),
                CancellationToken.None);

        Assert.IsType<NoContentResult>(antwort);
    }

    [Fact]
    public async Task EntscheidungSetzen_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new B56KonfliktController(
            new B56KonfliktServiceFake(entscheidungErgebnis: false));

        var antwort =
            await controller.EntscheidungSetzenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                new B56KonfliktEntscheidungAnfrage(
                    B56KonfliktEntscheidungsTyp.Behalten),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort);
    }

    [Fact]
    public async Task EntscheidungSetzen_liefert_400_wenn_Entscheidung_Offen()
    {
        var controller = new B56KonfliktController(
            new B56KonfliktServiceFake(entscheidungErgebnis: true));

        var antwort =
            await controller.EntscheidungSetzenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                new B56KonfliktEntscheidungAnfrage(
                    B56KonfliktEntscheidungsTyp.Offen),
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort);
    }

    private static B56KonfliktEintrag ErstelleKonfliktEintrag()
    {
        var projektId = Guid.NewGuid();
        var vorgaenger = Guid.NewGuid();
        var nachfolger = Guid.NewGuid();

        return new B56KonfliktEintrag
        {
            Id = Guid.NewGuid(),
            ProjektId = projektId,
            VorgaengerImportId = vorgaenger,
            NachfolgerImportId = nachfolger,
            Bereich = "Bestandskennwert",
            Schluessel = "Heizwärmebedarf",
            Feld = "Wert",
            Aenderung = B56VergleichsAenderung.Geaendert,
            Entscheidung = B56KonfliktEntscheidungsTyp.Offen,
            ErstelltAm = DateTimeOffset.UtcNow
        };
    }

    private sealed class B56KonfliktServiceFake(
        IReadOnlyList<B56KonfliktEintrag>? konflikte = null,
        bool entscheidungErgebnis = true)
        : IB56KonfliktService
    {
        public Task<IReadOnlyList<B56KonfliktEintrag>>
            ListenOderErzeugenAsync(
                Guid projektId,
                Guid vorgaengerImportId,
                Guid nachfolgerImportId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<B56KonfliktEintrag> ergebnis =
                konflikte ?? [];
            return Task.FromResult(ergebnis);
        }

        public Task<bool> EntscheidungSetzenAsync(
            Guid projektId,
            Guid nachfolgerImportId,
            Guid id,
            B56KonfliktEntscheidungsTyp entscheidung,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(entscheidungErgebnis);
        }
    }
}
