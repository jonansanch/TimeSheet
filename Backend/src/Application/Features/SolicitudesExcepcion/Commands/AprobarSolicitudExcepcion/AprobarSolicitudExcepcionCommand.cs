using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.AprobarSolicitudExcepcion;

[Authorize(Roles = Roles.Admin)]
public record AprobarSolicitudExcepcionCommand(int Id) : IRequest;
