using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class ModalidadRepository : IModalidadRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public ModalidadRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<List<ModalidadResponse>> GetAllAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, "api/modalidades");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<ModalidadResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<List<ModalidadResponse>> GetActivasAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, "api/modalidades/activas");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<ModalidadResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<(bool Ok, ModalidadResponse? Modalidad, string? Error)> CreateAsync(
        CreateModalidadRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, "api/modalidades");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var modalidad = await response.Content.ReadFromJsonAsync<ModalidadResponse>(cancellationToken: ct);
        return (modalidad is not null, modalidad, null);
    }

    public async Task<(bool Ok, ModalidadResponse? Modalidad, string? Error)> UpdateAsync(
        int id, UpdateModalidadRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/modalidades/{id}");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var modalidad = await response.Content.ReadFromJsonAsync<ModalidadResponse>(cancellationToken: ct);
        return (modalidad is not null, modalidad, null);
    }

    public async Task<(bool Ok, ModalidadResponse? Modalidad, string? Error)> ToggleActivaAsync(
        int id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/modalidades/{id}/toggle");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var modalidad = await response.Content.ReadFromJsonAsync<ModalidadResponse>(cancellationToken: ct);
        return (modalidad is not null, modalidad, null);
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
