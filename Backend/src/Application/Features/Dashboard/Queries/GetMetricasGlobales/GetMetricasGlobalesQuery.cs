using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetMetricasGlobales;

[Authorize(Roles = Roles.Admin)]
public record GetMetricasGlobalesQuery(DateOnly Desde, DateOnly Hasta)
    : IRequest<MetricasGlobalesResponse>;
