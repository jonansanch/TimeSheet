using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.ToggleClienteActivo;

public class ToggleClienteActivoCommandHandler : IRequestHandler<ToggleClienteActivoCommand, ClienteDto>
{
    private readonly IApplicationDbContext _context;

    public ToggleClienteActivoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClienteDto> Handle(ToggleClienteActivoCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Cliente con id '{request.Id}' no fue encontrado.");

        if (cliente.Activo)
            cliente.Desactivar();
        else
            cliente.Activar();

        await _context.SaveChangesAsync(cancellationToken);

        return new ClienteDto(cliente.Id, cliente.Nombre, cliente.Activo);
    }
}
