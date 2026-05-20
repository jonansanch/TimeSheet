using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface INotificacionesRepository
{
    Task<HistorialNotificacionesResponse?> GetHistorialAsync(
        DateOnly? desde = null,
        DateOnly? hasta = null,
        string? userId = null,
        bool? soloErrores = null,
        CancellationToken cancellationToken = default);
}
