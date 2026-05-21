using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserAdminDto>
{
    private readonly IIdentityService _identityService;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public CreateUserCommandHandler(IIdentityService identityService, IBitacoraService bitacora, IUser user)
    {
        _identityService = identityService;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task<UserAdminDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.CreateUserAsync(request.Email.Trim(), request.Password, request.Role, request.NombreCompleto?.Trim());
        if (!result.Succeeded || user is null)
        {
            throw new ValidationException(result.Errors.Select(error => new ValidationFailure(nameof(request.Email), error)));
        }

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.AltaUsuario,
            _user.Id ?? "system", null,
            "AspNetUsers", user.Id,
            new { user.Email, user.Role },
            cancellationToken);

        return user;
    }
}
