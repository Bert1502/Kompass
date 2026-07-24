using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kompass.Application.B56Import;
using Kompass.Persistence;
using Kompass.Persistence.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kompass.Tests.B56Import;

public sealed class B56ImportServiceIntegrationTests
{
    [Fact]
    public async Task Import_verhindert_Dublette_nur_innerhalb_desselben_Projekts()
    {
        var testverzeichnis =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-integration-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            testverzeichnis);

        var quelldatei =
            Path.Combine(
                testverzeichnis,
                "b56-test.xlsx");

        var datenbankpfad =
            Path.Combine(
                testverzeichnis,
                "kompass.db");

        var archivverzeichnis =
            Path.Combine(
                testverzeichnis,
                "archiv");

        try
        {
            ErzeugeArbeitsmappe(
                quelldatei);

            var configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:KompassDatabase"] =
                                $"Data Source={datenbankpfad}",
                            ["B56Import:ArchivBasisverzeichnis"] =
                                archivverzeichnis,
                            ["B56Import:ErlaubteDateiendungen:0"] =
                                ".xlsx",
                            ["B56Import:MaximaleDateigroesseBytes"] =
                                "1048576"
                        })
                    .Build();

            var services =
                new ServiceCollection();

            services.AddPersistence(
                configuration);

            services.AddB56Import(
                configuration);

            await using var serviceProvider =
                services.BuildServiceProvider();

            await using var scope =
                serviceProvider.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<KompassDbContext>();

            await dbContext.Database.MigrateAsync();

            var importService =
                scope.ServiceProvider
                    .GetRequiredService<IB56ImportService>();

            var importRegister =
                scope.ServiceProvider
                    .GetRequiredService<IB56ImportRegister>();

            var projektId =
                Guid.NewGuid();

            var ergebnis =
                await importService.ImportierenAsync(
                    new B56ImportAnfrage(
                        projektId,
                        "Integrationsprojekt",
                        quelldatei));

            var registereintraege =
                await importRegister
                    .AlleFuerProjektAbrufenAsync(
                        projektId);

            var zweitesErgebnis =
                await importService.ImportierenAsync(
                    new B56ImportAnfrage(
                        projektId,
                        "Integrationsprojekt",
                        quelldatei));

            var registereintraegeNachDublette =
                await importRegister
                    .AlleFuerProjektAbrufenAsync(
                        projektId);

            var archivdateien =
                Directory.GetFiles(
                    archivverzeichnis,
                    "*.xlsx",
                    SearchOption.AllDirectories);

            var anderesProjektId =
                Guid.NewGuid();

            var anderesProjektErgebnis =
                await importService.ImportierenAsync(
                    new B56ImportAnfrage(
                        anderesProjektId,
                        "Anderes Integrationsprojekt",
                        quelldatei));

            var andereProjektRegistereintraege =
                await importRegister
                    .AlleFuerProjektAbrufenAsync(
                        anderesProjektId);

            var archivdateienNachAnderemProjekt =
                Directory.GetFiles(
                    archivverzeichnis,
                    "*.xlsx",
                    SearchOption.AllDirectories);

            Assert.Equal(
                B56ImportStatus.Erfolgreich,
                ergebnis.Status);

            Assert.NotNull(
                ergebnis.PipelineErgebnis);

            Assert.Equal(
                1,
                ergebnis.PipelineErgebnis
                    .ImportierteArbeitsblaetter);

            Assert.Equal(
                0,
                ergebnis.PipelineErgebnis
                    .ImportierteTabellen);

            Assert.Equal(
                1,
                ergebnis.PipelineErgebnis
                    .ErkannteTabellen);

            Assert.Single(
                ergebnis.PipelineErgebnis.Warnungen);

            Assert.Single(
                registereintraege);

            Assert.True(
                File.Exists(
                    ergebnis.ImportEintrag?
                        .Archivdateipfad));

            Assert.Equal(
                B56ImportStatus.BereitsImportiert,
                zweitesErgebnis.Status);

            Assert.Equal(
                ergebnis.ImportEintrag?.ImportId,
                zweitesErgebnis.ImportEintrag?.ImportId);

            Assert.Single(
                registereintraegeNachDublette);

            Assert.Single(
                archivdateien);

            Assert.Equal(
                B56ImportStatus.Erfolgreich,
                anderesProjektErgebnis.Status);

            Assert.NotEqual(
                ergebnis.ImportEintrag?.ImportId,
                anderesProjektErgebnis.ImportEintrag?.ImportId);

            Assert.Single(
                andereProjektRegistereintraege);

            Assert.Equal(
                2,
                archivdateienNachAnderemProjekt.Length);
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
                    new Row(
                        new Cell
                        {
                            CellReference = "A1",
                            DataType = CellValues.String,
                            CellValue = new CellValue("Bauteil")
                        },
                        new Cell
                        {
                            CellReference = "B1",
                            DataType = CellValues.String,
                            CellValue = new CellValue("Fläche")
                        },
                        new Cell
                        {
                            CellReference = "C1",
                            DataType = CellValues.String,
                            CellValue = new CellValue("U-Wert")
                        })));

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
                Name = "B56"
            });

        workbookPart.Workbook.Save();
    }
}
