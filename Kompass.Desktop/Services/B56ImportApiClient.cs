using Kompass.Desktop.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kompass.Desktop.Services;

public sealed class B56ImportApiClient : IB56ImportApiClient
{
    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public B56ImportApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<B56ImportAntwortDto> ImportierenAsync(
        Guid projektId,
        string dateipfad,
        CancellationToken cancellationToken = default)
    {
        if (projektId == Guid.Empty)
        {
            throw new ArgumentException(
                "Die Projekt-ID darf nicht leer sein.",
                nameof(projektId));
        }

        if (!File.Exists(dateipfad))
        {
            throw new FileNotFoundException(
                "Die B56-Datei wurde nicht gefunden.",
                dateipfad);
        }

        try
        {
            await using var dateiStream =
                new FileStream(
                    dateipfad,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    FileOptions.Asynchronous);

            using var dateiInhalt =
                new StreamContent(
                    dateiStream);

            dateiInhalt.Headers.ContentType =
                new MediaTypeHeaderValue(
                    "application/octet-stream");

            using var formular =
                new MultipartFormDataContent();

            formular.Add(
                dateiInhalt,
                "datei",
                Path.GetFileName(dateipfad));

            using var response =
                await _httpClient.PostAsync(
                    $"api/projekte/{projektId}/b56-importe",
                    formular,
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ProjektApiException(
                    "Das Projekt wurde in der KOMPASS-API nicht gefunden.");
            }

            var antwort =
                await response.Content
                    .ReadFromJsonAsync<B56ImportAntwortDto>(
                        JsonOptionen,
                        cancellationToken);

            if (antwort is not null &&
                antwort.Meldungen is not null)
            {
                return antwort;
            }

            throw new ProjektApiException(
                $"Die KOMPASS-API hat für den B56-Import keine vollständige Antwort geliefert " +
                $"(HTTP {(int)response.StatusCode}).");
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Die B56-Datei konnte nicht an die KOMPASS-API übertragen werden. " +
                "Prüfe, ob Kompass.Api gestartet ist.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new ProjektApiException(
                "Die Antwort des B56-Imports konnte nicht gelesen werden.",
                exception);
        }
    }
}
