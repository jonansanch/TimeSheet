using FluentValidation;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using MediatR;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.UpdateProyecto;

public record UpdateProyectoCommand(int Id, string Nombre) : IRequest<ProyectoDto>;

public class UpdateProyectoCommandValidator : AbstractValidator<UpdateProyectoCommand>
{
    public UpdateProyectoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}
