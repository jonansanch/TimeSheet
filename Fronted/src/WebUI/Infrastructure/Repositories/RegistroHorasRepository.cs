using System.Net.Http.Headers;
using System.Net.Http.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class RegistroHorasRepository : IRegistroHorasRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public RegistroHorasRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<RegistroHorasResponse?> CreateAsync(CreateRegistroHorasRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/registros-horas");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<RegistroHorasResponse>(cancellationToken: cancellationToken);
    }

    public async Task<List<RegistroRecienteResponse>> GetRecientesAsync(int top = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"api/registros-horas/recientes?top={top}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<RegistroRecienteResponse>>(cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<List<HistorialRegistroResponse>> GetHistorialAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/registros-horas");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<HistorialRegistroResponse>>(cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return false;

        using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/registros-horas/{id}");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        return response.StatusCode == System.Net.HttpStatusCode.NoContent;
    }
}
