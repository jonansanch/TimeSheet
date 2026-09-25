using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Users.Commands.AsignarEstructura;

public class AsignarEstructuraUsuarioCommandHandler(
    IIdentityService identityService,
    IApplicationDbContext context,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<AsignarEstructuraUsuarioCommand, UserAdminDto>
{
    public async Task<UserAdminDto> Handle(
        AsignarEstructuraUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var (result, user) = await identityService.AsignarEstructuraAsync(
            request.UserId, request.SupervisorUserId, request.PuestoId,
            request.CodigoPais, request.ActualizarCodigoPais, cancellationToken);

        if (!result.Succeeded || user is null)
            throw new ValidationException(
                result.Errors.Select(e => new ValidationFailure(nameof(request.UserId), e)));

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.CambioEstructuraUsuario,
            actor.Id ?? "system", null,
            "AspNetUsers", request.UserId,
            new { request.SupervisorUserId, request.PuestoId, request.CodigoPais, request.ActualizarCodigoPais },
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
