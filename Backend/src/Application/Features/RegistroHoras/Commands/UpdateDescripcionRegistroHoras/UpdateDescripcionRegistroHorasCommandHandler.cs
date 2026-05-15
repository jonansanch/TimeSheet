using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

public class UpdateDescripcionRegistroHorasCommandHandler : IRequestHandler<UpdateDescripcionRegistroHorasCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDescripcionRegistroHorasCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDescripcionRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var registro = await _context.RegistrosHoras
            .FirstOrDefaultAsync(r => r.Id == request.RegistroId, cancellationToken);

        if (registro is null)
            throw new NotFoundException($"RegistroHoras con id '{request.RegistroId}' no fue encontrado.");

        registro.UpdateDescripcion(request.Descripcion);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
