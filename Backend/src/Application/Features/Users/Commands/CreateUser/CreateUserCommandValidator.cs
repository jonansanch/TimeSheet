using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private static readonly string[] ValidRoles = [Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado];

    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El formato de email no es valido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contrasena es requerida.")
            .MinimumLength(6).WithMessage("La contrasena debe tener al menos 6 caracteres.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es requerido.")
            .Must(role => ValidRoles.Contains(role)).WithMessage("El rol seleccionado no es valido.");
    }
}
