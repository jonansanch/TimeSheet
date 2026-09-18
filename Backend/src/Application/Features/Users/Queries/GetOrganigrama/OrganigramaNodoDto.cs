using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Queries.GetOrganigrama;

/// <summary>
/// Un nodo plano del organigrama. El arbol se arma en el cliente enlazando por
/// <see cref="SupervisorUserId"/>; los nodos sin supervisor son las raices.
/// </summary>
public record OrganigramaNodoDto(
    string UserId,
    string Nombre,
    string Email,
    string Rol,
    string? PuestoNombre,
    string? SupervisorUserId);

[Authorize(Roles = $"{Roles.Admin},{Roles.Gerente},{Roles.Supervisor}")]
public record GetOrganigramaQuery : IRequest<IReadOnlyList<OrganigramaNodoDto>>;
