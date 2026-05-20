using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class NotificacionesRepository : INotificacionesRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public NotificacionesRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<HistorialNotificacionesResponse?> GetHistorialAsync(
        DateOnly? desde = null,
        DateOnly? hasta = null,
        string? userId = null,
        bool? soloErrores = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var sb = new StringBuilder("api/notificaciones/historial?");
        if (desde.HasValue)   sb.Append($"desde={desde.Value:yyyy-MM-dd}&");
        if (hasta.HasValue)   sb.Append($"hasta={hasta.Value:yyyy-MM-dd}&");
        if (!string.IsNullOrWhiteSpace(userId)) sb.Append($"userId={Uri.EscapeDataString(userId)}&");
        if (soloErrores.HasValue) sb.Append($"soloErrores={soloErrores.Value}&");

        using var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString().TrimEnd('&', '?'));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<HistorialNotificacionesResponse>(cancellationToken: cancellationToken);
    }
}
