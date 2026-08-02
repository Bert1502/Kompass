using Kompass.Desktop.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kompass.Desktop.Services;

public sealed class FachdatenApiClient(HttpClient httpClient) : IFachdatenApiClient
{
    private static readonly JsonSerializerOptions JsonOptionen = new(JsonSerializerDefaults.Web);

    public Task<FachdatenimportErgebnisDto> PruefenAsync(CancellationToken cancellationToken = default) =>
        SendenAsync(HttpMethod.Get, "api/fachdatenbanken/pruefen", cancellationToken);

    public Task<FachdatenimportErgebnisDto> ImportierenAsync(CancellationToken cancellationToken = default) =>
        SendenAsync(HttpMethod.Post, "api/fachdatenbanken/importieren", cancellationToken);

    private async Task<FachdatenimportErgebnisDto> SendenAsync(HttpMethod method, string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, requestUri);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ProjektApiException($"Fachdaten-Anfrage fehlgeschlagen ({(int)response.StatusCode}): {detail}");
            }

            return await response.Content.ReadFromJsonAsync<FachdatenimportErgebnisDto>(JsonOptionen, cancellationToken)
                ?? throw new ProjektApiException("Die KOMPASS-API lieferte kein Fachdaten-Ergebnis.");
        }
        catch (HttpRequestException exception)
        {
            throw new ProjektApiException("Die Fachdaten konnten nicht von der KOMPASS-API geladen werden.", exception);
        }
        catch (JsonException exception)
        {
            throw new ProjektApiException("Das Fachdaten-Ergebnis konnte nicht gelesen werden.", exception);
        }
    }
}
