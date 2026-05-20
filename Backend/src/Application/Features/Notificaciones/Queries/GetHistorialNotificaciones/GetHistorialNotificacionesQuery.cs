using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Notificaciones.Queries.GetHistorialNotificaciones;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetHistorialNotificacionesQuery(
    DateOnly? Desde,
    DateOnly? Hasta,
    string? UserId,
    bool? SoloErrores) : IRequest<HistorialNotificacionesResponse>;
