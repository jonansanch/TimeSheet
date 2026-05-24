using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record GetReporteHorasQuery(
    DateOnly Desde,
    DateOnly Hasta,
    string? UserId,
    string? Cliente,
    string? Proyecto,
    int PageNumber,
    int PageSize,
    string? SortBy,
    bool SortDescending) : IRequest<ReporteHorasResponse>;
