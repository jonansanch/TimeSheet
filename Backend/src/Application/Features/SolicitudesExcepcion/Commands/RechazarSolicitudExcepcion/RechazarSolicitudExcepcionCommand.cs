using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.RechazarSolicitudExcepcion;

[Authorize(Roles = Roles.Admin)]
public record RechazarSolicitudExcepcionCommand(int Id) : IRequest;
