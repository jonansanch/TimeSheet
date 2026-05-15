using System.Net.Http.Headers;
using System.Net.Http.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class SolicitudExcepcionAdminRepository : ISolicitudExcepcionAdminRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public SolicitudExcepcionAdminRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<List<SolicitudExcepcionAdminResponse>> GetAllAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/solicitudes-excepcion");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<SolicitudExcepcionAdminResponse>>(
            cancellationToken: ct) ?? [];
    }

    public async Task<bool> AprobarAsync(int id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return false;

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/solicitudes-excepcion/{id}/aprobar");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(message, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RechazarAsync(int id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return false;

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/solicitudes-excepcion/{id}/rechazar");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(message, ct);
        return response.IsSuccessStatusCode;
    }
}
