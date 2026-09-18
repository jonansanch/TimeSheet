namespace KPG.Timesheet.Application.Features.Users.Commands.AsignarEstructura;

public class AsignarEstructuraUsuarioCommandValidator : AbstractValidator<AsignarEstructuraUsuarioCommand>
{
    public AsignarEstructuraUsuarioCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El usuario es requerido.");

        // La existencia del supervisor, la ausencia de ciclos y la validez del puesto
        // se comprueban en IdentityService, que es quien tiene acceso a AspNetUsers.
        RuleFor(x => x.SupervisorUserId)
            .Must((cmd, supervisor) => string.IsNullOrWhiteSpace(supervisor) || supervisor != cmd.UserId)
            .WithMessage("Un usuario no puede ser su propio supervisor.");

        RuleFor(x => x.PuestoId)
            .GreaterThan(0).When(x => x.PuestoId.HasValue)
            .WithMessage("El puesto indicado no es valido.");
    }
}
