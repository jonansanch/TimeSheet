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
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = true,
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
