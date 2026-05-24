using FluentValidation;
using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Users.Commands.AdminResetPassword;

[Authorize(Roles = Roles.Admin)]
public record AdminResetPasswordCommand(string UserId, string NewPassword) : IRequest;

public class AdminResetPasswordCommandValidator : AbstractValidator<AdminResetPasswordCommand>
{
    public AdminResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}

public class AdminResetPasswordCommandHandler : IRequestHandler<AdminResetPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public AdminResetPasswordCommandHandler(IIdentityService identityService, IBitacoraService bitacora, IUser user)
    {
        _identityService = identityService;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.AdminResetPasswordAsync(request.UserId, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure("NewPassword", e)));

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.ResetContrasena,
            _user.Id ?? "system", null,
            "AspNetUsers", request.UserId,
            new { ResetBy = _user.Id },
            cancellationToken);
    }
}
