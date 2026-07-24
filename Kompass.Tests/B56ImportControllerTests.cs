using Kompass.Api.B56Import;
using Kompass.Application.B56Import;
using Kompass.Application.Projects;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kompass.Tests.Api;

public sealed class B56ImportControllerTests
{
    [Fact]
    public async Task Historie_fuer_unbekanntes_Projekt_liefert_NotFound()
    {
        var controller =
            new B56ImportController(
                new ProjektServiceFake(null),
                new ImportServiceFake(),
                new ImportRegisterFake([]));

        var ergebnis =
            await controller.HistorieAbrufenAsync(
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Historie_liefert_sichere_Registerdaten_ohne_Archivpfad()
    {
        var projektId =
            Guid.NewGuid();

        var neuererEintrag =
            ErzeugeEintrag(
                projektId,
                "neu.xlsx",
                "C:\\Intern\\Geheim\\neu.xlsx",
                DateTimeOffset.Parse(
                    "2026-07-24T12:00:00+02:00"));

        var aeltererEintrag =
            ErzeugeEintrag(
                projektId,
                "alt.xlsx",
                "C:\\Intern\\Geheim\\alt.xlsx",
                DateTimeOffset.Parse(
                    "2026-07-23T12:00:00+02:00"));

        var controller =
            new B56ImportController(
                new ProjektServiceFake(
                    new ProjektUebersicht(
                        projektId,
                        "Testprojekt",
                        0)),
                new ImportServiceFake(),
                new ImportRegisterFake(
                    [
                        neuererEintrag,
                        aeltererEintrag
                    ]));

        var ergebnis =
            await controller.HistorieAbrufenAsync(
                projektId,
                CancellationToken.None);

        var ok =
            Assert.IsType<OkObjectResult>(
                ergebnis.Result);

        var antworten =
            Assert.IsAssignableFrom<
                    IEnumerable<B56ImportHistorieAntwort>>(
                    ok.Value)
                .ToList();

        Assert.Equal(
            [
                neuererEintrag.ImportId,
                aeltererEintrag.ImportId
            ],
            antworten.Select(x => x.ImportId));

        var json =
            JsonSerializer.Serialize(
                antworten,
                new JsonSerializerOptions(
                    JsonSerializerDefaults.Web));

        Assert.DoesNotContain(
            "Intern",
            json,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "archivdateipfad",
            json,
            StringComparison.OrdinalIgnoreCase);
    }

    private static B56ImportEintrag ErzeugeEintrag(
        Guid projektId,
        string originaldateiname,
        string archivdateipfad,
        DateTimeOffset importiertAm)
    {
        return new B56ImportEintrag
        {
            ImportId = Guid.NewGuid(),
            ProjektId = projektId,
            Projektname = "Testprojekt",
            Originaldateiname = originaldateiname,
            Archivdateipfad = archivdateipfad,
            Sha256 = "0123456789abcdef",
            DateigroesseBytes = 1024,
            ImportiertAm = importiertAm,
            Dateiendung = ".xlsx"
        };
    }

    private sealed class ProjektServiceFake
        : IProjektService
    {
        private readonly ProjektUebersicht? _projekt;

        public ProjektServiceFake(
            ProjektUebersicht? projekt)
        {
            _projekt = projekt;
        }

        public Task<ProjektUebersicht?> NachIdAbrufenAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _projekt);
        }

        public Task<IReadOnlyList<ProjektUebersicht>> AlleAbrufenAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ProjektUebersicht> ErstellenAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ProjektUebersicht?> AktualisierenAsync(
            Guid id,
            string name,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> LoeschenAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class ImportServiceFake
        : IB56ImportService
    {
        public Task<B56ImportErgebnis> ImportierenAsync(
            B56ImportAnfrage anfrage,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class ImportRegisterFake
        : IB56ImportRegister
    {
        private readonly IReadOnlyList<B56ImportEintrag> _eintraege;

        public ImportRegisterFake(
            IReadOnlyList<B56ImportEintrag> eintraege)
        {
            _eintraege = eintraege;
        }

        public Task<IReadOnlyList<B56ImportEintrag>>
            AlleFuerProjektAbrufenAsync(
                Guid projektId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _eintraege);
        }

        public Task<B56ImportEintrag?> NachHashSuchenAsync(
            Guid projektId,
            string sha256,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task EintragSpeichernAsync(
            B56ImportEintrag eintrag,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
