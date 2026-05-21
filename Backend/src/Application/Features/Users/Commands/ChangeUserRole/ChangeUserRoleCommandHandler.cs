using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, UserAdminDto>
{
    private readonly IIdentityService _identityService;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public ChangeUserRoleCommandHandler(IIdentityService identityService, IBitacoraService bitacora, IUser user)
    {
        _identityService = identityService;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task<UserAdminDto> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_user.Id) && _user.Id == request.UserId)
            throw new ForbiddenAccessException();

        var (result, user) = await _identityService.ChangeUserRoleAsync(
            request.UserId,
            request.Role,
            cancellationToken);

        if (!result.Succeeded || user is null)
        {
            throw new ValidationException(result.Errors.Select(error => new ValidationFailure(nameof(request.Role), error)));
        }

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.CambioRol,
            _user.Id ?? "system", null,
            "AspNetUsers", request.UserId,
            new { NuevoRol = user.Role },
            cancellationToken);

        return user;
    }
}
