using KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IReportesRepository
{
    Task<ReporteHorasResponse> GetReporteHorasAsync(DateOnly desde, DateOnly hasta, string? userId, string? cliente, string? proyecto, CancellationToken cancellationToken = default);
}
