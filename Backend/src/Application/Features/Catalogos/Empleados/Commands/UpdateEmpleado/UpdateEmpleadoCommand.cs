using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.UpdateEmpleado;

public record UpdateEmpleadoCommand(int Id, string Nombre) : IRequest<EmpleadoDto>;
