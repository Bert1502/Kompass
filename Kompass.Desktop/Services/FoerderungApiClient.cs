using Kompass.Desktop.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kompass.Desktop.Services;

public sealed class FoerderungApiClient : IFoerderungApiClient
{
    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public FoerderungApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<FoerderprogrammKatalogDto>> KatalogAbrufenAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response =
                await _httpClient.GetAsync(
                    "api/foerderprogramme",
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            var programme =
                await response.Content
                    .ReadFromJsonAsync<List<FoerderprogrammKatalogDto>>(
                        JsonOptionen,
                        cancellationToken);

            return programme
                ?? (IReadOnlyList<FoerderprogrammKatalogDto>)Array.Empty<FoerderprogrammKatalogDto>();
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Der Förderprogrammkatalog konnte nicht von der KOMPASS-API geladen werden.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new ProjektApiException(
                "Der Förderprogrammkatalog konnte nicht gelesen werden.",
                exception);
        }
    }

    public async Task<FoerderuebersichtBerichtDto?> UebersichtAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        if (projektId == Guid.Empty)
        {
            return null;
        }

        try
        {
            using var response =
                await _httpClient.GetAsync(
                    $"api/projekte/{projektId}/berichte/foerderuebersicht",
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<FoerderuebersichtBerichtDto>(
                    JsonOptionen,
                    cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Die Förderübersicht konnte nicht von der KOMPASS-API geladen werden.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new ProjektApiException(
                "Die Förderübersicht konnte nicht gelesen werden.",
                exception);
        }
    }
}
