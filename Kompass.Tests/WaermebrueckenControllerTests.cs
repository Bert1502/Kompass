using Kompass.Api.Waermebruecken;
using Kompass.Application.Waermebruecken;
using Kompass.Domain.Common;
using Kompass.Domain.Waermebruecken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kompass.Tests.Api;

public sealed class WaermebrueckenControllerTests
{
    [Fact]
    public async Task Listen_liefert_200_mit_Waermebruecken()
    {
        var wb = ErstelleWaermebruecke();

        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(liste: [wb]));

        var antwort =
            await controller.ListenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        var gesendet =
            Assert.IsAssignableFrom<IReadOnlyList<Waermebruecke>>(ok.Value);

        Assert.Single(gesendet);
    }

    [Fact]
    public async Task Abrufen_liefert_200_wenn_gefunden()
    {
        var wb = ErstelleWaermebruecke();

        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(einzelne: wb));

        var antwort =
            await controller.AbrufenAsync(
                wb.ProjektId,
                wb.Id,
                CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(antwort.Result);
        Assert.IsType<Waermebruecke>(ok.Value);
    }

    [Fact]
    public async Task Abrufen_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(einzelne: null));

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
        var wb = ErstelleWaermebruecke();

        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(angelegt: wb));

        var request = new WaermebrueckeAnlegenRequest
        {
            InterneNummer = "WB01",
            Bezeichnung = "Außenwandecke",
            Typ = WaermebrueckeTyp.Ecke
        };

        var antwort =
            await controller.AnlegenAsync(
                wb.ProjektId,
                request,
                CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(antwort.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
    }

    [Fact]
    public async Task Anlegen_liefert_404_wenn_Projekt_nicht_gefunden()
    {
        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(angelegt: null));

        var request = new WaermebrueckeAnlegenRequest
        {
            InterneNummer = "WB01",
            Bezeichnung = "Außenwandecke",
            Typ = WaermebrueckeTyp.Ecke
        };

        var antwort =
            await controller.AnlegenAsync(
                Guid.NewGuid(),
                request,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort.Result);
    }

    [Fact]
    public async Task Anlegen_liefert_409_bei_doppelter_InternerNummer()
    {
        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(
                anlegenFehler: new DomainException("Nummer bereits vergeben.")));

        var request = new WaermebrueckeAnlegenRequest
        {
            InterneNummer = "WB01",
            Bezeichnung = "Außenwandecke",
            Typ = WaermebrueckeTyp.Ecke
        };

        var antwort =
            await controller.AnlegenAsync(
                Guid.NewGuid(),
                request,
                CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(antwort.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [Fact]
    public async Task Aktualisieren_liefert_204_wenn_erfolgreich()
    {
        var wb = ErstelleWaermebruecke();

        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(einzelne: wb, aktualisierenErgebnis: true));

        var request = new WaermebrueckeAktualisierenRequest
        {
            InterneNummer = "WB01",
            Bezeichnung = "Aktualisiert",
            Typ = WaermebrueckeTyp.Wandanschluss,
            Status = WaermebrueckeStatus.Berechnet,
            GleichwertigkeitStatus = GleichwertigkeitStatus.Gleichwertig
        };

        var antwort =
            await controller.AktualisierenAsync(
                wb.ProjektId,
                wb.Id,
                request,
                CancellationToken.None);

        Assert.IsType<NoContentResult>(antwort);
    }

    [Fact]
    public async Task Aktualisieren_liefert_404_wenn_nicht_gefunden()
    {
        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(einzelne: null));

        var request = new WaermebrueckeAktualisierenRequest
        {
            InterneNummer = "WB01",
            Bezeichnung = "Bezeichnung",
            Typ = WaermebrueckeTyp.Ecke,
            Status = WaermebrueckeStatus.Offen,
            GleichwertigkeitStatus = GleichwertigkeitStatus.NichtBewertet
        };

        var antwort =
            await controller.AktualisierenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                request,
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort);
    }

    [Fact]
    public async Task Loeschen_liefert_204_wenn_erfolgreich()
    {
        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(loeschenErgebnis: true));

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
        var controller = new WaermebrueckenController(
            new WaermebrueckeServiceFake(loeschenErgebnis: false));

        var antwort =
            await controller.LoeschenAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(antwort);
    }

    private static Waermebruecke ErstelleWaermebruecke() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "WB01",
            "Außenwandecke",
            WaermebrueckeTyp.Ecke);

    private sealed class WaermebrueckeServiceFake(
        IReadOnlyList<Waermebruecke>? liste = null,
        Waermebruecke? einzelne = null,
        Waermebruecke? angelegt = null,
        DomainException? anlegenFehler = null,
        bool aktualisierenErgebnis = true,
        bool loeschenErgebnis = true)
        : IWaermebrueckeService
    {
        public Task<IReadOnlyList<Waermebruecke>> ListenAsync(
            Guid projektId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Waermebruecke> ergebnis = liste ?? [];
            return Task.FromResult(ergebnis);
        }

        public Task<Waermebruecke?> AbrufenAsync(
            Guid projektId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(einzelne);
        }

        public Task<Waermebruecke?> AnlegenAsync(
            Waermebruecke waermebruecke,
            CancellationToken cancellationToken = default)
        {
            if (anlegenFehler is not null)
            {
                throw anlegenFehler;
            }

            return Task.FromResult(angelegt);
        }

        public Task<bool> AktualisierenAsync(
            Waermebruecke waermebruecke,
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
    }
}
