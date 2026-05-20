using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IReportesRepository
{
    Task<ReporteHorasResponse?> GetReporteHorasAsync(
        DateOnly desde,
        DateOnly hasta,
        string? userId = null,
        string? cliente = null,
        string? proyecto = null,
        CancellationToken cancellationToken = default);

    Task<(byte[] Contenido, string ContentType, string FileName)?> ExportarAsync(
        DateOnly desde,
        DateOnly hasta,
        string formato,
        string? userId = null,
        string? cliente = null,
        string? proyecto = null,
        CancellationToken cancellationToken = default);

    Task<(byte[] Contenido, string ContentType, string FileName)?> ExportarTimesheetAsync(
        string userId,
        int mes,
        int anio,
        CancellationToken cancellationToken = default);
}
