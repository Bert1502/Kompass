using Kompass.Desktop.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kompass.Desktop.Services;

public sealed class ProjektApiClient : IProjektApiClient
{
    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public ProjektApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ProjektUebersichtDto>>
        AlleAbrufenAsync(
            CancellationToken cancellationToken = default)
    {
        try
        {
            var projekte =
                await _httpClient
                    .GetFromJsonAsync<List<ProjektUebersichtDto>>(
                        "api/projekte",
                        JsonOptionen,
                        cancellationToken);

            return projekte
                ?? new List<ProjektUebersichtDto>();
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Die KOMPASS-API konnte nicht erreicht werden. " +
                "Prüfe, ob Kompass.Api gestartet ist.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new ProjektApiException(
                "Die Antwort der KOMPASS-API konnte nicht gelesen werden.",
                exception);
        }
    }

    public async Task<ProjektUebersichtDto?>
        NachIdAbrufenAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        try
        {
            using var response =
                await _httpClient.GetAsync(
                    $"api/projekte/{id}",
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            await StelleErfolgSicherAsync(
                response,
                cancellationToken);

            return await response.Content
                .ReadFromJsonAsync<ProjektUebersichtDto>(
                    JsonOptionen,
                    cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Das Projekt konnte nicht von der API geladen werden.",
                exception);
        }
    }

    public async Task<ProjektUebersichtDto>
        ErstellenAsync(
            string name,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Der Projektname darf nicht leer sein.",
                nameof(name));
        }

        var request =
            new ProjektErstellenDto(name.Trim());

        try
        {
            using var response =
                await _httpClient.PostAsJsonAsync(
                    "api/projekte",
                    request,
                    JsonOptionen,
                    cancellationToken);

            await StelleErfolgSicherAsync(
                response,
                cancellationToken);

            var projekt =
                await response.Content
                    .ReadFromJsonAsync<ProjektUebersichtDto>(
                        JsonOptionen,
                        cancellationToken);

            return projekt
                ?? throw new ProjektApiException(
                    "Die API hat kein angelegtes Projekt zurückgegeben.");
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Das Projekt konnte nicht angelegt werden.",
                exception);
        }
    }

public async Task<ProjektUebersichtDto?>
    AktualisierenAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken = default)
{
    if (id == Guid.Empty)
    {
        return null;
    }

    if (string.IsNullOrWhiteSpace(name))
    {
        throw new ArgumentException(
            "Der Projektname darf nicht leer sein.",
            nameof(name));
    }

    var request =
        new ProjektAktualisierenDto(
            name.Trim());

    try
    {
        using var response =
            await _httpClient.PutAsJsonAsync(
                $"api/projekte/{id}",
                request,
                JsonOptionen,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await StelleErfolgSicherAsync(
            response,
            cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<ProjektUebersichtDto>(
                JsonOptionen,
                cancellationToken);
    }
    catch (HttpRequestException exception)
    {
        throw new ProjektApiException(
            "Das Projekt konnte nicht aktualisiert werden.",
            exception);
    }
}

    public async Task<bool>
        LoeschenAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        try
        {
            using var response =
                await _httpClient.DeleteAsync(
                    $"api/projekte/{id}",
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            await StelleErfolgSicherAsync(
                response,
                cancellationToken);

            return true;
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Das Projekt konnte nicht gelöscht werden.",
                exception);
        }
    }

    private static async Task StelleErfolgSicherAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var inhalt =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        var nachricht =
            ExtrahiereFehlernachricht(inhalt);

        throw new ProjektApiException(
            $"API-Fehler {(int)response.StatusCode} " +
            $"({response.StatusCode}): {nachricht}");
    }

    private static string ExtrahiereFehlernachricht(
        string inhalt)
    {
        if (string.IsNullOrWhiteSpace(inhalt))
        {
            return "Keine weitere Fehlerinformation verfügbar.";
        }

        try
        {
            using var dokument =
                JsonDocument.Parse(inhalt);

            if (dokument.RootElement.TryGetProperty(
                    "nachricht",
                    out var nachricht))
            {
                return nachricht.GetString()
                    ?? inhalt;
            }

            if (dokument.RootElement.TryGetProperty(
                    "title",
                    out var titel))
            {
                return titel.GetString()
                    ?? inhalt;
            }
        }
        catch (JsonException)
        {
            // Antwort ist kein JSON.
        }

        return inhalt;
    }
}
