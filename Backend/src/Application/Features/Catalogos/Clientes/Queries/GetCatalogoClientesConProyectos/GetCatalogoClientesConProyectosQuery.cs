using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetCatalogoClientesConProyectos;

/// <summary>Proyecto activo del cliente. Lleva Id porque el registro se guarda por ProyectoId.</summary>
public record ProyectoActivoDto(int Id, string Nombre);

public record ClienteConProyectosDto(int Id, string Nombre, List<ProyectoActivoDto> ProyectosActivos);

public record GetCatalogoClientesConProyectosQuery : IRequest<List<ClienteConProyectosDto>>;
