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

    public async Task<HistorialPaginadoResponse> GetHistorialAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return new HistorialPaginadoResponse(0, []);

        var url = $"api/registros-horas?page={page}&pageSize={pageSize}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new HistorialPaginadoResponse(0, []);

        return await response.Content.ReadFromJsonAsync<HistorialPaginadoResponse>(cancellationToken: cancellationToken)
               ?? new HistorialPaginadoResponse(0, []);
    }

    public async Task<List<DateOnly>> GetDiasConRegistroAsync(int mes, int anio, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        var desde = new DateOnly(anio, mes, 1);
        var hasta  = desde.AddMonths(1).AddDays(-1);
        var url    = $"api/registros-horas?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&pageSize=50";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return [];

        var paginado = await response.Content.ReadFromJsonAsync<HistorialPaginadoResponse>(cancellationToken: cancellationToken)
                       ?? new HistorialPaginadoResponse(0, []);
        return paginado.Items.Select(r => r.FechaRegistro).Distinct().ToList();
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

    public async Task<bool> UpdateDescripcionAsync(int id, string descripcion, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return false;

        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"api/registros-horas/{id}/descripcion");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        httpRequest.Content = JsonContent.Create(new UpdateDescripcionRegistroRequest(descripcion));

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        return response.StatusCode == System.Net.HttpStatusCode.NoContent;
    }
}
