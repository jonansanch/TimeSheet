using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

public class UpdateDescripcionRegistroHorasCommandHandler : IRequestHandler<UpdateDescripcionRegistroHorasCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public UpdateDescripcionRegistroHorasCommandHandler(IApplicationDbContext context, IBitacoraService bitacora, IUser user)
    {
        _context = context;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task Handle(UpdateDescripcionRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var registro = await _context.RegistrosHoras
            .FirstOrDefaultAsync(r => r.Id == request.RegistroId, cancellationToken);

        if (registro is null)
            throw new NotFoundException($"RegistroHoras con id '{request.RegistroId}' no fue encontrado.");

        registro.UpdateDescripcion(request.Descripcion);

        await _context.SaveChangesAsync(cancellationToken);

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.ModificacionDescripcion,
            _user.Id ?? "system", null,
            "RegistrosHoras", request.RegistroId.ToString(),
            new { OwnerUserId = registro.UserId },
            cancellationToken);
    }
}
