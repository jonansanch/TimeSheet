using FluentValidation;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.UpdateEmpleado;

public class UpdateEmpleadoCommandValidator : AbstractValidator<UpdateEmpleadoCommand>
{
    public UpdateEmpleadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El id del empleado es requerido.");
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del empleado es requerido.")
            .MaximumLength(200).WithMessage("El nombre del empleado no puede superar 200 caracteres.");
    }
}
