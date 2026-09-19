using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<(RegistroHorasResponse? Registro, string? Error)> CreateAsync(
        CreateRegistroHorasRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/registros-horas");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return (null, await ReadErrorAsync(response, cancellationToken));

        var registro = await response.Content.ReadFromJsonAsync<RegistroHorasResponse>(cancellationToken: cancellationToken);
        return (registro, null);
    }

    /// <summary>Extrae el detalle del ProblemDetails para mostrar el motivo real del rechazo.</summary>
    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(raw)) return null;

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("errors", out var errores))
            {
                var mensajes = errores.EnumerateObject()
                    .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                    .Where(m => !string.IsNullOrWhiteSpace(m));
                var texto = string.Join(" ", mensajes);
                if (!string.IsNullOrWhiteSpace(texto)) return texto;
            }

            return doc.RootElement.TryGetProperty("detail", out var detail) ? detail.GetString() : null;
        }
        catch { return null; }
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

    public async Task<HistorialPaginadoResponse> GetHistorialAsync(int page = 1, int pageSize = 20, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return new HistorialPaginadoResponse(0, []);

        var url = $"api/registros-horas?page={page}&pageSize={pageSize}";
        if (desde.HasValue) url += $"&desde={desde.Value:yyyy-MM-dd}";
        if (hasta.HasValue) url += $"&hasta={hasta.Value:yyyy-MM-dd}";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new HistorialPaginadoResponse(0, []);

        return await response.Content.ReadFromJsonAsync<HistorialPaginadoResponse>(cancellationToken: cancellationToken)
               ?? new HistorialPaginadoResponse(0, []);
    }

    public async Task<ResumenMensualResponse> GetResumenMensualAsync(int mes, int anio, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return ResumenMensualResponse.Vacio();

        var url = $"api/registros-horas/resumen-mensual?mes={mes}&anio={anio}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ResumenMensualResponse.Vacio();

        return await response.Content.ReadFromJsonAsync<ResumenMensualResponse>(cancellationToken: cancellationToken)
               ?? ResumenMensualResponse.Vacio();
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
