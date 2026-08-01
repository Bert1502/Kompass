using Kompass.Api.Verbrauch;
using Kompass.Application.Verbrauch;
using Kompass.Domain.Economics;
using Kompass.Domain.Verbrauch;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.Api;

public sealed class VerbrauchsDatenControllerTests
{
    [Fact]
    public async Task Listen_liefert_200_mit_Verbrauchsdaten()
    {
        var daten = ErstelleVerbrauchsDaten();

        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(liste: [daten]));

        var antwort =
            await controller.ListenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet =
            Assert.IsAssignableFrom<IReadOnlyList<VerbrauchsDaten>>(ok.Value);

        Assert.Single(gesendet);
    }

    [Fact]
    public async Task Abrufen_liefert_200_wenn_gefunden()
    {
        var daten = ErstelleVerbrauchsDaten();

        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(einzelne: daten));

        var antwort =
            await controller.AbrufenAsync(
                daten.ProjektId,
                daten.Id,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        Assert.IsType<VerbrauchsDaten>(ok.Value);
    }

    [Fact]
    public async Task Abrufen_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(einzelne: null));

        var antwort =
            await controller.AbrufenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Anlegen_liefert_201_wenn_erfolgreich()
    {
        var daten = ErstelleVerbrauchsDaten();

        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(angelegt: daten));

        var request = new VerbrauchsDatenAnlegenRequest
        {
            PeriodeVon = new DateOnly(2024, 1, 1),
            PeriodeBis = new DateOnly(2024, 12, 31),
            Energietraeger = Energietraeger.Gas,
            Menge = 12000m,
            Kosten = 2400m
        };

        var antwort =
            await controller.AnlegenAsync(
                daten.ProjektId,
                request,
                CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(antwort.Result);
        Assert.IsType<VerbrauchsDaten>(created.Value);
    }

    [Fact]
    public async Task Anlegen_liefert_404_wenn_Projekt_nicht_gefunden()
    {
        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(angelegt: null));

        var request = new VerbrauchsDatenAnlegenRequest
        {
            PeriodeVon = new DateOnly(2024, 1, 1),
            PeriodeBis = new DateOnly(2024, 12, 31),
            Energietraeger = Energietraeger.Gas,
            Menge = 12000m,
            Kosten = 2400m
        };

        var antwort =
            await controller.AnlegenAsync(
                Guid.NewGuid(),
                request,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Aktualisieren_liefert_200_wenn_erfolgreich()
    {
        var daten = ErstelleVerbrauchsDaten();

        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(einzelne: daten, aktualisierenErgebnis: true));

        var request = new VerbrauchsDatenAktualisierenRequest
        {
            PeriodeVon = new DateOnly(2024, 1, 1),
            PeriodeBis = new DateOnly(2024, 12, 31),
            Energietraeger = Energietraeger.Gas,
            Menge = 13000m,
            Kosten = 2600m,
            WitterungsbereinigungsFaktor = 1.08m
        };

        var antwort =
            await controller.AktualisierenAsync(
                daten.ProjektId,
                daten.Id,
                request,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        Assert.IsType<VerbrauchsDaten>(ok.Value);
    }

    [Fact]
    public async Task Aktualisieren_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(einzelne: null));

        var request = new VerbrauchsDatenAktualisierenRequest
        {
            PeriodeVon = new DateOnly(2024, 1, 1),
            PeriodeBis = new DateOnly(2024, 12, 31),
            Energietraeger = Energietraeger.Gas,
            Menge = 12000m,
            Kosten = 2400m
        };

        var antwort =
            await controller.AktualisierenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                request,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Loeschen_liefert_204_wenn_erfolgreich()
    {
        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(loeschenErgebnis: true));

        var antwort =
            await controller.LoeschenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NoContentResult>(antwort);
    }

    [Fact]
    public async Task Loeschen_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(loeschenErgebnis: false));

        var antwort =
            await controller.LoeschenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort);
    }

    [Fact]
    public async Task Zusammenfassung_liefert_200_mit_Liste()
    {
        var zusammenfassung = new List<VerbrauchsZusammenfassungJeEnergietraeger>
        {
            new(Energietraeger.Gas, 2, 24000m, 24000m, 12000m, 4800m),
        };

        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(zusammenfassung: zusammenfassung));

        var antwort =
            await controller.ZusammenfassenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var ergebnis = Assert.IsAssignableFrom<IReadOnlyList<VerbrauchsZusammenfassungJeEnergietraeger>>(ok.Value);
        Assert.Single(ergebnis);
    }

    [Fact]
    public async Task Zusammenfassung_liefert_404_wenn_Projekt_nicht_gefunden()
    {
        var controller = new VerbrauchsDatenController(
            new VerbrauchsDatenServiceFake(zusammenfassung: null));

        var antwort =
            await controller.ZusammenfassenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    private static VerbrauchsDaten ErstelleVerbrauchsDaten() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            Energietraeger.Gas,
            12000m,
            2400m);

    private sealed class VerbrauchsDatenServiceFake(
        IReadOnlyList<VerbrauchsDaten>? liste = null,
        VerbrauchsDaten? einzelne = null,
        VerbrauchsDaten? angelegt = null,
        bool aktualisierenErgebnis = true,
        bool loeschenErgebnis = true,
        IReadOnlyList<VerbrauchsZusammenfassungJeEnergietraeger>? zusammenfassung = null)
        : IVerbrauchsDatenService
    {
        public Task<IReadOnlyList<VerbrauchsDaten>> ListenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<VerbrauchsDaten> ergebnis = liste ?? [];
            return Task.FromResult(ergebnis);
        }

        public Task<VerbrauchsDaten?> AbrufenAsync(
            Guid projektId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(einzelne);
        }

        public Task<VerbrauchsDaten?> AnlegenAsync(
            VerbrauchsDaten verbrauchsDaten,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(angelegt);
        }

        public Task<bool> AktualisierenAsync(
            VerbrauchsDaten verbrauchsDaten,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(aktualisierenErgebnis);
        }

        public Task<bool> LoeschenAsync(
            Guid projektId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(loeschenErgebnis);
        }

        public Task<IReadOnlyList<VerbrauchsZusammenfassungJeEnergietraeger>?> ZusammenfassenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(zusammenfassung);
        }
    }
}
