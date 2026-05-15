using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public ClienteRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<List<ClienteResponse>> GetAllAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, "api/clientes");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<ClienteResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<List<ClienteConProyectosResponse>> GetCatalogoAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, "api/clientes/catalogo");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<ClienteConProyectosResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<(bool Ok, ClienteResponse? Cliente, string? Error)> CreateAsync(
        CreateClienteRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, "api/clientes");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>(cancellationToken: ct);
        return (cliente is not null, cliente, null);
    }

    public async Task<(bool Ok, ClienteResponse? Cliente, string? Error)> UpdateAsync(
        int id, UpdateClienteRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/clientes/{id}");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>(cancellationToken: ct);
        return (cliente is not null, cliente, null);
    }

    public async Task<(bool Ok, ClienteResponse? Cliente, string? Error)> ToggleActivoAsync(
        int id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/clientes/{id}/toggle");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponse>(cancellationToken: ct);
        return (cliente is not null, cliente, null);
    }

    public async Task<List<ProyectoResponse>> GetProyectosAsync(int clienteId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, $"api/clientes/{clienteId}/proyectos");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<ProyectoResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<(bool Ok, ProyectoResponse? Proyecto, string? Error)> CreateProyectoAsync(
        int clienteId, CreateProyectoRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, $"api/clientes/{clienteId}/proyectos");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var proyecto = await response.Content.ReadFromJsonAsync<ProyectoResponse>(cancellationToken: ct);
        return (proyecto is not null, proyecto, null);
    }

    public async Task<(bool Ok, ProyectoResponse? Proyecto, string? Error)> UpdateProyectoAsync(
        int id, UpdateProyectoRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/proyectos/{id}");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var proyecto = await response.Content.ReadFromJsonAsync<ProyectoResponse>(cancellationToken: ct);
        return (proyecto is not null, proyecto, null);
    }

    public async Task<(bool Ok, ProyectoResponse? Proyecto, string? Error)> ToggleProyectoActivoAsync(
        int id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/proyectos/{id}/toggle");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var proyecto = await response.Content.ReadFromJsonAsync<ProyectoResponse>(cancellationToken: ct);
        return (proyecto is not null, proyecto, null);
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
