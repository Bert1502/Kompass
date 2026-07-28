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
    public async Task Vollstaendiger_Lebenszyklus_Upload_Bestaetigen_Uebernehmen_ReImport_Vergleich()
    {
        var testverzeichnis =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-lifecycle-{Guid.NewGuid():N}");

        Directory.CreateDirectory(testverzeichnis);

        try
        {
            await using var factory =
                new B56ApiFactory(testverzeichnis);

            using var client = factory.CreateClient();

            // 1. Projekt anlegen
            var projektAntwort =
                await client.PostAsJsonAsync(
                    "/api/projekte",
                    new { Name = "Lebenszyklus-Testprojekt" });

            Assert.Equal(
                HttpStatusCode.Created,
                projektAntwort.StatusCode);

            var projektId =
                (await JsonDocument.ParseAsync(
                    await projektAntwort.Content.ReadAsStreamAsync()))
                    .RootElement.GetProperty("id").GetGuid();

            // 2. Erste Datei hochladen
            var ersterImportId =
                await HochladenAsync(
                    client,
                    projektId,
                    uWert: 0.24,
                    dateiname: "b56-v1.xlsx");

            using var erstesImportJson =
                await JsonDocument.ParseAsync(
                    await (await client.GetAsync(
                            $"/api/projekte/{projektId}/b56-importe/{ersterImportId}"))
                        .Content.ReadAsStreamAsync());

            Assert.Single(
                erstesImportJson.RootElement
                    .GetProperty("modernisierungsalternativen")
                    .EnumerateArray());

            // 3. Snapshot anzeigen – Status ist TechnischGeprueft
            using var historieJson =
                await JsonDocument.ParseAsync(
                    await (await client.GetAsync(
                            $"/api/projekte/{projektId}/b56-importe"))
                        .Content.ReadAsStreamAsync());

            var ersterEintrag =
                Assert.Single(
                    historieJson.RootElement.EnumerateArray());

            Assert.Equal(
                (int)B56SnapshotStatus.TechnischGeprueft,
                ersterEintrag.GetProperty("snapshotStatus").GetInt32());

            // 4. Import bestätigen
            var bestaetigtAntwort =
                await client.PostAsync(
                    $"/api/projekte/{projektId}/b56-importe/{ersterImportId}/bestaetigen",
                    null);

            Assert.Equal(
                HttpStatusCode.OK,
                bestaetigtAntwort.StatusCode);

            using var bestaetigtJson =
                await JsonDocument.ParseAsync(
                    await bestaetigtAntwort.Content.ReadAsStreamAsync());

            Assert.Equal(
                (int)B56SnapshotStatus.FachlichBestaetigt,
                bestaetigtJson.RootElement
                    .GetProperty("snapshotStatus").GetInt32());

            // 5. Projektmodell erzeugen
            var uebernahmeAntwort =
                await client.PostAsync(
                    $"/api/projekte/{projektId}/b56-importe/{ersterImportId}/in-projektmodell-uebernehmen",
                    null);

            Assert.Equal(
                HttpStatusCode.OK,
                uebernahmeAntwort.StatusCode);

            using var uebernahmeJson =
                await JsonDocument.ParseAsync(
                    await uebernahmeAntwort.Content.ReadAsStreamAsync());

            Assert.Equal(
                1,
                uebernahmeJson.RootElement
                    .GetProperty("uebernommeneAlternativen").GetInt32());

            // 6. Snapshot ist jetzt InProjektmodellUebernommen
            using var historieNachUebernahmeJson =
                await JsonDocument.ParseAsync(
                    await (await client.GetAsync(
                            $"/api/projekte/{projektId}/b56-importe"))
                        .Content.ReadAsStreamAsync());

            var eintragNachUebernahme =
                Assert.Single(
                    historieNachUebernahmeJson.RootElement.EnumerateArray());

            Assert.Equal(
                (int)B56SnapshotStatus.InProjektmodellUebernommen,
                eintragNachUebernahme.GetProperty("snapshotStatus").GetInt32());

            // 7. Idempotenz – zweite Übernahme liefert ebenfalls Erfolgreich
            var zweiteUebernahme =
                await client.PostAsync(
                    $"/api/projekte/{projektId}/b56-importe/{ersterImportId}/in-projektmodell-uebernehmen",
                    null);

            Assert.Equal(
                HttpStatusCode.OK,
                zweiteUebernahme.StatusCode);

            // 8. Zweiten Snapshot mit geändertem U-Wert importieren
            var zweiterImportId =
                await HochladenAsync(
                    client,
                    projektId,
                    uWert: 0.18,
                    dateiname: "b56-v2.xlsx");

            // 9. Unterschiede anzeigen
            var vergleichAntwort =
                await client.GetAsync(
                    $"/api/projekte/{projektId}/b56-importe/vergleich" +
                    $"?altSnapshotId={ersterImportId}" +
                    $"&neuSnapshotId={zweiterImportId}");

            Assert.Equal(
                HttpStatusCode.OK,
                vergleichAntwort.StatusCode);

            using var vergleichJson =
                await JsonDocument.ParseAsync(
                    await vergleichAntwort.Content.ReadAsStreamAsync());

            var bauteile =
                vergleichJson.RootElement
                    .GetProperty("bauteile")
                    .EnumerateArray()
                    .ToList();

            Assert.Single(bauteile);
            Assert.Equal(
                (int)B56VergleichsArt.Geaendert,
                bauteile[0].GetProperty("art").GetInt32());
            Assert.Equal(
                0.24,
                bauteile[0].GetProperty("alterUWert").GetDouble(),
                precision: 9);
            Assert.Equal(
                0.18,
                bauteile[0].GetProperty("neuerUWert").GetDouble(),
                precision: 9);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(testverzeichnis))
            {
                Directory.Delete(testverzeichnis, recursive: true);
            }
        }
    }

    private static async Task<Guid> HochladenAsync(
        HttpClient client,
        Guid projektId,
        double uWert,
        string dateiname)
    {
        using var upload = new MultipartFormDataContent();
        using var dateiinhalt =
            new ByteArrayContent(
                ErzeugeB56ArbeitsmappeWithUWert(uWert));

        upload.Add(dateiinhalt, "datei", dateiname);

        var importAntwort =
            await client.PostAsync(
                $"/api/projekte/{projektId}/b56-importe",
                upload);

        Assert.Equal(HttpStatusCode.Created, importAntwort.StatusCode);

        using var importJson =
            await JsonDocument.ParseAsync(
                await importAntwort.Content.ReadAsStreamAsync());

        return importJson.RootElement.GetProperty("importId").GetGuid();
    }

    private static byte[] ErzeugeB56ArbeitsmappeWithUWert(double uWert)
    {
        using var stream = new MemoryStream();

        using (var dokument =
            SpreadsheetDocument.Create(
                stream,
                SpreadsheetDocumentType.Workbook,
                autoSave: true))
        {
            var workbookPart = dokument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart =
                workbookPart.AddNewPart<WorksheetPart>();

            worksheetPart.Worksheet =
                new Worksheet(
                    new SheetData(
                        Zeile(4, ("A", "Modernisierung in einem Zug")),
                        Zeile(5, ("B", "Bezeichnung"), ("C", "Gesamtpaket")),
                        Zeile(8, ("B", "Primärenergiebedarf Gebäude"), ("C", "100")),
                        Zeile(227, ("A", "Bestand")),
                        Zeile(228, ("B", "Primärenergiebedarf Gebäude"), ("C", "200")),
                        Zeile(245, ("A", "Tabelle U-Werte der Bauteile")),
                        Zeile(247,
                            ("B", "Bauteilcode"),
                            ("C", "Bauteil"),
                            ("D", "Nachbarseite"),
                            ("E", "U-Wert")),
                        Zeile(249,
                            ("B", "AW01"),
                            ("C", "Außenwand"),
                            ("D", "gegen Außenluft"),
                            ("E", uWert.ToString("F3",
                                System.Globalization.CultureInfo.InvariantCulture)))));

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(
                new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "SCModernisierungen"
                });

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

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

    [Fact]
    public async Task Http_Vertrag_zweiter_Import_und_Vergleich_zeigen_Aenderungen()
    {
        var testverzeichnis =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-vergleich-{Guid.NewGuid():N}");

        Directory.CreateDirectory(
            testverzeichnis);

        try
        {
            await using var factory =
                new B56ApiFactory(
                    testverzeichnis);

            using var client =
                factory.CreateClient();

            // Projekt anlegen
            var projektAntwort =
                await client.PostAsJsonAsync(
                    "/api/projekte",
                    new { Name = "Vergleich-Testprojekt" });

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

            // Ersten Import hochladen
            using var upload1 =
                new MultipartFormDataContent();

            upload1.Add(
                new ByteArrayContent(
                    ErzeugeB56Arbeitsmappe()),
                "datei",
                "b56-v1.xlsx");

            var import1Antwort =
                await client.PostAsync(
                    $"/api/projekte/{projektId}/b56-importe",
                    upload1);

            Assert.Equal(
                HttpStatusCode.Created,
                import1Antwort.StatusCode);

            using var import1Json =
                await JsonDocument.ParseAsync(
                    await import1Antwort.Content.ReadAsStreamAsync());

            var import1Id =
                import1Json.RootElement
                    .GetProperty("importId")
                    .GetGuid();

            // Zweiten Import mit geändertem Kennwert hochladen
            using var upload2 =
                new MultipartFormDataContent();

            upload2.Add(
                new ByteArrayContent(
                    ErzeugeB56ArbeitsmappeGeaendert()),
                "datei",
                "b56-v2.xlsx");

            var import2Antwort =
                await client.PostAsync(
                    $"/api/projekte/{projektId}/b56-importe",
                    upload2);

            Assert.Equal(
                HttpStatusCode.Created,
                import2Antwort.StatusCode);

            using var import2Json =
                await JsonDocument.ParseAsync(
                    await import2Antwort.Content.ReadAsStreamAsync());

            var import2Id =
                import2Json.RootElement
                    .GetProperty("importId")
                    .GetGuid();

            // Vergleich abrufen
            var vergleichAntwort =
                await client.GetAsync(
                    $"/api/projekte/{projektId}/b56-importe/{import2Id}/vergleich?vorgaenger={import1Id}");

            vergleichAntwort.EnsureSuccessStatusCode();

            using var vergleichJson =
                await JsonDocument.ParseAsync(
                    await vergleichAntwort.Content.ReadAsStreamAsync());

            Assert.Equal(
                projektId,
                vergleichJson.RootElement
                    .GetProperty("projektId")
                    .GetGuid());

            Assert.Equal(
                import1Id,
                vergleichJson.RootElement
                    .GetProperty("vorgaengerSnapshotId")
                    .GetGuid());

            Assert.Equal(
                import2Id,
                vergleichJson.RootElement
                    .GetProperty("nachfolgerSnapshotId")
                    .GetGuid());

            Assert.True(
                vergleichJson.RootElement
                    .GetProperty("hatAenderungen")
                    .GetBoolean());

            // Der Bestandskennwert hat sich geändert (200 → 180)
            var kennwerte =
                vergleichJson.RootElement
                    .GetProperty("bestandskennwertVergleiche")
                    .EnumerateArray()
                    .ToList();

            var geaendert =
                Assert.Single(
                    kennwerte,
                    k =>
                        k.GetProperty("name").GetString() ==
                            "Primärenergiebedarf Gebäude" &&
                        k.GetProperty("aenderung").GetInt32() == 1);

            Assert.Equal(
                200,
                geaendert
                    .GetProperty("alterWert")
                    .GetDouble());

            Assert.Equal(
                180,
                geaendert
                    .GetProperty("neuerWert")
                    .GetDouble());

            // Fehlende Vorgänger-Snapshot-ID liefert 404
            var fehlendeAntwort =
                await client.GetAsync(
                    $"/api/projekte/{projektId}/b56-importe/{import2Id}/vergleich?vorgaenger={Guid.NewGuid()}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                fehlendeAntwort.StatusCode);
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

    private static byte[] ErzeugeB56ArbeitsmappeGeaendert()
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

            // Gleiche Struktur wie ErzeugeB56Arbeitsmappe,
            // aber Bestandskennwert 180 statt 200 → anderer Hash
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
                            ("C", "180")),
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
