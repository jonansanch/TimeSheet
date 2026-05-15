namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;

public class CreateSolicitudExcepcionCommandValidator : AbstractValidator<CreateSolicitudExcepcionCommand>
{
    public CreateSolicitudExcepcionCommandValidator()
    {
        RuleFor(x => x.FechaRegistro)
            .NotEmpty().WithMessage("La fecha es requerida.");

        RuleFor(x => x.Justificacion)
            .NotEmpty().WithMessage("La justificación es requerida.")
            .MaximumLength(1000).WithMessage("La justificación no puede superar 1000 caracteres.");
    }
}
