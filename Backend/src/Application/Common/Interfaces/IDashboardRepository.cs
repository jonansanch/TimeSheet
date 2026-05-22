using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDashboardGerencial;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDistribucionHoras;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetEstadoEquipo;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetMetricasGlobales;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetPendientesCriticos;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardGerencialResponse> GetGerencialAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default);
    Task<DistribucionHorasResponse> GetDistribucionHorasAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default);
    Task<EstadoEquipoResponse> GetEstadoEquipoAsync(DateOnly fecha, CancellationToken cancellationToken = default);
    Task<MetricasGlobalesResponse> GetMetricasGlobalesAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default);
    Task<PendientesCriticosResponse> GetPendientesCriticosAsync(CancellationToken cancellationToken = default);
}
