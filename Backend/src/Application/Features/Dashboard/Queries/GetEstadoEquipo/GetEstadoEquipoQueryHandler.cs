using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetEstadoEquipo;

public class GetEstadoEquipoQueryHandler(IDashboardRepository repository)
    : IRequestHandler<GetEstadoEquipoQuery, EstadoEquipoResponse>
{
    public Task<EstadoEquipoResponse> Handle(GetEstadoEquipoQuery request, CancellationToken cancellationToken)
        => repository.GetEstadoEquipoAsync(request.Fecha, cancellationToken);
}
