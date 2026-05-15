using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Queries.GetModalidades;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.ToggleModalidadActiva;

public record ToggleModalidadActivaCommand(int Id) : IRequest<ModalidadDto>;

public class ToggleModalidadActivaCommandHandler : IRequestHandler<ToggleModalidadActivaCommand, ModalidadDto>
{
    private readonly IApplicationDbContext _context;

    public ToggleModalidadActivaCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ModalidadDto> Handle(ToggleModalidadActivaCommand request, CancellationToken cancellationToken)
    {
        var modalidad = await _context.Modalidades
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Modalidad con id '{request.Id}' no fue encontrada.");

        if (modalidad.Activo)
            modalidad.Desactivar();
        else
            modalidad.Activar();

        await _context.SaveChangesAsync(cancellationToken);

        return new ModalidadDto(modalidad.Id, modalidad.Nombre, modalidad.Activo);
    }
}
