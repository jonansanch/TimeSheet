using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetDistribucionHoras;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetDistribucionHorasQuery(DateOnly Desde, DateOnly Hasta)
    : IRequest<DistribucionHorasResponse>;
