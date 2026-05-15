using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;

public record ProyectoDto(int Id, string Nombre, int ClienteId, bool Activo);

public record GetProyectosPorClienteQuery(int ClienteId, bool SoloActivos = false) : IRequest<List<ProyectoDto>>;
