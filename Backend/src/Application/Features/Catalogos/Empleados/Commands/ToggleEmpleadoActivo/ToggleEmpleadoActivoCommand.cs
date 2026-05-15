using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.ToggleEmpleadoActivo;

public record ToggleEmpleadoActivoCommand(int Id) : IRequest<EmpleadoDto>;
