using System.Text.Json;
using Kompass.Application.Referenzdaten;
using Kompass.Domain.Referenzdaten;
using Microsoft.Extensions.Options;

namespace Kompass.Persistence.Services.Referenzdaten;

public sealed class JsonReferenzdatenProvider : IReferenzdatenProvider
{
    private readonly ReferenzdatenProviderOptionen _optionen;

    public JsonReferenzdatenProvider(
        IOptions<ReferenzdatenProviderOptionen> optionen)
    {
        _optionen = optionen.Value;
    }

    public string ProviderName => "json";

    public async Task<IReadOnlyList<ReferenzdatenImportEintrag>> LadeReferenzdatenAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_optionen.JsonDateiPfad) || !File.Exists(_optionen.JsonDateiPfad))
        {
            return [];
        }

        await using var stream = File.OpenRead(_optionen.JsonDateiPfad);

        var entries = await JsonSerializer.DeserializeAsync<List<Dictionary<string, string?>>(
            stream,
            cancellationToken: cancellationToken);

        if (entries is null)
        {
            return [];
        }

        return entries
            .Select(row => ReferenzdatenImportParser.Parse(Normalize(row), ReferenzdatenImportart.DateiImport))
            .ToList();
    }

    private static IReadOnlyDictionary<string, string?> Normalize(Dictionary<string, string?> values)
    {
        return values.ToDictionary(
            item => item.Key.Trim().ToLowerInvariant(),
            item => item.Value,
            StringComparer.OrdinalIgnoreCase);
    }
}
