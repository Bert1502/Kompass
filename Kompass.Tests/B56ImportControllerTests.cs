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

        Assert.All(
            antworten,
            antwort =>
            {
                Assert.Equal(
                    B56SnapshotVersionen.AktuelleSchemaVersion,
                    antwort.SnapshotSchemaVersion);
                Assert.Equal(
                    B56SnapshotVersionen.AktuelleParserVersion,
                    antwort.ParserVersion);
            });

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

    [Fact]
    public async Task Details_liefern_gespeicherte_Fachdaten()
    {
        var projektId =
            Guid.NewGuid();

        var importId =
            Guid.NewGuid();

        var controller =
            new B56ImportController(
                new ProjektServiceFake(
                    new ProjektUebersicht(
                        projektId,
                        "Testprojekt",
                        0)),
                new ImportServiceFake(),
                new ImportRegisterFake(
                    [],
                    new B56ImportPipelineErgebnis
                    {
                        ImportierteBauteile = 1,
                        Bauteile =
                        [
                            new B56Bauteil
                            {
                                Bauteilcode = "AW01",
                                Bezeichnung = "Außenwand",
                                Nachbarseite = "gegen Außenluft",
                                UWert = 0.24
                            }
                        ]
                    }));

        var ergebnis =
            await controller.DetailsAbrufenAsync(
                projektId,
                importId,
                CancellationToken.None);

        var ok =
            Assert.IsType<OkObjectResult>(
                ergebnis.Result);

        var antwort =
            Assert.IsType<B56ImportPipelineAntwort>(
                ok.Value);

        Assert.Equal(
            "AW01",
            Assert.Single(
                    antwort.Bauteile)
                .Bauteilcode);
    }

    [Fact]
    public async Task Details_ohne_Fachdaten_liefern_NotFound()
    {
        var projektId =
            Guid.NewGuid();

        var controller =
            new B56ImportController(
                new ProjektServiceFake(
                    new ProjektUebersicht(
                        projektId,
                        "Testprojekt",
                        0)),
                new ImportServiceFake(),
                new ImportRegisterFake([]));

        var ergebnis =
            await controller.DetailsAbrufenAsync(
                projektId,
                Guid.NewGuid(),
                CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(
            ergebnis.Result);
    }

    [Fact]
    public void Importantwort_enthaelt_fachliche_B56_Details()
    {
        var projektId =
            Guid.NewGuid();

        var pipelineErgebnis =
            new B56ImportPipelineErgebnis
            {
                ImportierteArbeitsblaetter = 1,
                ErkannteTabellen = 3,
                ImportierteTabellen = 3,
                ImportierteBauteile = 1,
                ImportierteKennwerte = 5,
                ImportierteModernisierungsalternativen = 1,
                Bauteile =
                [
                    new B56Bauteil
                    {
                        Bauteilcode = "AW01",
                        Bezeichnung = "Außenwand",
                        Nachbarseite = "gegen Außenluft",
                        UWert = 0.24
                    }
                ],
                Bestandskennwerte =
                [
                    new B56Kennwert
                    {
                        Name = "Primärenergiebedarf Gebäude",
                        Wert = 200
                    }
                ],
                Modernisierungsalternativen =
                [
                    AlternativeErzeugen()
                ]
            };

        var importErgebnis =
            B56ImportErgebnis.Erfolgreich(
                ErzeugeEintrag(
                    projektId,
                    "test.xlsx",
                    "C:\\Intern\\test.xlsx",
                    DateTimeOffset.UtcNow),
                "test.xlsx",
                pipelineErgebnis);

        var antwort =
            B56ImportAntwort.Aus(
                importErgebnis);

        Assert.NotNull(
            antwort.Pipeline);

        var bauteil =
            Assert.Single(
                antwort.Pipeline.Bauteile);

        Assert.Equal(
            "AW01",
            bauteil.Bauteilcode);

        var bestandskennwert =
            Assert.Single(
                antwort.Pipeline.Bestandskennwerte);

        Assert.Equal(
            200,
            bestandskennwert.Wert);

        var alternative =
            Assert.Single(
                antwort.Pipeline.Modernisierungsalternativen);

        Assert.Equal(
            "Fenster",
            alternative.Bezeichnung);

        Assert.Equal(
            3,
            alternative.Kennwerte.Count);
    }

    private static B56Modernisierungsalternative
        AlternativeErzeugen()
    {
        var alternative =
            new B56Modernisierungsalternative
            {
                Bezeichnung = "Fenster",
                Beschreibung = "Fenstertausch"
            };

        foreach (var name in new[]
                 {
                     "Primärenergiebedarf Gebäude",
                     "Endenergiebedarf Gebäude",
                     "CO2-Emissionen Gebäude"
                 })
        {
            alternative.Kennwerte.Add(
                new B56Kennwert
                {
                    Name = name,
                    Wert = 1
                });
        }

        return alternative;
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

        public Task<ProjektUebersicht?> ProjektdatenAktualisierenAsync(
            Guid id,
            string? interneBezeichnung,
            Kompass.Domain.Projects.Bearbeitungsstatus bearbeitungsstatus,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ProjektUebersicht?> StammdatenAktualisierenAsync(
            Guid id,
            string? auftraggeber,
            string? ansprechpartner,
            string? strasse,
            string? ort,
            string? postleitzahl,
            string? gebaeudeart,
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

        public Task<ProjektUebersicht?> FreigabestatusAktualisierenAsync(
            Guid id,
            Kompass.Domain.Projects.Freigabestatus status,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ProjektUebersicht?> NotizenAktualisierenAsync(
            Guid id,
            string? notizen,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AlternativeKurzinfo?> AlternativeNachIdAbrufenAsync(
            Guid projektId,
            Guid alternativeId,
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
        private readonly B56ImportPipelineErgebnis? _fachdaten;

        public ImportRegisterFake(
            IReadOnlyList<B56ImportEintrag> eintraege,
            B56ImportPipelineErgebnis? fachdaten = null)
        {
            _eintraege = eintraege;
            _fachdaten = fachdaten;
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

        public Task<B56ImportEintrag?> NachIdSuchenAsync(
            Guid projektId,
            Guid importId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _eintraege.SingleOrDefault(
                    eintrag =>
                        eintrag.ProjektId == projektId &&
                        eintrag.ImportId == importId));
        }

        public Task EintragSpeichernAsync(
            B56ImportEintrag eintrag,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task EintragMitFachdatenSpeichernAsync(
            B56ImportEintrag eintrag,
            B56ImportPipelineErgebnis fachdaten,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<B56ImportPipelineErgebnis?>
            FachdatenAbrufenAsync(
                Guid projektId,
                Guid importId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _fachdaten);
        }

        public Task<B56SnapshotVergleich?> VergleichAbrufenAsync(
            Guid projektId,
            Guid vorgaengerSnapshotId,
            Guid nachfolgerSnapshotId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task VergleichSpeichernAsync(
            B56SnapshotVergleich vergleich,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task LebenszyklusSpeichernAsync(
            B56ImportEintrag eintrag,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
