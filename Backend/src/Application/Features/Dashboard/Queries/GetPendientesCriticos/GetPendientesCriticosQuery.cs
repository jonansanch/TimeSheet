using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetPendientesCriticos;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetPendientesCriticosQuery : IRequest<PendientesCriticosResponse>;
