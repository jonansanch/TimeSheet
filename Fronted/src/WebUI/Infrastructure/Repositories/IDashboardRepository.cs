using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IDashboardRepository
{
    Task<EstadoEquipoResponse?> GetEstadoEquipoAsync(DateOnly? fecha = null, CancellationToken cancellationToken = default);
    Task<DistribucionHorasResponse?> GetDistribucionHorasAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default);
}
