using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;
using KPG.Timesheet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.CreateCliente;

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, ClienteDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClienteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClienteDto> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = new Cliente(request.Nombre);
        _context.Clientes.Add(cliente);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Nombre), "Ya existe un cliente con ese nombre.")
            ]);
        }

        return new ClienteDto(cliente.Id, cliente.Nombre, cliente.Activo);
    }

    private static bool EsDuplicado(DbUpdateException ex)
    {
        var mensaje = ex.InnerException?.Message ?? string.Empty;
        return mensaje.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || mensaje.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || mensaje.Contains("IX_Clientes_Nombre", StringComparison.OrdinalIgnoreCase);
    }
}
