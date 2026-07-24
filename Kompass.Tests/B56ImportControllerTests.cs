using Kompass.Api.B56Import;
using Kompass.Application.B56Import;
using Kompass.Application.Projects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kompass.Tests.Api;

public sealed class B56ImportControllerTests
{
    [Fact]
    public async Task Import_fuer_unbekanntes_Projekt_liefert_NotFound()
    {
        var controller =
            new B56ImportController(
                new ProjektServiceFake(null),
                new ImportServiceFake(),
                new ImportRegisterFake([]));

        using var dateiStream =
            new MemoryStream(
                [0x50, 0x4B, 0x03, 0x04]);

        var ergebnis =
            await controller.ImportierenAsync(
                Guid.NewGuid(),
                ErzeugeFormDatei(
                    dateiStream),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public async Task Abgelehnter_Import_loescht_temporaere_Datei()
    {
        var projektId =
            Guid.NewGuid();

        string? temporaererDateipfad =
            null;

        var importService =
            new ImportServiceFake(
                (anfrage, _) =>
                {
                    temporaererDateipfad =
                        anfrage.Quelldateipfad;

                    Assert.True(
                        File.Exists(
                            temporaererDateipfad));

                    Assert.Equal(
                        "test.xlsx",
                        Path.GetFileName(
                            temporaererDateipfad));

                    return Task.FromResult(
                        B56ImportErgebnis.Abgelehnt(
                            projektId,
                            temporaererDateipfad,
                            "B56-TEST",
                            "Testablehnung"));
                });

        var controller =
            new B56ImportController(
                new ProjektServiceFake(
                    new ProjektUebersicht(
                        projektId,
                        "Testprojekt",
                        0)),
                importService,
                new ImportRegisterFake([]));

        using var dateiStream =
            new MemoryStream(
                [0x50, 0x4B, 0x03, 0x04]);

        var ergebnis =
            await controller.ImportierenAsync(
                projektId,
                ErzeugeFormDatei(
                    dateiStream),
                CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(
            ergebnis.Result);

        Assert.NotNull(
            temporaererDateipfad);

        Assert.False(
            File.Exists(
                temporaererDateipfad));

        Assert.False(
            Directory.Exists(
                Path.GetDirectoryName(
                    temporaererDateipfad)));
    }

    [Fact]
    public async Task Abgebrochener_Import_loescht_temporaere_Datei_und_bleibt_abgebrochen()
    {
        var projektId =
            Guid.NewGuid();

        string? temporaererDateipfad =
            null;

        var importService =
            new ImportServiceFake(
                (anfrage, cancellationToken) =>
                {
                    temporaererDateipfad =
                        anfrage.Quelldateipfad;

                    Assert.True(
                        File.Exists(
                            temporaererDateipfad));

                    throw new OperationCanceledException(
                        cancellationToken);
                });

        var controller =
            new B56ImportController(
                new ProjektServiceFake(
                    new ProjektUebersicht(
                        projektId,
                        "Testprojekt",
                        0)),
                importService,
                new ImportRegisterFake([]));

        using var dateiStream =
            new MemoryStream(
                [0x50, 0x4B, 0x03, 0x04]);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                controller.ImportierenAsync(
                    projektId,
                    ErzeugeFormDatei(
                        dateiStream),
                    CancellationToken.None));

        Assert.NotNull(
            temporaererDateipfad);

        Assert.False(
            File.Exists(
                temporaererDateipfad));

        Assert.False(
            Directory.Exists(
                Path.GetDirectoryName(
                    temporaererDateipfad)));
    }

    [Theory]
    [InlineData(
        B56ImportStatus.Erfolgreich,
        StatusCodes.Status201Created)]
    [InlineData(
        B56ImportStatus.BereitsImportiert,
        StatusCodes.Status200OK)]
    [InlineData(
        B56ImportStatus.Fehlgeschlagen,
        StatusCodes.Status500InternalServerError)]
    public async Task Importstatus_wird_auf_HTTP_Status_abgebildet(
        B56ImportStatus importStatus,
        int erwarteterHttpStatus)
    {
        var projektId =
            Guid.NewGuid();

        var importService =
            new ImportServiceFake(
                (anfrage, _) =>
                    Task.FromResult(
                        ErzeugeImportErgebnis(
                            importStatus,
                            projektId,
                            anfrage.Quelldateipfad)));

        var controller =
            new B56ImportController(
                new ProjektServiceFake(
                    new ProjektUebersicht(
                        projektId,
                        "Testprojekt",
                        0)),
                importService,
                new ImportRegisterFake([]));

        using var dateiStream =
            new MemoryStream(
                [0x50, 0x4B, 0x03, 0x04]);

        var ergebnis =
            await controller.ImportierenAsync(
                projektId,
                ErzeugeFormDatei(
                    dateiStream),
                CancellationToken.None);

        var objectResult =
            Assert.IsAssignableFrom<ObjectResult>(
                ergebnis.Result);

        Assert.Equal(
            erwarteterHttpStatus,
            objectResult.StatusCode);
    }

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

    private static B56ImportErgebnis ErzeugeImportErgebnis(
        B56ImportStatus status,
        Guid projektId,
        string quelldateipfad)
    {
        var eintrag =
            ErzeugeEintrag(
                projektId,
                "test.xlsx",
                "C:\\Intern\\test.xlsx",
                DateTimeOffset.UtcNow);

        return status switch
        {
            B56ImportStatus.Erfolgreich =>
                B56ImportErgebnis.Erfolgreich(
                    eintrag,
                    quelldateipfad,
                    new B56ImportPipelineErgebnis()),

            B56ImportStatus.BereitsImportiert =>
                B56ImportErgebnis.BereitsImportiert(
                    eintrag,
                    quelldateipfad),

            B56ImportStatus.Fehlgeschlagen =>
                B56ImportErgebnis.Fehlgeschlagen(
                    projektId,
                    quelldateipfad,
                    "Testfehler"),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    null)
        };
    }


    private static IFormFile ErzeugeFormDatei(
        Stream stream)
    {
        return new FormFile(
            stream,
            0,
            stream.Length,
            "datei",
            "unterordner/test.xlsx");
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
        private readonly Func<
            B56ImportAnfrage,
            CancellationToken,
            Task<B56ImportErgebnis>>? _importieren;

        public ImportServiceFake(
            Func<
                B56ImportAnfrage,
                CancellationToken,
                Task<B56ImportErgebnis>>? importieren = null)
        {
            _importieren = importieren;
        }

        public Task<B56ImportErgebnis> ImportierenAsync(
            B56ImportAnfrage anfrage,
            CancellationToken cancellationToken = default)
        {
            return _importieren?.Invoke(
                    anfrage,
                    cancellationToken)
                ?? throw new NotSupportedException();
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
