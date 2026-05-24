using System.Net.Http.Headers;
using System.Net.Http.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class SolicitudExcepcionRepository : ISolicitudExcepcionRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public SolicitudExcepcionRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<List<DateOnly>> GetMisAprobadasAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/solicitudes-excepcion/mis-aprobadas");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<DateOnly>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<List<MiSolicitudExcepcionResponse>> GetMisSolicitudesAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = new HttpRequestMessage(HttpMethod.Get, "api/solicitudes-excepcion/mis-solicitudes");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<MiSolicitudExcepcionResponse>>(cancellationToken: cancellationToken) ?? [];
    }

    public async Task<SolicitudExcepcionResponse?> CreateAsync(
        CreateSolicitudExcepcionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return null;

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/solicitudes-excepcion");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<SolicitudExcepcionResponse>(
            cancellationToken: cancellationToken);
    }
}
