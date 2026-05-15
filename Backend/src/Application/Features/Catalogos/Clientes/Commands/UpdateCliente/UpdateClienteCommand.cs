using FluentValidation;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.UpdateCliente;

public record UpdateClienteCommand(int Id, string Nombre) : IRequest<ClienteDto>;

public class UpdateClienteCommandValidator : AbstractValidator<UpdateClienteCommand>
{
    public UpdateClienteCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}
