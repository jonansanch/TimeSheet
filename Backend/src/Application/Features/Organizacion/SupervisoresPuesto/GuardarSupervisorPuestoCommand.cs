using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Organizacion.SupervisoresPuesto;

/// <summary>
/// Alta o edicion de la regla que decide quien aprueba en primer nivel los registros
/// de un puesto. <paramref name="ClienteId"/> nulo = regla general para todos los clientes.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public record GuardarSupervisorPuestoCommand(
    int? Id,
    int PuestoId,
    int? ClienteId,
    string SupervisorUserId) : IRequest<SupervisorPuestoDto>;

public class GuardarSupervisorPuestoCommandValidator : AbstractValidator<GuardarSupervisorPuestoCommand>
{
    public GuardarSupervisorPuestoCommandValidator()
    {
        RuleFor(x => x.PuestoId).GreaterThan(0).WithMessage("El puesto es requerido.");
        RuleFor(x => x.ClienteId).GreaterThan(0).When(x => x.ClienteId.HasValue)
            .WithMessage("El cliente indicado no es valido.");
        RuleFor(x => x.SupervisorUserId).NotEmpty().WithMessage("El supervisor es requerido.");
    }
}

[Authorize(Roles = Roles.Admin)]
public record ToggleSupervisorPuestoCommand(int Id) : IRequest<SupervisorPuestoDto>;
