using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Reportes.Queries.ExportarReporteHoras;

public enum ExportFormato { Excel, Pdf }

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record ExportarReporteHorasQuery(
    DateOnly Desde,
    DateOnly Hasta,
    string? UserId,
    string? Cliente,
    string? Proyecto,
    ExportFormato Formato) : IRequest<ExportarReporteHorasResult>;

public record ExportarReporteHorasResult(byte[] Contenido, string ContentType, string FileName);
