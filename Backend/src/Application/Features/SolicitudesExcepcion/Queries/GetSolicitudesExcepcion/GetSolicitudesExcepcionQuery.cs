using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Queries.GetSolicitudesExcepcion;

[Authorize(Roles = Roles.Admin)]
public record GetSolicitudesExcepcionQuery : IRequest<IEnumerable<SolicitudExcepcionAdminDto>>;
