using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class ReglaVentanaRepository(HttpClient http, AuthStateService authState) : IReglaVentanaRepository
{
    public async Task<List<ReglaVentanaResponse>> GetAllAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken)) return [];

        using var message = CreateMessage(HttpMethod.Get, "api/reglas-ventana");
        var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<List<ReglaVentanaResponse>>(cancellationToken: ct) ?? [];
    }

    public Task<(bool Ok, ReglaVentanaResponse? Item, string? Error)> CrearAsync(
        GuardarReglaVentanaRequest request, CancellationToken ct = default) =>
        EnviarAsync(HttpMethod.Post, "api/reglas-ventana", request, ct);

    public Task<(bool Ok, ReglaVentanaResponse? Item, string? Error)> ActualizarAsync(
        int id, GuardarReglaVentanaRequest request, CancellationToken ct = default) =>
        EnviarAsync(HttpMethod.Put, $"api/reglas-ventana/{id}", request, ct);

    public async Task<(bool Ok, ReglaVentanaResponse? Item)> ToggleAsync(int id, CancellationToken ct = default)
    {
        var (ok, item, _) = await EnviarAsync<object?>(HttpMethod.Put, $"api/reglas-ventana/{id}/toggle", null, ct);
        return (ok, item);
    }

    private async Task<(bool Ok, ReglaVentanaResponse? Item, string? Error)> EnviarAsync<TBody>(
        HttpMethod method, string url, TBody body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(method, url);
        if (body is not null) message.Content = JsonContent.Create(body);

        var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var item = await response.Content.ReadFromJsonAsync<ReglaVentanaResponse>(cancellationToken: ct);
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
