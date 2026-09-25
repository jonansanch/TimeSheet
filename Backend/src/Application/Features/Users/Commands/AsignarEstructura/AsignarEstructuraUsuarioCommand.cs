using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Commands.AsignarEstructura;

/// <summary>
/// Asigna el jefe directo (organigrama, 2a aprobacion) y el puesto del usuario
/// (que resuelve la 1a aprobacion). Ambos son opcionales: null los desasigna.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public record AsignarEstructuraUsuarioCommand(
    string UserId,
    string? SupervisorUserId,
    int? PuestoId,
    string? CodigoPais = null,
    bool ActualizarCodigoPais = false) : IRequest<UserAdminDto>;
