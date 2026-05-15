using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, UserAdminDto>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public DeactivateUserCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }

    public async Task<UserAdminDto> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_user.Id) && _user.Id == request.Id)
            throw new ForbiddenAccessException();

        var result = await _identityService.DeactivateUserAsync(request.Id, _user.Id);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(error => new ValidationFailure(nameof(request.Id), error)));
        }

        var users = await _identityService.GetUsersAsync(1, 100, "email", false, cancellationToken);
        return users.Items.FirstOrDefault(u => u.Id == request.Id)
            ?? throw new NotFoundException($"Usuario con id '{request.Id}' no fue encontrado.");
    }
}
