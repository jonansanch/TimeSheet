using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, UserAdminDto>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public ChangeUserRoleCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
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

        return user;
    }
}
