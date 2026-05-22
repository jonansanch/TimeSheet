using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetMetricasGlobales;

public class GetMetricasGlobalesQueryHandler(IDashboardRepository repository)
    : IRequestHandler<GetMetricasGlobalesQuery, MetricasGlobalesResponse>
{
    public Task<MetricasGlobalesResponse> Handle(GetMetricasGlobalesQuery request, CancellationToken cancellationToken)
        => repository.GetMetricasGlobalesAsync(request.Desde, request.Hasta, cancellationToken);
}
