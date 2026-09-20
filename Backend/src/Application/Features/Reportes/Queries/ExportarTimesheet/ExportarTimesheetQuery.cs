using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Application.Features.Reportes.Queries.ExportarReporteHoras;
using KPG.Timesheet.Domain.Constants;
using MediatR;

namespace KPG.Timesheet.Application.Features.Reportes.Queries.ExportarTimesheet;

/// <summary>
/// Timesheet mensual de un empleado con el formato de la plantilla del cliente.
///
/// <para>
/// Los filtros de cliente, proyecto y recurso acotan que se imprime: un consultor que
/// trabajo para dos clientes en el mes necesita una hoja por cliente para entregar.
/// </para>
/// </summary>
[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record ExportarTimesheetQuery(
    string        UserId,
    int           Mes,
    int           Anio,
    string?       Cliente  = null,
    string?       Proyecto = null,
    string?       Recurso  = null,
    ExportFormato Formato  = ExportFormato.Excel) : IRequest<ExportarTimesheetResult>;

public record ExportarTimesheetResult(byte[] Contenido, string ContentType, string FileName);
