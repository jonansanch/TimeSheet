namespace KPG.Timesheet.Application.Features.Users.Commands.AsignarEstructura;

public class AsignarEstructuraUsuarioCommandValidator : AbstractValidator<AsignarEstructuraUsuarioCommand>
{
    public AsignarEstructuraUsuarioCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El usuario es requerido.");

        // La existencia del supervisor, la ausencia de ciclos y la validez del puesto
        // se comprueban en IdentityService, que es quien tiene acceso a AspNetUsers.
        //
        // Que alguien sea su propio jefe SI se permite: es como se marca la cabeza de la
        // organizacion, porque todos tienen jefe y el de mas arriba es el suyo propio.

        RuleFor(x => x.PuestoId)
            .GreaterThan(0).When(x => x.PuestoId.HasValue)
            .WithMessage("El puesto indicado no es valido.");
    }
}
