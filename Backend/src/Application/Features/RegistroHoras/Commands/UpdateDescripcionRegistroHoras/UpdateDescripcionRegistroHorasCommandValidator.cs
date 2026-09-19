namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

public class UpdateDescripcionRegistroHorasCommandValidator : AbstractValidator<UpdateDescripcionRegistroHorasCommand>
{
    public UpdateDescripcionRegistroHorasCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.");

        // Opcionales: en null se conserva el valor actual, pero si vienen no pueden ir vacios.
        RuleFor(x => x.Modalidad)
            .NotEmpty().MaximumLength(100).When(x => x.Modalidad is not null)
            .WithMessage("La modalidad no es válida.");

        RuleFor(x => x.Recurso)
            .NotEmpty().MaximumLength(100).When(x => x.Recurso is not null)
            .WithMessage("El recurso no es válido.");

        RuleFor(x => x.Lugar)
            .NotEmpty().MaximumLength(200).When(x => x.Lugar is not null)
            .WithMessage("El lugar no es válido.");
    }
}
