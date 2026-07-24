using System.Text.Json;
using Kompass.Application.B56Import;

namespace Kompass.Persistence.B56Import;

public sealed class JsonB56BauteilzuordnungsRepository
    : IB56BauteilzuordnungsRepository
{
    private readonly B56ImportOptionen _optionen;

    private IReadOnlyList<B56Bauteilzuordnung>? _cache;

    public JsonB56BauteilzuordnungsRepository(
        B56ImportOptionen optionen)
    {
        ArgumentNullException.ThrowIfNull(optionen);

        _optionen = optionen;
    }

    public IReadOnlyList<B56Bauteilzuordnung> Laden()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var dateipfad =
            ErmittleDateipfad();

        if (!File.Exists(dateipfad))
        {
            _cache = ErzeugeStandardzuordnungen();

            return _cache;
        }

        try
        {
            var json =
                File.ReadAllText(dateipfad);

            var zuordnungen =
                JsonSerializer.Deserialize<
                    List<B56Bauteilzuordnung>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            _cache =
                zuordnungen?
                    .Where(z =>
                        !string.IsNullOrWhiteSpace(z.Code))
                    .OrderByDescending(z => z.Prioritaet)
                    .ToList()
                ?? ErzeugeStandardzuordnungen();

            return _cache;
        }
        catch (
            JsonException)
        {
            _cache = ErzeugeStandardzuordnungen();

            return _cache;
        }
        catch (
            IOException)
        {
            _cache = ErzeugeStandardzuordnungen();

            return _cache;
        }
    }

    private string ErmittleDateipfad()
    {
        if (!string.IsNullOrWhiteSpace(
                _optionen.Bauteilzuordnungsdatei))
        {
            return Path.GetFullPath(
                _optionen.Bauteilzuordnungsdatei);
        }

        return Path.Combine(
            AppContext.BaseDirectory,
            "Konfiguration",
            "b56-bauteilzuordnungen.json");
    }

    private static IReadOnlyList<B56Bauteilzuordnung>
        ErzeugeStandardzuordnungen()
    {
        return
        [
            new()
            {
                Code = "AW",
                Kategorie = "Außenwand",
                KompassTyp = "Aussenwand",
                Beschreibung = "Außenwand",
                Prioritaet = 100
            },
            new()
            {
                Code = "IW",
                Kategorie = "Innenwand",
                KompassTyp = "Innenwand",
                Beschreibung = "Innenwand",
                Prioritaet = 90
            },
            new()
            {
                Code = "DA",
                Kategorie = "Dach",
                KompassTyp = "Dach",
                Beschreibung = "Dachfläche",
                Prioritaet = 100
            },
            new()
            {
                Code = "OD",
                Kategorie = "Decke",
                KompassTyp = "ObersteGeschossdecke",
                Beschreibung = "Oberste Geschossdecke",
                Prioritaet = 100
            },
            new()
            {
                Code = "KD",
                Kategorie = "Decke",
                KompassTyp = "Kellerdecke",
                Beschreibung = "Kellerdecke",
                Prioritaet = 100
            },
            new()
            {
                Code = "BP",
                Kategorie = "Boden",
                KompassTyp = "Bodenplatte",
                Beschreibung = "Bodenplatte",
                Prioritaet = 100
            },
            new()
            {
                Code = "FE",
                Kategorie = "Fenster",
                KompassTyp = "Fenster",
                Beschreibung = "Fenster",
                Prioritaet = 100
            },
            new()
            {
                Code = "FT",
                Kategorie = "Fenstertür",
                KompassTyp = "Fenstertuer",
                Beschreibung = "Fenstertür",
                Prioritaet = 110
            },
            new()
            {
                Code = "TU",
                Kategorie = "Tür",
                KompassTyp = "Tuer",
                Beschreibung = "Außentür",
                Prioritaet = 100
            }
        ];
    }
}
