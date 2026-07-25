using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kompass.Application.B56Import;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Kompass.Tests.B56Import;

public sealed class B56ImportHttpEndToEndTests
{
    [Fact]
    public async Task Http_Vertrag_verbindet_Projekt_Upload_Historie_und_Details()
    {
        var testverzeichnis =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-http-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            testverzeichnis);

        try
        {
            await using var factory =
                new B56ApiFactory(
                    testverzeichnis);

            using var client =
                factory.CreateClient();

            var projektAntwort =
                await client.PostAsJsonAsync(
                    "/api/projekte",
                    new
                    {
                        Name = "HTTP-Testprojekt"
                    });

            Assert.Equal(
                HttpStatusCode.Created,
                projektAntwort.StatusCode);

            using var projektJson =
                await JsonDocument.ParseAsync(
                    await projektAntwort.Content.ReadAsStreamAsync());

            var projektId =
                projektJson.RootElement
                    .GetProperty("id")
                    .GetGuid();

            using var upload =
                new MultipartFormDataContent();

            using var dateiinhalt =
                new ByteArrayContent(
                    ErzeugeB56Arbeitsmappe());

            upload.Add(
                dateiinhalt,
                "datei",
                "b56-http-test.xlsx");

            var importAntwort =
                await client.PostAsync(
                    $"/api/projekte/{projektId}/b56-importe",
                    upload);

            Assert.Equal(
                HttpStatusCode.Created,
                importAntwort.StatusCode);

            using var importJson =
                await JsonDocument.ParseAsync(
                    await importAntwort.Content.ReadAsStreamAsync());

            var importId =
                importJson.RootElement
                    .GetProperty("importId")
                    .GetGuid();

            Assert.Equal(
                B56SnapshotVersionen.AktuelleSchemaVersion,
                importJson.RootElement
                    .GetProperty("snapshotSchemaVersion")
                    .GetInt32());
            Assert.Equal(
                B56SnapshotVersionen.AktuelleParserVersion,
                importJson.RootElement
                    .GetProperty("parserVersion")
                    .GetString());

            var historieAntwort =
                await client.GetAsync(
                    $"/api/projekte/{projektId}/b56-importe");

            historieAntwort.EnsureSuccessStatusCode();

            using var historieJson =
                await JsonDocument.ParseAsync(
                    await historieAntwort.Content.ReadAsStreamAsync());

            var historieEintrag =
                Assert.Single(
                    historieJson.RootElement.EnumerateArray());

            Assert.Equal(
                importId,
                historieEintrag
                    .GetProperty("importId")
                    .GetGuid());
            Assert.False(
                historieEintrag.TryGetProperty(
                    "archivdateipfad",
                    out _));

            var detailsAntwort =
                await client.GetAsync(
                    $"/api/projekte/{projektId}/b56-importe/{importId}");

            detailsAntwort.EnsureSuccessStatusCode();

            using var detailsJson =
                await JsonDocument.ParseAsync(
                    await detailsAntwort.Content.ReadAsStreamAsync());

            Assert.Single(
                detailsJson.RootElement
                    .GetProperty("bestandskennwerte")
                    .EnumerateArray());
            Assert.Single(
                detailsJson.RootElement
                    .GetProperty("modernisierungsalternativen")
                    .EnumerateArray());
            Assert.Single(
                detailsJson.RootElement
                    .GetProperty("bauteile")
                    .EnumerateArray());

            Assert.True(
                File.Exists(
                    Path.Combine(
                        testverzeichnis,
                        "kompass.db")));
            Assert.Single(
                Directory.GetFiles(
                    Path.Combine(
                        testverzeichnis,
                        "archiv"),
                    "*.xlsx",
                    SearchOption.AllDirectories));
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

    private static byte[] ErzeugeB56Arbeitsmappe()
    {
        using var stream =
            new MemoryStream();

        using (var dokument =
            SpreadsheetDocument.Create(
                stream,
                SpreadsheetDocumentType.Workbook,
                autoSave: true))
        {
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

        return stream.ToArray();
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

    private sealed class B56ApiFactory(
        string testverzeichnis)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:KompassDatabase",
                $"Data Source={Path.Combine(testverzeichnis, "kompass.db")}");
            builder.UseSetting(
                "B56Import:ArchivBasisverzeichnis",
                Path.Combine(
                    testverzeichnis,
                    "archiv"));
            builder.UseSetting(
                "B56Import:ErlaubteDateiendungen:0",
                ".xlsx");
            builder.UseSetting(
                "B56Import:ErlaubteDateiendungen:1",
                ".xlsm");
            builder.UseSetting(
                "B56Import:MaximaleDateigroesseBytes",
                "1048576");
        }
    }
}
