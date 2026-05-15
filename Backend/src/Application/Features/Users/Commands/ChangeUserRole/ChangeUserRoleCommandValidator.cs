using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    private static readonly string[] ValidRoles = [Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado];

    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El usuario es requerido.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es requerido.")
            .Must(role => ValidRoles.Contains(role)).WithMessage("El rol seleccionado no es valido.");
    }
}
