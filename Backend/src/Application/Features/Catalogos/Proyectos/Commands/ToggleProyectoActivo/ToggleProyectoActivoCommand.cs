using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.ToggleProyectoActivo;

public record ToggleProyectoActivoCommand(int Id) : IRequest<ProyectoDto>;
