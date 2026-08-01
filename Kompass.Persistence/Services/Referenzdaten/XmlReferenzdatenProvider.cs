using System.Xml.Linq;
using Kompass.Application.Referenzdaten;
using Kompass.Domain.Referenzdaten;
using Microsoft.Extensions.Options;

namespace Kompass.Persistence.Services.Referenzdaten;

public sealed class XmlReferenzdatenProvider : IReferenzdatenProvider
{
    private readonly ReferenzdatenProviderOptionen _optionen;

    public XmlReferenzdatenProvider(
        IOptions<ReferenzdatenProviderOptionen> optionen)
    {
        _optionen = optionen.Value;
    }

    public string ProviderName => "xml";

    public Task<IReadOnlyList<ReferenzdatenImportEintrag>> LadeReferenzdatenAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_optionen.XmlDateiPfad) || !File.Exists(_optionen.XmlDateiPfad))
        {
            return Task.FromResult<IReadOnlyList<ReferenzdatenImportEintrag>>([]);
        }

        var doc = XDocument.Load(_optionen.XmlDateiPfad);
        var rows = doc.Root?.Elements("datensatz") ?? [];
        var result = new List<ReferenzdatenImportEintrag>();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dict = row.Elements()
                .ToDictionary(
                    element => element.Name.LocalName.Trim().ToLowerInvariant(),
                    element => (string?)element.Value,
                    StringComparer.OrdinalIgnoreCase);

            result.Add(ReferenzdatenImportParser.Parse(dict, ReferenzdatenImportart.DateiImport));
        }

        return Task.FromResult<IReadOnlyList<ReferenzdatenImportEintrag>>(result);
    }
}
