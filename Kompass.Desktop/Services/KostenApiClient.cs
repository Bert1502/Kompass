using Kompass.Desktop.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kompass.Desktop.Services;

public sealed class KostenApiClient : IKostenApiClient
{
    private static readonly JsonSerializerOptions JsonOptionen =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public KostenApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<KostenAlternativeDto>> AlternativenAbrufenAsync(
        Guid projektId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<KostenAlternativeDto>>(
                $"api/projekte/{projektId}/alternativen",
                JsonOptionen,
                cancellationToken)
                ?? new List<KostenAlternativeDto>();
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Die Modernisierungsalternativen konnten nicht geladen werden.",
                exception);
        }
    }

    public async Task<IReadOnlyList<KostenpositionDto>> PositionenAbrufenAsync(
        Guid projektId,
        Guid alternativeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<KostenpositionDto>>(
                Pfad(projektId, alternativeId),
                JsonOptionen,
                cancellationToken)
                ?? new List<KostenpositionDto>();
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException(
                "Die Kostenpositionen konnten nicht geladen werden.",
                exception);
        }
    }

    public async Task<KostenpositionDto> HinzufuegenAsync(
        Guid projektId,
        Guid alternativeId,
        KostenpositionErstellenDto position,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            Pfad(projektId, alternativeId),
            position,
            JsonOptionen,
            cancellationToken);

        await StelleErfolgSicherAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<KostenpositionDto>(
            JsonOptionen,
            cancellationToken)
            ?? throw new ProjektApiException(
                "Die API hat keine gespeicherte Kostenposition zurückgegeben.");
    }

    public async Task<bool> EntfernenAsync(
        Guid projektId,
        Guid alternativeId,
        Guid kostenpositionId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"{Pfad(projektId, alternativeId)}/{kostenpositionId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await StelleErfolgSicherAsync(response, cancellationToken);
        return true;
    }

    private static string Pfad(Guid projektId, Guid alternativeId) =>
        $"api/projekte/{projektId}/alternativen/{alternativeId}/kostenpositionen";

    private static async Task StelleErfolgSicherAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var inhalt = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new ProjektApiException(
            $"API-Fehler {(int)response.StatusCode} ({response.StatusCode}): {inhalt}");
    }
}
