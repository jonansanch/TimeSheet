using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetEstadoEquipo;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetEstadoEquipoQuery(DateOnly Fecha) : IRequest<EstadoEquipoResponse>;
