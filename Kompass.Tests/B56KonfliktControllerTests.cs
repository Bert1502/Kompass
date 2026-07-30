using Kompass.Api.B56Import;
using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.Api;

public sealed class B56KonfliktControllerTests
{
    // ─── Listen ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Listen_liefert_400_wenn_vorgaenger_leer()
    {
        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake());

        var antwort =
            await controller.ListenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                vorgaenger: Guid.Empty,
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Listen_liefert_200_mit_Konfliktliste()
    {
        var eintrag = ErstelleEintrag();

        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake(liste: [eintrag]));

        var antwort =
            await controller.ListenAsync(
                eintrag.ProjektId,
                eintrag.NachfolgerSnapshotId,
                vorgaenger: eintrag.VorgaengerSnapshotId,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet =
            Assert.IsAssignableFrom<IReadOnlyList<B56KonfliktAntwort>>(
                ok.Value);

        Assert.Single(gesendet);
        Assert.Equal(eintrag.KonfliktId, gesendet[0].KonfliktId);
    }

    // ─── Entscheiden ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Entscheiden_liefert_400_wenn_vorgaenger_leer()
    {
        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake());

        var request =
            new B56KonfliktEntscheidungRequest(
                B56KonfliktEntscheidungsTyp.Akzeptiert);

        var antwort =
            await controller.EntscheidenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                vorgaenger: Guid.Empty,
                request,
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Entscheiden_liefert_400_wenn_Entscheidung_Ausstehend()
    {
        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake());

        var request =
            new B56KonfliktEntscheidungRequest(
                B56KonfliktEntscheidungsTyp.Ausstehend);

        var antwort =
            await controller.EntscheidenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                vorgaenger: Guid.NewGuid(),
                request,
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Entscheiden_liefert_404_wenn_Konflikt_nicht_gefunden()
    {
        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake(entschieden: null));

        var request =
            new B56KonfliktEntscheidungRequest(
                B56KonfliktEntscheidungsTyp.Abgelehnt);

        var antwort =
            await controller.EntscheidenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                vorgaenger: Guid.NewGuid(),
                request,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Entscheiden_liefert_200_mit_aktualisiertem_Eintrag()
    {
        var eintrag = ErstelleEintrag(
            entscheidung: B56KonfliktEntscheidungsTyp.Akzeptiert,
            entschiedenAm: DateTimeOffset.UtcNow);

        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake(entschieden: eintrag));

        var request =
            new B56KonfliktEntscheidungRequest(
                B56KonfliktEntscheidungsTyp.Akzeptiert);

        var antwort =
            await controller.EntscheidenAsync(
                eintrag.ProjektId,
                eintrag.NachfolgerSnapshotId,
                eintrag.KonfliktId,
                vorgaenger: eintrag.VorgaengerSnapshotId,
                request,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet =
            Assert.IsType<B56KonfliktAntwort>(ok.Value);

        Assert.Equal(
            B56KonfliktEntscheidungsTyp.Akzeptiert,
            gesendet.Entscheidung);
    }

    // ─── Alle akzeptieren ────────────────────────────────────────────────────

    [Fact]
    public async Task AlleAkzeptieren_liefert_400_wenn_vorgaenger_leer()
    {
        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake());

        var antwort =
            await controller.AlleAkzeptierenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                vorgaenger: Guid.Empty,
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task AlleAkzeptieren_liefert_200_mit_Anzahl()
    {
        var controller =
            new B56KonfliktController(
                new KonfliktServiceFake(alleAkzeptiertAnzahl: 3));

        var antwort =
            await controller.AlleAkzeptierenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                vorgaenger: Guid.NewGuid(),
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet =
            Assert.IsType<B56AlleAkzeptiertAntwort>(ok.Value);

        Assert.Equal(3, gesendet.AkzeptierteKonflikte);
    }

    // ─── Hilfsmethoden ────────────────────────────────────────────────────────

    private static B56KonfliktEintrag ErstelleEintrag(
        B56KonfliktEntscheidungsTyp entscheidung =
            B56KonfliktEntscheidungsTyp.Ausstehend,
        DateTimeOffset? entschiedenAm = null)
    {
        return new B56KonfliktEintrag(
            KonfliktId: Guid.NewGuid(),
            ProjektId: Guid.NewGuid(),
            VorgaengerSnapshotId: Guid.NewGuid(),
            NachfolgerSnapshotId: Guid.NewGuid(),
            Bereich: "Bestandskennwert",
            Schluessel: "Heizwärmebedarf",
            Feld: "Wert",
            Aenderung: B56VergleichsAenderung.Geaendert,
            AlterWert: "120.5",
            NeuerWert: "98.3",
            Entscheidung: entscheidung,
            EntschiedenAm: entschiedenAm);
    }

    private sealed class KonfliktServiceFake : IB56KonfliktService
    {
        private readonly IReadOnlyList<B56KonfliktEintrag> _liste;
        private readonly B56KonfliktEintrag? _entschieden;
        private readonly int _alleAkzeptiertAnzahl;

        public KonfliktServiceFake(
            IReadOnlyList<B56KonfliktEintrag>? liste = null,
            B56KonfliktEintrag? entschieden = null,
            int alleAkzeptiertAnzahl = 0)
        {
            _liste = liste ?? [];
            _entschieden = entschieden;
            _alleAkzeptiertAnzahl = alleAkzeptiertAnzahl;
        }

        public Task<IReadOnlyList<B56KonfliktEintrag>> ListenAsync(
            Guid projektId,
            Guid vorgaengerSnapshotId,
            Guid nachfolgerSnapshotId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_liste);

        public Task<B56KonfliktEintrag?> EntscheidenAsync(
            Guid projektId,
            Guid vorgaengerSnapshotId,
            Guid nachfolgerSnapshotId,
            Guid konfliktId,
            B56KonfliktEntscheidungsTyp entscheidung,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_entschieden);

        public Task<int> AlleAusstehendAkzeptierenAsync(
            Guid projektId,
            Guid vorgaengerSnapshotId,
            Guid nachfolgerSnapshotId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_alleAkzeptiertAnzahl);
    }
}
