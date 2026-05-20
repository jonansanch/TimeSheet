using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserAdminDto>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserAdminDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.CreateUserAsync(request.Email.Trim(), request.Password, request.Role, request.NombreCompleto?.Trim());
        if (!result.Succeeded || user is null)
        {
            throw new ValidationException(result.Errors.Select(error => new ValidationFailure(nameof(request.Email), error)));
        }

        return user;
    }
}
