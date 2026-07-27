using Kompass.Api.Projects;
using Kompass.Application.Projects;
using Kompass.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kompass.Tests.Api;

public sealed class ProjekteControllerTests
{
    [Fact]
    public async Task AlleAbrufen_liefert_OK_mit_Projektliste()
    {
        var projekte =
            new List<ProjektUebersicht>
            {
                new(Guid.NewGuid(), "Altbau", 0),
                new(Guid.NewGuid(), "Zentrale", 2)
            };

        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    alleAbrufen: projekte),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.AlleAbrufenAsync(
                CancellationToken.None);

        var ok =
            Assert.IsType<OkObjectResult>(
                ergebnis.Result);

        Assert.Equal(
            projekte,
            ok.Value);
    }

    [Fact]
    public async Task NachIdAbrufen_liefert_OK_bei_bekannter_Id()
    {
        var id =
            Guid.NewGuid();

        var projekt =
            new ProjektUebersicht(
                id,
                "Rathaus",
                0);

        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    nachIdAbrufen: projekt),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.NachIdAbrufenAsync(
                id,
                CancellationToken.None);

        var ok =
            Assert.IsType<OkObjectResult>(
                ergebnis.Result);

        Assert.Equal(
            projekt,
            ok.Value);
    }

    [Fact]
    public async Task NachIdAbrufen_liefert_NotFound_bei_unbekannter_Id()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    nachIdAbrufen: null),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.NachIdAbrufenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Erstellen_liefert_Created_bei_gueltigem_Namen()
    {
        var id =
            Guid.NewGuid();

        var neuesProjekt =
            new ProjektUebersicht(
                id,
                "Rathaus",
                0);

        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    erstellen: neuesProjekt),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.ErstellenAsync(
                new ProjektErstellenRequest
                {
                    Name = "Rathaus"
                },
                CancellationToken.None);

        var created =
            Assert.IsType<CreatedAtRouteResult>(
                ergebnis.Result);

        Assert.Equal(
            neuesProjekt,
            created.Value);
    }

    [Fact]
    public async Task Erstellen_liefert_BadRequest_bei_ungueltigem_Namen()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    erstellenWirft:
                        new DomainException(
                            "Der Projektname darf nicht leer sein.")),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.ErstellenAsync(
                new ProjektErstellenRequest
                {
                    Name = " "
                },
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Erstellen_liefert_Conflict_bei_duplikatem_Namen()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    erstellenWirft:
                        new InvalidOperationException(
                            "Ein Projekt mit dem Namen 'Rathaus' ist bereits vorhanden.")),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.ErstellenAsync(
                new ProjektErstellenRequest
                {
                    Name = "Rathaus"
                },
                CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Aktualisieren_liefert_OK_bei_gueltigem_Namen()
    {
        var id =
            Guid.NewGuid();

        var aktualisiertesProjekt =
            new ProjektUebersicht(
                id,
                "Schule",
                0);

        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    aktualisieren: aktualisiertesProjekt),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.AktualisierenAsync(
                id,
                new ProjektAktualisierenRequest
                {
                    Name = "Schule"
                },
                CancellationToken.None);

        var ok =
            Assert.IsType<OkObjectResult>(
                ergebnis.Result);

        Assert.Equal(
            aktualisiertesProjekt,
            ok.Value);
    }

    [Fact]
    public async Task Aktualisieren_liefert_NotFound_bei_unbekannter_Id()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    aktualisieren: null),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.AktualisierenAsync(
                Guid.NewGuid(),
                new ProjektAktualisierenRequest
                {
                    Name = "Unbekannt"
                },
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Aktualisieren_liefert_BadRequest_bei_ungueltigem_Namen()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    aktualisierenWirft:
                        new DomainException(
                            "Der Projektname darf nicht leer sein.")),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.AktualisierenAsync(
                Guid.NewGuid(),
                new ProjektAktualisierenRequest
                {
                    Name = " "
                },
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Aktualisieren_liefert_Conflict_bei_duplikatem_Namen()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    aktualisierenWirft:
                        new InvalidOperationException(
                            "Ein anderes Projekt mit dem Namen 'Schule' ist bereits vorhanden.")),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.AktualisierenAsync(
                Guid.NewGuid(),
                new ProjektAktualisierenRequest
                {
                    Name = "Schule"
                },
                CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Loeschen_liefert_NoContent_bei_bekannter_Id()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    loeschen: true),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.LoeschenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NoContentResult>(
            ergebnis);
    }

    [Fact]
    public async Task Loeschen_liefert_NotFound_bei_unbekannter_Id()
    {
        var controller =
            new ProjekteController(
                new ProjektServiceFake(
                    loeschen: false),
                NullLogger<ProjekteController>.Instance);

        var ergebnis =
            await controller.LoeschenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(
            ergebnis);
    }

    private sealed class ProjektServiceFake : IProjektService
    {
        private readonly IReadOnlyList<ProjektUebersicht>? _alleAbrufen;
        private readonly ProjektUebersicht? _nachIdAbrufen;
        private readonly ProjektUebersicht? _erstellen;
        private readonly Exception? _erstellenWirft;
        private readonly ProjektUebersicht? _aktualisieren;
        private readonly Exception? _aktualisierenWirft;
        private readonly bool _loeschen;

        public ProjektServiceFake(
            IReadOnlyList<ProjektUebersicht>? alleAbrufen = null,
            ProjektUebersicht? nachIdAbrufen = null,
            ProjektUebersicht? erstellen = null,
            Exception? erstellenWirft = null,
            ProjektUebersicht? aktualisieren = null,
            Exception? aktualisierenWirft = null,
            bool loeschen = false)
        {
            _alleAbrufen = alleAbrufen;
            _nachIdAbrufen = nachIdAbrufen;
            _erstellen = erstellen;
            _erstellenWirft = erstellenWirft;
            _aktualisieren = aktualisieren;
            _aktualisierenWirft = aktualisierenWirft;
            _loeschen = loeschen;
        }

        public Task<IReadOnlyList<ProjektUebersicht>> AlleAbrufenAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _alleAbrufen
                ?? (IReadOnlyList<ProjektUebersicht>)Array.Empty<ProjektUebersicht>());
        }

        public Task<ProjektUebersicht?> NachIdAbrufenAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _nachIdAbrufen);
        }

        public Task<ProjektUebersicht> ErstellenAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            if (_erstellenWirft is not null)
            {
                throw _erstellenWirft;
            }

            return Task.FromResult(
                _erstellen
                ?? throw new NotSupportedException());
        }

        public Task<ProjektUebersicht?> AktualisierenAsync(
            Guid id,
            string name,
            CancellationToken cancellationToken = default)
        {
            if (_aktualisierenWirft is not null)
            {
                throw _aktualisierenWirft;
            }

            return Task.FromResult(
                _aktualisieren);
        }

        public Task<ProjektUebersicht?> ProjektdatenAktualisierenAsync(
            Guid id,
            string? interneBezeichnung,
            Kompass.Domain.Projects.Bearbeitungsstatus bearbeitungsstatus,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> LoeschenAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _loeschen);
        }
    }
}
