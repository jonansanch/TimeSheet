using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetDashboardGerencial;

public class GetDashboardGerencialQueryHandler(IDashboardRepository repository)
    : IRequestHandler<GetDashboardGerencialQuery, DashboardGerencialResponse>
{
    public Task<DashboardGerencialResponse> Handle(GetDashboardGerencialQuery request, CancellationToken cancellationToken)
        => repository.GetGerencialAsync(request.Desde, request.Hasta, cancellationToken);
}
