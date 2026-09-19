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

        // "Debe dejar llenar todo el registro, pero debe llenar la solicitud": si adjunta
        // el registro, tiene que venir completo; la justificacion es obligatoria siempre.
        When(x => x.TraeRegistro, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HoraEntrada1.HasValue || x.HoraEntrada2.HasValue || x.HoraEntrada3.HasValue)
                .WithMessage("Debe registrar al menos un horario.");

            RuleFor(x => x.Modalidad).NotEmpty().MaximumLength(100)
                .WithMessage("La modalidad es requerida.");
            RuleFor(x => x.Recurso).NotEmpty().MaximumLength(100)
                .WithMessage("El recurso es requerido.");
            RuleFor(x => x.Lugar).NotEmpty().MaximumLength(200)
                .WithMessage("El lugar es requerido.");
            RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(1000)
                .WithMessage("La descripcion es requerida.");
        });
    }
}
