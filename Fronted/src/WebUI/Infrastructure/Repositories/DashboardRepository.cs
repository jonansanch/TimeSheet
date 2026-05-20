using System.Net.Http.Headers;
using System.Net.Http.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public DashboardRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<EstadoEquipoResponse?> GetEstadoEquipoAsync(DateOnly? fecha = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var query = fecha.HasValue ? $"?fecha={fecha.Value:yyyy-MM-dd}" : string.Empty;
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/dashboard/estado-equipo{query}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<EstadoEquipoResponse>(cancellationToken: cancellationToken);
    }

    public async Task<DistribucionHorasResponse?> GetDistribucionHorasAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var url = $"api/dashboard/distribucion-horas?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<DistribucionHorasResponse>(cancellationToken: cancellationToken);
    }

    public async Task<DashboardGerencialResponse?> GetDashboardGerencialAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var url = $"api/dashboard/gerencial?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<DashboardGerencialResponse>(cancellationToken: cancellationToken);
    }

    public async Task<MetricasGlobalesResponse?> GetMetricasGlobalesAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var url = $"api/dashboard/admin?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<MetricasGlobalesResponse>(cancellationToken: cancellationToken);
    }
}
