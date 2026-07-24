using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kompass.Application.B56Import;

namespace Kompass.Persistence.Services;

public sealed class OpenXmlB56ArbeitsmappenLeser
    : IB56ArbeitsmappenLeser
{
    public Task<B56Arbeitsmappe> LesenAsync(
        string dateipfad,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dateipfad);

        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(dateipfad))
        {
            throw new FileNotFoundException(
                "Die Bilanzierungsdatei wurde nicht gefunden.",
                dateipfad);
        }

        var arbeitsblaetter =
            new List<B56Arbeitsblatt>();

        using var dokument =
            SpreadsheetDocument.Open(
                dateipfad,
                false);

        var workbookPart =
            dokument.WorkbookPart
            ?? throw new InvalidDataException(
                "Die Excel-Datei enthält keine Arbeitsmappe.");

        var workbook =
            workbookPart.Workbook
            ?? throw new InvalidDataException(
                "Die Excel-Datei enthält keine gültige Workbook-Struktur.");

        var sharedStrings =
            SharedStringsLesen(workbookPart);

        var sheets =
            workbook.Sheets?
                .Elements<Sheet>()
                .ToList()
            ?? new List<Sheet>();

        foreach (var sheet in sheets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sheetId = sheet.Id?.Value;

            if (string.IsNullOrWhiteSpace(sheetId))
            {
                continue;
            }

            var worksheetPart =
                workbookPart.GetPartById(sheetId)
                as WorksheetPart;

            if (worksheetPart is null)
            {
                continue;
            }

            var zeilen =
                ZeilenLesen(
                    worksheetPart,
                    sharedStrings,
                    cancellationToken);

            arbeitsblaetter.Add(
                new B56Arbeitsblatt
                {
                    Name = sheet.Name?.Value
                        ?? string.Empty,

                    Zeilen = zeilen
                });
        }

        var arbeitsmappe =
            new B56Arbeitsmappe
            {
                Dateipfad =
                    Path.GetFullPath(dateipfad),

                Arbeitsblaetter =
                    arbeitsblaetter
            };

        return Task.FromResult(arbeitsmappe);
    }

    private static IReadOnlyList<string> SharedStringsLesen(
        WorkbookPart workbookPart)
    {
        var sharedStringTable =
            workbookPart.SharedStringTablePart?
                .SharedStringTable;

        if (sharedStringTable is null)
        {
            return Array.Empty<string>();
        }

        return sharedStringTable
            .Elements<SharedStringItem>()
            .Select(x => x.InnerText)
            .ToList();
    }

    private static IReadOnlyList<B56Zeile> ZeilenLesen(
        WorksheetPart worksheetPart,
        IReadOnlyList<string> sharedStrings,
        CancellationToken cancellationToken)
    {
        var worksheet =
            worksheetPart.Worksheet;

        if (worksheet is null)
        {
            return Array.Empty<B56Zeile>();
        }

        var sheetData =
            worksheet.GetFirstChild<SheetData>();

        if (sheetData is null)
        {
            return Array.Empty<B56Zeile>();
        }

        var ergebnis =
            new List<B56Zeile>();

        foreach (var row in sheetData.Elements<Row>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var zellen =
                new List<B56Zelle>();

            foreach (var cell in row.Elements<Cell>())
            {
                var adresse =
                    cell.CellReference?.Value
                    ?? string.Empty;

                zellen.Add(
                    new B56Zelle
                    {
                        Adresse = adresse,
                        Spalte = SpalteErmitteln(adresse),
                        Zeile = checked(
                            (int)(row.RowIndex?.Value ?? 0)),
                        Wert = ZellwertLesen(
                            cell,
                            sharedStrings)
                    });
            }

            ergebnis.Add(
                new B56Zeile
                {
                    Zeilennummer = checked(
                        (int)(row.RowIndex?.Value ?? 0)),

                    Zellen = zellen
                });
        }

        return ergebnis;
    }

    private static string ZellwertLesen(
        Cell cell,
        IReadOnlyList<string> sharedStrings)
    {
        if (cell.DataType?.Value ==
            CellValues.InlineString)
        {
            return cell.InlineString?.InnerText
                ?? string.Empty;
        }

        var rohwert =
            cell.CellValue?.InnerText;

        if (string.IsNullOrWhiteSpace(rohwert))
        {
            return string.Empty;
        }

        if (cell.DataType?.Value ==
            CellValues.SharedString)
        {
            if (int.TryParse(
                    rohwert,
                    out var index) &&
                index >= 0 &&
                index < sharedStrings.Count)
            {
                return sharedStrings[index];
            }

            return string.Empty;
        }

        if (cell.DataType?.Value ==
            CellValues.Boolean)
        {
            return rohwert == "1"
                ? "Ja"
                : "Nein";
        }

        return rohwert;
    }

    private static string SpalteErmitteln(
        string zelladresse)
    {
        if (string.IsNullOrWhiteSpace(zelladresse))
        {
            return string.Empty;
        }

        return new string(
            zelladresse
                .TakeWhile(char.IsLetter)
                .ToArray());
    }
}