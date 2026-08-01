using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kompass.Application.Referenzdaten;
using Kompass.Domain.Referenzdaten;
using Microsoft.Extensions.Options;

namespace Kompass.Persistence.Services.Referenzdaten;

public sealed class ExcelReferenzdatenProvider : IReferenzdatenProvider
{
    private readonly ReferenzdatenProviderOptionen _optionen;

    public ExcelReferenzdatenProvider(
        IOptions<ReferenzdatenProviderOptionen> optionen)
    {
        _optionen = optionen.Value;
    }

    public string ProviderName => "excel";

    public Task<IReadOnlyList<ReferenzdatenImportEintrag>> LadeReferenzdatenAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_optionen.ExcelDateiPfad) || !File.Exists(_optionen.ExcelDateiPfad))
        {
            return Task.FromResult<IReadOnlyList<ReferenzdatenImportEintrag>>([]);
        }

        using var document = SpreadsheetDocument.Open(_optionen.ExcelDateiPfad, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Ungültige Excel-Datei.");
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;
        var sheets = workbookPart.Workbook?.Sheets;
        var firstSheet = sheets?.Elements<Sheet>().FirstOrDefault();

        if (firstSheet is null)
        {
            return Task.FromResult<IReadOnlyList<ReferenzdatenImportEintrag>>([]);
        }

        var sheetId = firstSheet.Id?.Value;

        if (string.IsNullOrWhiteSpace(sheetId))
        {
            return Task.FromResult<IReadOnlyList<ReferenzdatenImportEintrag>>([]);
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheetId);
        var worksheet = worksheetPart.Worksheet;
        var rows = worksheet?.Descendants<Row>().ToList() ?? [];

        if (rows.Count < 2)
        {
            return Task.FromResult<IReadOnlyList<ReferenzdatenImportEintrag>>([]);
        }

        var headers = rows[0].Elements<Cell>().Select(cell => Normalize(ReadCell(cell, sharedStrings))).ToList();
        var result = new List<ReferenzdatenImportEintrag>();

        foreach (var row in rows.Skip(1))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cells = row.Elements<Cell>().ToList();
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < headers.Count; i++)
            {
                dict[headers[i]] = i < cells.Count ? ReadCell(cells[i], sharedStrings) : null;
            }

            result.Add(ReferenzdatenImportParser.Parse(dict, ReferenzdatenImportart.DateiImport));
        }

        return Task.FromResult<IReadOnlyList<ReferenzdatenImportEintrag>>(result);
    }

    private static string? ReadCell(Cell cell, SharedStringTable? sharedStrings)
    {
        var value = cell.CellValue?.InnerText;

        if (value is null)
        {
            return null;
        }

        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(value, out var index) &&
            sharedStrings is not null)
        {
            return sharedStrings.ElementAtOrDefault(index)?.InnerText;
        }

        return value;
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }
}
