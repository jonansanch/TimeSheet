using FluentValidation;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.CreateCliente;

public record CreateClienteCommand(string Nombre) : IRequest<ClienteDto>;

public class CreateClienteCommandValidator : AbstractValidator<CreateClienteCommand>
{
    public CreateClienteCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}
