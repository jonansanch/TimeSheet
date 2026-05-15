using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;

public record ClienteDto(int Id, string Nombre, bool Activo);

public record GetClientesQuery(bool SoloActivos = false) : IRequest<List<ClienteDto>>;
