using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class VozRepository(HttpClient http, AuthStateService authState) : IVozRepository
{
    public async Task<bool> EstaDisponibleAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken)) return false;

        try
        {
            using var message = Crear(HttpMethod.Get, "api/voz/disponible");
            var response = await http.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode) return false;

            var cuerpo = await response.Content.ReadFromJsonAsync<DisponibleResponse>(
                cancellationToken: cancellationToken);
            return cuerpo?.Disponible ?? false;
        }
        catch { return false; }
    }

    public async Task<InterpretacionVozResponse?> InterpretarAsync(
        string transcripcion, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authState.AccessToken)) return null;

        try
        {
            using var message = Crear(HttpMethod.Post, "api/voz/interpretar");
            message.Content = JsonContent.Create(new { Transcripcion = transcripcion });

            var response = await http.SendAsync(message, cancellationToken);

            // 503 = el ambiente no tiene IA configurada. No es un error que mostrar:
            // quien llama cae al parser de reglas.
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable) return null;
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<InterpretacionVozResponse>(
                cancellationToken: cancellationToken);
        }
        catch { return null; }
    }

    private HttpRequestMessage Crear(HttpMethod metodo, string url)
    {
        var message = new HttpRequestMessage(metodo, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authState.AccessToken);
        return message;
    }

    private sealed record DisponibleResponse(bool Disponible);
}
