using FluentValidation;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.CreateEmpleado;

public class CreateEmpleadoCommandValidator : AbstractValidator<CreateEmpleadoCommand>
{
    public CreateEmpleadoCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del empleado es requerido.")
            .MaximumLength(200).WithMessage("El nombre del empleado no puede superar 200 caracteres.");
    }
}
