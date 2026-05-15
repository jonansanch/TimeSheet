using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetCatalogoClientesConProyectos;

public record ClienteConProyectosDto(int Id, string Nombre, List<string> ProyectosActivos);

public record GetCatalogoClientesConProyectosQuery : IRequest<List<ClienteConProyectosDto>>;
