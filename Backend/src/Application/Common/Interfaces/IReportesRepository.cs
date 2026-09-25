using KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IReportesRepository
{
    Task<ReporteHorasResponse> GetReporteHorasAsync(
        DateOnly desde,
        DateOnly hasta,
        string? userId,
        string? cliente,
        string? proyecto,
        string? recurso,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        bool soloConObservaciones = false,
        CancellationToken cancellationToken = default);
}
