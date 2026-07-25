using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kompass.Api.B56Import;
using Kompass.Application.B56Import;
using Kompass.Application.Projects;
using Kompass.Persistence;
using Kompass.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kompass.Tests.B56Import;

public sealed class B56ImportEndToEndSmokeTests
{
    [Fact]
    public async Task Upload_Persistenz_Historie_und_Detailabruf_funktionieren_durchgaengig()
    {
        var testverzeichnis =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-e2e-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            testverzeichnis);

        var quelldatei =
            Path.Combine(
                testverzeichnis,
                "anonymisierter-b56-export.xlsx");

        var projekt =
            new ProjektUebersicht(
                Guid.NewGuid(),
                "B56-End-to-End-Testprojekt",
                0);

        var configuration =
            ErzeugeKonfiguration(
                testverzeichnis);

        try
        {
            ErzeugeArbeitsmappe(
                quelldatei);

            var services =
                new ServiceCollection();

            services.AddPersistence(
                configuration);

            services.AddB56Import(
                configuration);

            await using var serviceProvider =
                services.BuildServiceProvider();

            Guid importId;

            await using (var importScope =
                serviceProvider.CreateAsyncScope())
            {
                var dbContext =
                    importScope.ServiceProvider
                        .GetRequiredService<KompassDbContext>();

                await dbContext.Database.MigrateAsync();

                var controller =
                    ErzeugeController(
                        importScope.ServiceProvider,
                        projekt);

                await using var uploadStream =
                    File.OpenRead(
                        quelldatei);

                var upload =
                    new FormFile(
                        uploadStream,
                        0,
                        uploadStream.Length,
                        "datei",
                        Path.GetFileName(
                            quelldatei));

                var importErgebnis =
                    await controller.ImportierenAsync(
                        projekt.Id,
                        upload,
                        CancellationToken.None);

                var created =
                    Assert.IsType<ObjectResult>(
                        importErgebnis.Result);

                Assert.Equal(
                    StatusCodes.Status201Created,
                    created.StatusCode);

                var antwort =
                    Assert.IsType<B56ImportAntwort>(
                        created.Value);

                Assert.Equal(
                    B56ImportStatus.Erfolgreich,
                    antwort.Status);

                Assert.NotNull(
                    antwort.ImportId);

                Assert.NotNull(
                    antwort.Pipeline);

                Assert.Single(
                    antwort.Pipeline.Bestandskennwerte);

                Assert.Single(
                    antwort.Pipeline.Modernisierungsalternativen);

                Assert.Single(
                    antwort.Pipeline.Bauteile);

                importId =
                    antwort.ImportId.Value;
            }

            await using (var leseScope =
                serviceProvider.CreateAsyncScope())
            {
                var controller =
                    ErzeugeController(
                        leseScope.ServiceProvider,
                        projekt);

                var historieErgebnis =
                    await controller.HistorieAbrufenAsync(
                        projekt.Id,
                        CancellationToken.None);

                var historieOk =
                    Assert.IsType<OkObjectResult>(
                        historieErgebnis.Result);

                var historie =
                    Assert.IsAssignableFrom<
                        IEnumerable<B56ImportHistorieAntwort>>(
                        historieOk.Value);

                var eintrag =
                    Assert.Single(
                        historie);

                Assert.Equal(
                    importId,
                    eintrag.ImportId);

                Assert.Equal(
                    Path.GetFileName(
                        quelldatei),
                    eintrag.Originaldateiname);

                var detailsErgebnis =
                    await controller.DetailsAbrufenAsync(
                        projekt.Id,
                        importId,
                        CancellationToken.None);

                var detailsOk =
                    Assert.IsType<OkObjectResult>(
                        detailsErgebnis.Result);

                var details =
                    Assert.IsType<B56ImportPipelineAntwort>(
                        detailsOk.Value);

                Assert.Equal(
                    "200",
                    details.Bestandskennwerte.Single().Wert.ToString());

                Assert.Equal(
                    "Gesamtpaket",
                    details.Modernisierungsalternativen
                        .Single()
                        .Bezeichnung);

                Assert.Equal(
                    "AW01",
                    details.Bauteile
                        .Single()
                        .Bauteilcode);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(
                    testverzeichnis))
            {
                Directory.Delete(
                    testverzeichnis,
                    recursive: true);
            }
        }
    }

    private static IConfiguration ErzeugeKonfiguration(
        string testverzeichnis)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:KompassDatabase"] =
                        $"Data Source={Path.Combine(testverzeichnis, "kompass.db")}",
                    ["B56Import:ArchivBasisverzeichnis"] =
                        Path.Combine(
                            testverzeichnis,
                            "archiv"),
                    ["B56Import:ErlaubteDateiendungen:0"] =
                        ".xlsx",
                    ["B56Import:MaximaleDateigroesseBytes"] =
                        "1048576"
                })
            .Build();
    }

    private static B56ImportController ErzeugeController(
        IServiceProvider serviceProvider,
        ProjektUebersicht projekt)
    {
        return new B56ImportController(
            new ProjektServiceFake(
                projekt),
            serviceProvider.GetRequiredService<IB56ImportService>(),
            serviceProvider.GetRequiredService<IB56ImportRegister>());
    }

    private static void ErzeugeArbeitsmappe(
        string dateipfad)
    {
        using var dokument =
            SpreadsheetDocument.Create(
                dateipfad,
                SpreadsheetDocumentType.Workbook);

        var workbookPart =
            dokument.AddWorkbookPart();

        workbookPart.Workbook =
            new Workbook();

        var worksheetPart =
            workbookPart.AddNewPart<WorksheetPart>();

        worksheetPart.Worksheet =
            new Worksheet(
                new SheetData(
                    Zeile(
                        4,
                        ("A", "Modernisierung in einem Zug")),
                    Zeile(
                        5,
                        ("B", "Bezeichnung"),
                        ("C", "Gesamtpaket")),
                    Zeile(
                        8,
                        ("B", "Primärenergiebedarf Gebäude"),
                        ("C", "100")),
                    Zeile(
                        227,
                        ("A", "Bestand")),
                    Zeile(
                        228,
                        ("B", "Primärenergiebedarf Gebäude"),
                        ("C", "200")),
                    Zeile(
                        245,
                        ("A", "Tabelle U-Werte der Bauteile")),
                    Zeile(
                        247,
                        ("B", "Bauteilcode"),
                        ("C", "Bauteil"),
                        ("D", "Nachbarseite"),
                        ("E", "U-Wert")),
                    Zeile(
                        249,
                        ("B", "AW01"),
                        ("C", "Außenwand"),
                        ("D", "gegen Außenluft"),
                        ("E", "0.24"))));

        var sheets =
            workbookPart.Workbook.AppendChild(
                new Sheets());

        sheets.Append(
            new Sheet
            {
                Id =
                    workbookPart.GetIdOfPart(
                        worksheetPart),
                SheetId = 1,
                Name = "SCModernisierungen"
            });

        workbookPart.Workbook.Save();
    }

    private static Row Zeile(
        uint zeilennummer,
        params (string Spalte, string Wert)[] zellen)
    {
        return new Row(
            zellen.Select(
                zelle =>
                    new Cell
                    {
                        CellReference =
                            $"{zelle.Spalte}{zeilennummer}",
                        DataType =
                            CellValues.String,
                        CellValue =
                            new CellValue(
                                zelle.Wert)
                    }))
        {
            RowIndex =
                zeilennummer
        };
    }

    private sealed class ProjektServiceFake(
        ProjektUebersicht projekt)
        : IProjektService
    {
        public Task<ProjektUebersicht?> NachIdAbrufenAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ProjektUebersicht?>(
                id == projekt.Id
                    ? projekt
                    : null);
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
}
