using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class BitacoraRepository : IBitacoraRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public BitacoraRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<BitacoraResponse?> GetMiAlcanceAsync(
        DateOnly? desde = null,
        DateOnly? hasta = null,
        string? actorId = null,
        string? tipoEvento = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var sb = new StringBuilder("api/bitacora/mi-alcance?");
        if (desde.HasValue)                         sb.Append($"desde={desde.Value:yyyy-MM-dd}&");
        if (hasta.HasValue)                         sb.Append($"hasta={hasta.Value:yyyy-MM-dd}&");
        if (!string.IsNullOrWhiteSpace(actorId))    sb.Append($"actorId={Uri.EscapeDataString(actorId)}&");
        if (!string.IsNullOrWhiteSpace(tipoEvento)) sb.Append($"tipoEvento={Uri.EscapeDataString(tipoEvento)}&");

        using var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString().TrimEnd('&', '?'));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<BitacoraResponse>(cancellationToken: ct);
    }
}
