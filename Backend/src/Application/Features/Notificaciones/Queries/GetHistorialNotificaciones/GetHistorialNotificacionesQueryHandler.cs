using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Notificaciones.Queries.GetHistorialNotificaciones;

public class GetHistorialNotificacionesQueryHandler(INotificacionesRepository repository)
    : IRequestHandler<GetHistorialNotificacionesQuery, HistorialNotificacionesResponse>
{
    public Task<HistorialNotificacionesResponse> Handle(GetHistorialNotificacionesQuery request, CancellationToken cancellationToken)
        => repository.GetHistorialAsync(request.Desde, request.Hasta, request.UserId, request.SoloErrores, cancellationToken);
}
