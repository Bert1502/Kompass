using System.Globalization;
using Kompass.Application.Referenzdaten;
using Kompass.Domain.Referenzdaten;

namespace Kompass.Persistence.Services.Referenzdaten;

internal static class ReferenzdatenImportParser
{
    public static ReferenzdatenImportEintrag Parse(
        IReadOnlyDictionary<string, string?> values,
        ReferenzdatenImportart defaultImportart)
    {
        var fachlicheBezeichnung = Pflicht(values, "fachlichebezeichnung");
        var parameterart = Pflicht(values, "parameterart");
        var wert = Pflicht(values, "wert");
        var ebene = ParseEnum(values, "ebene", ReferenzdatenEbene.Systemweit);
        var quelle = Pflicht(values, "quelle");
        var herausgeber = Pflicht(values, "herausgeber");
        var quellenVerweis = Pflicht(values, "quellenverweis");
        var gueltigAb = ParseDateOnly(values, "gueltigab") ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var gueltigBis = ParseDateOnly(values, "gueltigbis");
        var versionsstand = Wert(values, "versionsstand") ?? "1";
        var datenstatus = ParseEnum(values, "datenstatus", ReferenzdatenStatus.Freigegeben);
        var qualitaetsstatus = ParseEnum(values, "qualitaetsstatus", Qualitaetsstatus.NichtVerifiziert);
        var importart = ParseEnum(values, "importart", defaultImportart);
        var letzteAktualisierung = ParseDateTimeOffset(values, "letzteaktualisierungutc") ?? DateTimeOffset.UtcNow;

        return new ReferenzdatenImportEintrag(
            fachlicheBezeichnung,
            parameterart,
            wert,
            ebene,
            quelle,
            herausgeber,
            quellenVerweis,
            gueltigAb,
            gueltigBis,
            versionsstand,
            datenstatus,
            qualitaetsstatus,
            importart,
            letzteAktualisierung,
            Einheit: Wert(values, "einheit"),
            Bezugsgroesse: Wert(values, "bezugsgroesse"),
            EnergietraegerOderKategorie: Wert(values, "energietraegeroderkategorie"),
            Veroeffentlichungsdatum: ParseDateOnly(values, "veroeffentlichungsdatum"),
            Abrufdatum: ParseDateOnly(values, "abrufdatum"),
            ProjektId: ParseGuid(values, "projektid"),
            UnternehmenId: ParseGuid(values, "unternehmenid"));
    }

    private static string Pflicht(IReadOnlyDictionary<string, string?> values, string key)
    {
        var value = Wert(values, key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Pflichtfeld '{key}' fehlt.");
        }

        return value;
    }

    private static string? Wert(IReadOnlyDictionary<string, string?> values, string key)
    {
        return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;
    }

    private static T ParseEnum<T>(IReadOnlyDictionary<string, string?> values, string key, T fallback)
        where T : struct, Enum
    {
        var raw = Wert(values, key);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        raw = raw
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal);

        foreach (var name in Enum.GetNames<T>())
        {
            var normalized = name
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal);

            if (string.Equals(normalized, raw, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.Parse<T>(name);
            }
        }

        return fallback;
    }

    private static DateOnly? ParseDateOnly(IReadOnlyDictionary<string, string?> values, string key)
    {
        var raw = Wert(values, key);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            return value;
        }

        return null;
    }

    private static DateTimeOffset? ParseDateTimeOffset(IReadOnlyDictionary<string, string?> values, string key)
    {
        var raw = Wert(values, key);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value))
        {
            return value;
        }

        return null;
    }

    private static Guid? ParseGuid(IReadOnlyDictionary<string, string?> values, string key)
    {
        var raw = Wert(values, key);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return Guid.TryParse(raw, out var value)
            ? value
            : null;
    }
}
