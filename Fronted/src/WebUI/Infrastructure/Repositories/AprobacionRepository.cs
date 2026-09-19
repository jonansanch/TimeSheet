using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class AprobacionRepository(HttpClient http, AuthStateService authState) : IAprobacionRepository
{
    public async Task<PendientesAprobacionResponse> GetPendientesAsync(
        DateOnly desde,
        DateOnly hasta,
        string? userId = null,
        int? clienteId = null,
        int? proyectoId = null,
        bool incluirRevisados = false,
        CancellationToken ct = default)
    {
        var vacio = new PendientesAprobacionResponse(desde, hasta, 0, 0, []);
        if (string.IsNullOrWhiteSpace(authState.AccessToken)) return vacio;

        var url = $"api/aprobaciones/pendientes?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}"
                + $"&incluirRevisados={incluirRevisados}";
        if (!string.IsNullOrWhiteSpace(userId)) url += $"&userId={Uri.EscapeDataString(userId)}";
        if (clienteId.HasValue)  url += $"&clienteId={clienteId}";
        if (proyectoId.HasValue) url += $"&proyectoId={proyectoId}";

        using var message = CreateMessage(HttpMethod.Get, url);
        var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode) return vacio;

        return await response.Content.ReadFromJsonAsync<PendientesAprobacionResponse>(cancellationToken: ct) ?? vacio;
    }

    public Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> AprobarAsync(int id, CancellationToken ct = default) =>
        EnviarAsync<object?>($"api/aprobaciones/{id}/aprobar", null, ct);

    public Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> RechazarAsync(
        int id, string comentario, CancellationToken ct = default) =>
        EnviarAsync($"api/aprobaciones/{id}/rechazar", new RechazarRequest(comentario), ct);

    public Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> RevertirAprobacionAsync(int id, CancellationToken ct = default) =>
        EnviarAsync<object?>($"api/aprobaciones/{id}/revertir-aprobacion", null, ct);

    public Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> RevertirRechazoAsync(int id, CancellationToken ct = default) =>
        EnviarAsync<object?>($"api/aprobaciones/{id}/revertir-rechazo", null, ct);

    public async Task<(bool Ok, ImportacionResultadoResponse? Resultado, string? Error)> ImportarAsync(
        Stream archivo,
        string nombreArchivo,
        string userId,
        bool marcarAprobado,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var contenido = new MultipartFormDataContent();
        var streamContent = new StreamContent(archivo);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        contenido.Add(streamContent, "archivo", nombreArchivo);
        contenido.Add(new StringContent(userId), "userId");
        contenido.Add(new StringContent(marcarAprobado.ToString()), "marcarAprobado");

        using var message = CreateMessage(HttpMethod.Post, "api/aprobaciones/importar");
        message.Content = contenido;

        var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var resultado = await response.Content.ReadFromJsonAsync<ImportacionResultadoResponse>(cancellationToken: ct);
        return (resultado is not null, resultado, null);
    }

    private async Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> EnviarAsync<TBody>(
        string url, TBody body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, url);
        if (body is not null) message.Content = JsonContent.Create(body);

        var response = await http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var estado = await response.Content.ReadFromJsonAsync<EstadoRegistroResponse>(cancellationToken: ct);
        return (estado is not null, estado, null);
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
        // Un 403 no trae cuerpo util: el motivo es siempre el mismo.
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            return "No tienes permiso para revisar este registro en este nivel.";

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
