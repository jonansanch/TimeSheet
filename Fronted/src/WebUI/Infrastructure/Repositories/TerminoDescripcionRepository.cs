using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class TerminoDescripcionRepository : ITerminoDescripcionRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public TerminoDescripcionRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<List<TerminoDescripcionResponse>> GetAllAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return [];

        using var message = CreateMessage(HttpMethod.Get, "api/terminos-descripcion");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return [];

        return await response.Content.ReadFromJsonAsync<List<TerminoDescripcionResponse>>(cancellationToken: ct) ?? [];
    }

    public async Task<(bool Ok, TerminoDescripcionResponse? Termino, string? Error)> CreateAsync(
        CreateTerminoDescripcionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, "api/terminos-descripcion");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var termino = await response.Content.ReadFromJsonAsync<TerminoDescripcionResponse>(cancellationToken: ct);
        return (termino is not null, termino, null);
    }

    public async Task<(bool Ok, TerminoDescripcionResponse? Termino, string? Error)> UpdateAsync(
        int id, UpdateTerminoDescripcionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/terminos-descripcion/{id}");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var termino = await response.Content.ReadFromJsonAsync<TerminoDescripcionResponse>(cancellationToken: ct);
        return (termino is not null, termino, null);
    }

    public async Task<(bool Ok, TerminoDescripcionResponse? Termino, string? Error)> ToggleActivoAsync(
        int id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/terminos-descripcion/{id}/toggle");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var termino = await response.Content.ReadFromJsonAsync<TerminoDescripcionResponse>(cancellationToken: ct);
        return (termino is not null, termino, null);
    }

    public async Task<ParametrosDescripcionResponse?> GetParametrosAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return null;

        using var message = CreateMessage(HttpMethod.Get, "api/descripciones/parametros");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ParametrosDescripcionResponse>(cancellationToken: ct);
    }

    public async Task<(bool Ok, string? Error)> UpdateParametrosAsync(
        UpdateParametrosDescripcionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, "api/descripciones/parametros");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, await ReadErrorAsync(response, ct));
    }

    public async Task<EvaluacionDescripcionResponse?> EvaluarAsync(
        string texto, int? proyectoId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return null;

        using var message = CreateMessage(HttpMethod.Post, "api/descripciones/evaluar");
        message.Content = JsonContent.Create(new EvaluarDescripcionRequest(texto, proyectoId));

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<EvaluacionDescripcionResponse>(cancellationToken: ct);
    }

    public async Task<(MejorarDescripcionResultResponse? Resultado, string? Error)> MejorarAsync(
        string texto, int? proyectoId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, "api/descripciones/mejorar");
        message.Content = JsonContent.Create(new MejorarDescripcionRequest(texto, proyectoId));

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (null, await ReadErrorAsync(response, ct));

        var resultado = await response.Content.ReadFromJsonAsync<MejorarDescripcionResultResponse>(cancellationToken: ct);
        return (resultado, null);
    }

    public async Task<bool> MejorarDisponibleAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return false;

        try
        {
            using var message = CreateMessage(HttpMethod.Get, "api/descripciones/mejorar/disponible");
            var response = await _http.SendAsync(message, ct);
            if (!response.IsSuccessStatusCode) return false;

            var body = await response.Content.ReadFromJsonAsync<DisponibleResponse>(cancellationToken: ct);
            return body?.disponible ?? false;
        }
        catch { return false; }
    }

    private sealed record DisponibleResponse(bool disponible);

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
