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
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var sb = new StringBuilder($"api/reportes/horas?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&pageNumber={pageNumber}&pageSize={pageSize}&sortDescending={sortDescending}");
        if (!string.IsNullOrWhiteSpace(userId))   sb.Append($"&userId={Uri.EscapeDataString(userId)}");
        if (!string.IsNullOrWhiteSpace(cliente))  sb.Append($"&cliente={Uri.EscapeDataString(cliente)}");
        if (!string.IsNullOrWhiteSpace(proyecto)) sb.Append($"&proyecto={Uri.EscapeDataString(proyecto)}");
        if (!string.IsNullOrWhiteSpace(sortBy))   sb.Append($"&sortBy={Uri.EscapeDataString(sortBy)}");

        using var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ReporteHorasResponse>(cancellationToken: cancellationToken);
    }

    public async Task<(byte[] Contenido, string ContentType, string FileName)?> ExportarAsync(
        DateOnly desde,
        DateOnly hasta,
        string formato,
        string? userId = null,
        string? cliente = null,
        string? proyecto = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var sb = new StringBuilder($"api/reportes/horas/{formato}?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(userId))   sb.Append($"&userId={Uri.EscapeDataString(userId)}");
        if (!string.IsNullOrWhiteSpace(cliente))  sb.Append($"&cliente={Uri.EscapeDataString(cliente)}");
        if (!string.IsNullOrWhiteSpace(proyecto)) sb.Append($"&proyecto={Uri.EscapeDataString(proyecto)}");

        using var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var ext = formato == "excel" ? "xlsx" : formato;
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                    ?? response.Content.Headers.ContentDisposition?.FileName
                    ?? $"reporte.{ext}";

        return (bytes, contentType, fileName.Trim('"'));
    }

    public async Task<(byte[] Contenido, string ContentType, string FileName)?> ExportarTimesheetAsync(
        string userId,
        int mes,
        int anio,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            throw new UnauthorizedAccessException("No hay token de acceso activo.");

        var url = $"api/reportes/timesheet/excel?userId={Uri.EscapeDataString(userId)}&mes={mes}&anio={anio}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var bytes      = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName   = response.Content.Headers.ContentDisposition?.FileNameStar
                      ?? response.Content.Headers.ContentDisposition?.FileName
                      ?? $"timesheet-{mes:00}-{anio}.xlsx";

        return (bytes, contentType, fileName.Trim('"'));
    }
}
