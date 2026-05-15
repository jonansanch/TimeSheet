using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;

public record GetEmpleadosQuery(bool SoloActivos = false) : IRequest<List<EmpleadoDto>>;
