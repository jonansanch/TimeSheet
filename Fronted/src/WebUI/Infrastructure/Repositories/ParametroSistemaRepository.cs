using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class ParametroSistemaRepository : IParametroSistemaRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public ParametroSistemaRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<int> GetVentanaRetroactividadAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return 3;

        using var request = CreateMessage(HttpMethod.Get, "api/sistema/ventana-retroactividad");
        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return 3;

        var result = await response.Content.ReadFromJsonAsync<VentanaRetroactividadResponse>(
            cancellationToken: cancellationToken);
        return result?.Ventana ?? 3;
    }

    public async Task<(bool Ok, string? Error)> UpdateVentanaRetroactividadAsync(int dias, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, "api/sistema/ventana-retroactividad");
        message.Content = JsonContent.Create(new UpdateVentanaRequest(dias));

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, await ReadErrorAsync(response, ct));

        return (true, null);
    }

    public async Task<int> GetUmbralNotificacionAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return 3;

        using var message = CreateMessage(HttpMethod.Get, "api/sistema/umbral-notificacion");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return 3;

        var result = await response.Content.ReadFromJsonAsync<UmbralNotificacionResponse>(cancellationToken: ct);
        return result?.Dias ?? 3;
    }

    public async Task<(bool Ok, string? Error)> UpdateUmbralNotificacionAsync(int dias, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, "api/sistema/umbral-notificacion");
        message.Content = JsonContent.Create(new UpdateUmbralRequest(dias));

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, await ReadErrorAsync(response, ct));

        return (true, null);
    }

    private HttpRequestMessage CreateMessage(HttpMethod method, string url)
    {
        var message = new HttpRequestMessage(method, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        return message;
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = json.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Array) continue;
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        var msg = item.GetString();
                        if (!string.IsNullOrWhiteSpace(msg)) messages.Add(msg);
                    }
                }
                if (messages.Count > 0) return string.Join(" ", messages);
            }

            if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
                return detail.GetString()!;

            if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
                return title.GetString()!;
        }
        catch { }

        return "No fue posible completar la operacion.";
    }
}
