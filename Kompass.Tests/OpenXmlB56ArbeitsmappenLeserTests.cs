using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kompass.Persistence.Services;

namespace Kompass.Tests.B56Import;

public sealed class OpenXmlB56ArbeitsmappenLeserTests
{
    [Fact]
    public async Task Datei_nicht_vorhanden_wirft_Exception()
    {
        var leser =
            new OpenXmlB56ArbeitsmappenLeser();

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => leser.LesenAsync(@"C:\Test\ExistiertNicht.xlsx"));
    }

    [Fact]
    public async Task Liest_unterschiedliche_Zelltypen_mit_Adresse_und_Spalte()
    {
        var dateipfad =
            Path.Combine(
                Path.GetTempPath(),
                $"kompass-b56-zelltypen-{Guid.NewGuid():N}.xlsx");

        try
        {
            ErzeugeArbeitsmappe(
                dateipfad);

            var leser =
                new OpenXmlB56ArbeitsmappenLeser();

            var arbeitsmappe =
                await leser.LesenAsync(
                    dateipfad);

            var arbeitsblatt =
                Assert.Single(
                    arbeitsmappe.Arbeitsblaetter);

            Assert.Equal(
                "B56",
                arbeitsblatt.Name);

            var zeile =
                Assert.Single(
                    arbeitsblatt.Zeilen);

            Assert.Equal(
                1,
                zeile.Zeilennummer);

            Assert.Equal(
                ["Bauteil", "Fläche", "Ja", "12.5"],
                zeile.Zellen.Select(
                    zelle => zelle.Wert));

            Assert.Equal(
                ["A1", "B1", "C1", "D1"],
                zeile.Zellen.Select(
                    zelle => zelle.Adresse));

            Assert.Equal(
                ["A", "B", "C", "D"],
                zeile.Zellen.Select(
                    zelle => zelle.Spalte));
        }
        finally
        {
            if (File.Exists(
                    dateipfad))
            {
                File.Delete(
                    dateipfad);
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

        var sharedStringPart =
            workbookPart.AddNewPart<SharedStringTablePart>();

        sharedStringPart.SharedStringTable =
            new SharedStringTable(
                new SharedStringItem(
                    new Text("Bauteil")));

        var worksheetPart =
            workbookPart.AddNewPart<WorksheetPart>();

        worksheetPart.Worksheet =
            new Worksheet(
                new SheetData(
                    new Row(
                        new Cell
                        {
                            CellReference = "A1",
                            DataType = CellValues.SharedString,
                            CellValue = new CellValue("0")
                        },
                        new Cell
                        {
                            CellReference = "B1",
                            DataType = CellValues.InlineString,
                            InlineString =
                                new InlineString(
                                    new Text("Fläche"))
                        },
                        new Cell
                        {
                            CellReference = "C1",
                            DataType = CellValues.Boolean,
                            CellValue = new CellValue("1")
                        },
                        new Cell
                        {
                            CellReference = "D1",
                            DataType = CellValues.Number,
                            CellValue = new CellValue("12.5")
                        })
                    {
                        RowIndex = 1
                    }));

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
