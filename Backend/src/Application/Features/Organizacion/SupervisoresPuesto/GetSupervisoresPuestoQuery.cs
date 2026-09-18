using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Organizacion.SupervisoresPuesto;

[Authorize(Roles = Roles.Admin)]
public record GetSupervisoresPuestoQuery : IRequest<IReadOnlyList<SupervisorPuestoDto>>;

public record SupervisorPuestoDto(
    int Id,
    int PuestoId,
    string PuestoNombre,
    int? ClienteId,
    string? ClienteNombre,
    string SupervisorUserId,
    bool Activo);
