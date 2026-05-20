using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetDashboardGerencial;

[Authorize(Roles = $"{Roles.Gerente},{Roles.Admin}")]
public record GetDashboardGerencialQuery(DateOnly Desde, DateOnly Hasta)
    : IRequest<DashboardGerencialResponse>;
