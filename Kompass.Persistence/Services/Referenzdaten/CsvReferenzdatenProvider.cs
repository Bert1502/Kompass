using Kompass.Application.Referenzdaten;
using Kompass.Domain.Referenzdaten;
using Microsoft.Extensions.Options;

namespace Kompass.Persistence.Services.Referenzdaten;

public sealed class CsvReferenzdatenProvider : IReferenzdatenProvider
{
    private readonly ReferenzdatenProviderOptionen _optionen;

    public CsvReferenzdatenProvider(
        IOptions<ReferenzdatenProviderOptionen> optionen)
    {
        _optionen = optionen.Value;
    }

    public string ProviderName => "csv";

    public async Task<IReadOnlyList<ReferenzdatenImportEintrag>> LadeReferenzdatenAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_optionen.CsvDateiPfad) || !File.Exists(_optionen.CsvDateiPfad))
        {
            return [];
        }

        var lines = await File.ReadAllLinesAsync(_optionen.CsvDateiPfad, cancellationToken);

        if (lines.Length < 2)
        {
            return [];
        }

        var delimiter = lines[0].Contains(';') ? ';' : ',';
        var headers = lines[0].Split(delimiter).Select(Normalize).ToArray();
        var result = new List<ReferenzdatenImportEintrag>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = line.Split(delimiter);
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < headers.Length; i++)
            {
                dict[headers[i]] = i < values.Length ? values[i].Trim().Trim('"') : null;
            }

            result.Add(ReferenzdatenImportParser.Parse(dict, ReferenzdatenImportart.DateiImport));
        }

        return result;
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
