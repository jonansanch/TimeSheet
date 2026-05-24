using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

public class GetReporteHorasQueryHandler(IReportesRepository repository)
    : IRequestHandler<GetReporteHorasQuery, ReporteHorasResponse>
{
    public Task<ReporteHorasResponse> Handle(GetReporteHorasQuery request, CancellationToken cancellationToken)
        => repository.GetReporteHorasAsync(
            request.Desde,
            request.Hasta,
            request.UserId,
            request.Cliente,
            request.Proyecto,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortDescending,
            cancellationToken);
}
