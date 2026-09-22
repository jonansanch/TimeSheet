using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class ReporteUsuarioRepository(HttpClient http, AuthStateService authState) : IReporteUsuarioRepository
{
    public Task<List<ReporteUsuarioResponse>> GetMisReportesAsync(CancellationToken ct = default) =>
        GetListAsync("api/reportes-usuario/mios", ct);

    public Task<List<ReporteUsuarioResponse>> GetTodosAsync(CancellationToken ct = default) =>
        GetListAsync("api/reportes-usuario", ct);

    public Task<(bool Ok, ReporteUsuarioResponse? Item, string? Error)> CrearAsync(
        CrearReporteUsuarioRequest request, CancellationToken ct = default) =>
        EnviarAsync(HttpMethod.Post, "api/reportes-usuario", request, ct);

    public Task<(bool Ok, ReporteUsuarioResponse? Item, string? Error)> CambiarEstadoAsync(
        int id, CambiarEstadoReporteRequest request, CancellationToken ct = default) =>
        EnviarAsync(HttpMethod.Put, $"api/reportes-usuario/{id}/estado", request, ct);

    private async Task<List<ReporteUsuarioResponse>> GetListAsync(string url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken)) return [];

        using var message = CreateMessage(HttpMethod.Get, url);
        var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<List<ReporteUsuarioResponse>>(cancellationToken: ct) ?? [];
    }

    private async Task<(bool Ok, ReporteUsuarioResponse? Item, string? Error)> EnviarAsync<TBody>(
        HttpMethod method, string url, TBody body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(method, url);
        if (body is not null) message.Content = JsonContent.Create(body);

        var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var item = await response.Content.ReadFromJsonAsync<ReporteUsuarioResponse>(cancellationToken: ct);
        return (item is not null, item, null);
    }

    private HttpRequestMessage CreateMessage(HttpMethod method, string url)
    {
        var message = new HttpRequestMessage(method, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.AccessToken);
        return message;
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
}
