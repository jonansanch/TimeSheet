using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.ToggleClienteActivo;

public record ToggleClienteActivoCommand(int Id) : IRequest<ClienteDto>;
