using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public DeleteUserCommandHandler(IApplicationDbContext context, IIdentityService identityService, IBitacoraService bitacora, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task<DeleteUserDto> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_user.Id) && _user.Id == request.Id)
            throw new ForbiddenAccessException();

        var hasHistory = await _context.RegistrosHoras.AnyAsync(r => r.UserId == request.Id, cancellationToken)
            || await _context.SolicitudesExcepcion.AnyAsync(s => s.UserId == request.Id, cancellationToken);

        if (hasHistory)
        {
            var deactivateResult = await _identityService.DeactivateUserAsync(request.Id, _user.Id);
            if (!deactivateResult.Succeeded)
            {
                throw new ValidationException(deactivateResult.Errors.Select(error => new ValidationFailure(nameof(request.Id), error)));
            }

            await _bitacora.RegistrarAsync(
                TipoEventoBitacora.EliminacionUsuario,
                _user.Id ?? "system", null,
                "AspNetUsers", request.Id,
                new { HardDelete = false },
                cancellationToken);

            return new DeleteUserDto(false);
        }

        var result = await _identityService.DeleteUserHardAsync(request.Id);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(error => new ValidationFailure(nameof(request.Id), error)));
        }

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.EliminacionUsuario,
            _user.Id ?? "system", null,
            "AspNetUsers", request.Id,
            new { HardDelete = true },
            cancellationToken);

        return new DeleteUserDto(true);
    }
}
