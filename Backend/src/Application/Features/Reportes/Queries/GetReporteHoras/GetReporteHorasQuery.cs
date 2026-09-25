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
    string? Recurso,
    int PageNumber,
    int PageSize,
    string? SortBy,
    bool SortDescending,
    /// <summary>
    /// True para quedarse solo con los registros cuya descripcion tiene observaciones de
    /// calidad (ver Docs/plan-calidad-descripciones.md). Paginar esto requiere evaluar
    /// todo lo que cae dentro del filtro, no solo la pagina pedida: por eso este modo tiene
    /// un tope mas chico de filas exploradas (ver ReportesRepository).
    /// </summary>
    bool SoloConObservaciones = false) : IRequest<ReporteHorasResponse>;
