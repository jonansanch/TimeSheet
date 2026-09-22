using KPG.Timesheet.Application.Common.Interfaces;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Application.Features.Sistema.Queries.GetLogoReportes;

/// <summary>Logo actual de los reportes, como data URI, o cadena vacia si no hay ninguno.</summary>
public record GetLogoReportesQuery : IRequest<string>;

public class GetLogoReportesQueryHandler(IParametrosSistemaService parametros)
    : IRequestHandler<GetLogoReportesQuery, string>
{
    public Task<string> Handle(GetLogoReportesQuery request, CancellationToken cancellationToken) =>
        parametros.GetTextoAsync(ParametrosSistemaKeys.LogoReportes, string.Empty, cancellationToken);
}
