using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetDistribucionHoras;

public class GetDistribucionHorasQueryHandler(IDashboardRepository repository)
    : IRequestHandler<GetDistribucionHorasQuery, DistribucionHorasResponse>
{
    public Task<DistribucionHorasResponse> Handle(GetDistribucionHorasQuery request, CancellationToken cancellationToken)
        => repository.GetDistribucionHorasAsync(request.Desde, request.Hasta, cancellationToken);
}
