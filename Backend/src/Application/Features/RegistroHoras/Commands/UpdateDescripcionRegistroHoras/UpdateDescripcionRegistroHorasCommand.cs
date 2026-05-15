using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Admin}")]
public record UpdateDescripcionRegistroHorasCommand(int RegistroId, string Descripcion) : IRequest;
