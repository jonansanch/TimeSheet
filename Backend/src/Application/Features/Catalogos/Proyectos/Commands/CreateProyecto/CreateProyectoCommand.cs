using FluentValidation;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.CreateProyecto;

public record CreateProyectoCommand(int ClienteId, string Nombre) : IRequest<ProyectoDto>;

public class CreateProyectoCommandValidator : AbstractValidator<CreateProyectoCommand>
{
    public CreateProyectoCommandValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}
