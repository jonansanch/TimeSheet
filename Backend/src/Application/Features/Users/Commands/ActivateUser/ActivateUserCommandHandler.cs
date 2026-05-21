using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserAdminDto>
{
    private readonly IIdentityService _identityService;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public ActivateUserCommandHandler(IIdentityService identityService, IBitacoraService bitacora, IUser user)
    {
        _identityService = identityService;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task<UserAdminDto> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ActivateUserAsync(request.Id);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(error => new ValidationFailure(nameof(request.Id), error)));
        }

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.ReactivacionUsuario,
            _user.Id ?? "system", null,
            "AspNetUsers", request.Id,
            null, cancellationToken);

        var users = await _identityService.GetUsersAsync(1, 100, "email", false, cancellationToken);
        return users.Items.FirstOrDefault(u => u.Id == request.Id)
            ?? throw new NotFoundException($"Usuario con id '{request.Id}' no fue encontrado.");
    }
}
