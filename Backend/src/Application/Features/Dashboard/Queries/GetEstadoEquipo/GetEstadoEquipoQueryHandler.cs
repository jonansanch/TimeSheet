using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetEstadoEquipo;

public class GetEstadoEquipoQueryHandler(
    IDashboardRepository repository,
    IParametrosSistemaService parametros)
    : IRequestHandler<GetEstadoEquipoQuery, EstadoEquipoResponse>
{
    public async Task<EstadoEquipoResponse> Handle(GetEstadoEquipoQuery request, CancellationToken cancellationToken)
    {
        var minutosDiaCompleto = await parametros.GetMinutosDiaCompletoAsync(cancellationToken);
        return await repository.GetEstadoEquipoAsync(request.Fecha, minutosDiaCompleto, cancellationToken);
    }
}
