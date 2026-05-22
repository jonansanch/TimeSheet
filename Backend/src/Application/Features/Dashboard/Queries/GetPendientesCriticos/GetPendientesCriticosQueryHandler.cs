using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetPendientesCriticos;

public class GetPendientesCriticosQueryHandler(IDashboardRepository repository)
    : IRequestHandler<GetPendientesCriticosQuery, PendientesCriticosResponse>
{
    public Task<PendientesCriticosResponse> Handle(GetPendientesCriticosQuery request, CancellationToken cancellationToken)
        => repository.GetPendientesCriticosAsync(cancellationToken);
}
