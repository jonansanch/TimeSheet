using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class LugarTrabajoRepository : ILugarTrabajoRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public LugarTrabajoRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<List<LugarTrabajoResponse>> GetAllAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, "api/lugares-trabajo");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<LugarTrabajoResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<List<LugarTrabajoResponse>> GetActivosAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, "api/lugares-trabajo/activos");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<LugarTrabajoResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<(bool Ok, LugarTrabajoResponse? Lugar, string? Error)> CreateAsync(
        CreateLugarTrabajoRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, "api/lugares-trabajo");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var lugar = await response.Content.ReadFromJsonAsync<LugarTrabajoResponse>(cancellationToken: ct);
        return (lugar is not null, lugar, null);
    }

    public async Task<(bool Ok, LugarTrabajoResponse? Lugar, string? Error)> UpdateAsync(
        int id, UpdateLugarTrabajoRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/lugares-trabajo/{id}");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var lugar = await response.Content.ReadFromJsonAsync<LugarTrabajoResponse>(cancellationToken: ct);
        return (lugar is not null, lugar, null);
    }

    public async Task<(bool Ok, LugarTrabajoResponse? Lugar, string? Error)> ToggleActivoAsync(
        int id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/lugares-trabajo/{id}/toggle");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var lugar = await response.Content.ReadFromJsonAsync<LugarTrabajoResponse>(cancellationToken: ct);
        return (lugar is not null, lugar, null);
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
