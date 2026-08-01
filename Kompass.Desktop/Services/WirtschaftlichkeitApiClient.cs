using Kompass.Desktop.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kompass.Desktop.Services;

public sealed class WirtschaftlichkeitApiClient : IWirtschaftlichkeitApiClient
{
    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public WirtschaftlichkeitApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WirtschaftlichkeitsberichtDto?> BerichtAbrufenAsync(
        Guid projektId,
        string basis,
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
                    $"api/projekte/{projektId}/berichte/wirtschaftlichkeit/{basis}",
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<WirtschaftlichkeitsberichtDto>(
                    JsonOptionen,
                    cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Der Wirtschaftlichkeitsbericht konnte nicht von der KOMPASS-API geladen werden.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new ProjektApiException(
                "Der Wirtschaftlichkeitsbericht konnte nicht gelesen werden.",
                exception);
        }
    }
}
