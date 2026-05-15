using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Queries.GetLugaresTrabajo;
using KPG.Timesheet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.CreateLugarTrabajo;

public record CreateLugarTrabajoCommand(string Nombre) : IRequest<LugarTrabajoDto>;

public class CreateLugarTrabajoCommandValidator : AbstractValidator<CreateLugarTrabajoCommand>
{
    public CreateLugarTrabajoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public class CreateLugarTrabajoCommandHandler : IRequestHandler<CreateLugarTrabajoCommand, LugarTrabajoDto>
{
    private readonly IApplicationDbContext _context;

    public CreateLugarTrabajoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<LugarTrabajoDto> Handle(CreateLugarTrabajoCommand request, CancellationToken cancellationToken)
    {
        var lugar = new LugarTrabajo(request.Nombre);
        _context.LugaresTrabajo.Add(lugar);

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
