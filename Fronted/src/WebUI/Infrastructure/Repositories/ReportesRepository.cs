using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class ReportesRepository : IReportesRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public ReportesRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<ReporteHorasResponse?> GetReporteHorasAsync(
        DateOnly desde,
        DateOnly hasta,
        string? userId = null,
        string? cliente = null,
        string? proyecto = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var sb = new StringBuilder($"api/reportes/horas?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(userId))   sb.Append($"&userId={Uri.EscapeDataString(userId)}");
        if (!string.IsNullOrWhiteSpace(cliente))  sb.Append($"&cliente={Uri.EscapeDataString(cliente)}");
        if (!string.IsNullOrWhiteSpace(proyecto)) sb.Append($"&proyecto={Uri.EscapeDataString(proyecto)}");

        using var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ReporteHorasResponse>(cancellationToken: cancellationToken);
    }
}
