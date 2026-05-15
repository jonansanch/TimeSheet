using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.DeleteRegistroHoras;

public class DeleteRegistroHorasCommandHandler : IRequestHandler<DeleteRegistroHorasCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteRegistroHorasCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var registro = await _context.RegistrosHoras
            .FirstOrDefaultAsync(r => r.Id == request.RegistroId, cancellationToken);

        if (registro is null)
            throw new NotFoundException($"RegistroHoras con id '{request.RegistroId}' no fue encontrado.");

        // Supervisor/Admin pueden eliminar cualquier registro.
        // Restricción de equipo por Supervisor se aplicará cuando exista entidad Team.
        var isSupervisorOrAdmin = _user.Roles?.Contains(Roles.Admin) == true
            || _user.Roles?.Contains(Roles.Supervisor) == true;

        if (!isSupervisorOrAdmin && registro.UserId != _user.Id)
            throw new ForbiddenAccessException();

        _context.RegistrosHoras.Remove(registro);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
