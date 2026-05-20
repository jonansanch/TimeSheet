using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using MediatR;

namespace KPG.Timesheet.Application.Features.Reportes.Queries.ExportarTimesheet;

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record ExportarTimesheetQuery(string UserId, int Mes, int Anio) : IRequest<ExportarTimesheetResult>;

public record ExportarTimesheetResult(byte[] Contenido, string ContentType, string FileName);
