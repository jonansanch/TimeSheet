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
        string? recurso = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = true,
        bool soloConObservaciones = false,
        CancellationToken cancellationToken = default);

    Task<(byte[] Contenido, string ContentType, string FileName)?> ExportarAsync(
        DateOnly desde,
        DateOnly hasta,
        string formato,
        string? userId = null,
        string? cliente = null,
        string? proyecto = null,
        string? recurso = null,
        bool soloConObservaciones = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Timesheet mensual en el formato de la plantilla del cliente. Los filtros acotan
    /// que se imprime: quien trabajo para dos clientes necesita una hoja por cliente.
    /// </summary>
    Task<(byte[] Contenido, string ContentType, string FileName)?> ExportarTimesheetAsync(
        string userId,
        int mes,
        int anio,
        string formato = "excel",
        string? cliente = null,
        string? proyecto = null,
        string? recurso = null,
        CancellationToken cancellationToken = default);
}
