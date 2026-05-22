using KPG.Timesheet.Application.Features.Notificaciones.Queries.GetHistorialNotificaciones;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface INotificacionesRepository
{
    Task<HistorialNotificacionesResponse> GetHistorialAsync(DateOnly? desde, DateOnly? hasta, string? userId, bool? soloErrores, CancellationToken cancellationToken = default);
}
