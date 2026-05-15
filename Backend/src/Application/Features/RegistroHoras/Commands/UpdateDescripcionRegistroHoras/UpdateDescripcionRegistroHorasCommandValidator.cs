namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

public class UpdateDescripcionRegistroHorasCommandValidator : AbstractValidator<UpdateDescripcionRegistroHorasCommand>
{
    public UpdateDescripcionRegistroHorasCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.");
    }
}
