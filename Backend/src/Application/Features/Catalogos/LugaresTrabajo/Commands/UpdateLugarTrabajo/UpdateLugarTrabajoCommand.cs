using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Queries.GetLugaresTrabajo;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.UpdateLugarTrabajo;

public record UpdateLugarTrabajoCommand(int Id, string Nombre) : IRequest<LugarTrabajoDto>;

public class UpdateLugarTrabajoCommandValidator : AbstractValidator<UpdateLugarTrabajoCommand>
{
    public UpdateLugarTrabajoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public class UpdateLugarTrabajoCommandHandler : IRequestHandler<UpdateLugarTrabajoCommand, LugarTrabajoDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateLugarTrabajoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<LugarTrabajoDto> Handle(UpdateLugarTrabajoCommand request, CancellationToken cancellationToken)
    {
        var lugar = await _context.LugaresTrabajo
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Lugar de trabajo con id '{request.Id}' no fue encontrado.");

        lugar.ActualizarNombre(request.Nombre);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(
                nameof(request.Nombre), "Ya existe un lugar de trabajo con ese nombre.")]);
        }

        return new LugarTrabajoDto(lugar.Id, lugar.Nombre, lugar.Activo);
    }

    private static bool EsDuplicado(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("IX_LugaresTrabajo_Nombre", StringComparison.OrdinalIgnoreCase);
    }
}
