using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.CreateEmpleado;

public record CreateEmpleadoCommand(string Nombre) : IRequest<EmpleadoDto>;
