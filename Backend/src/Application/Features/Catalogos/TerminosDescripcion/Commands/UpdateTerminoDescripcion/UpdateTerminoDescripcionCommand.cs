using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.CreateTerminoDescripcion;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Queries.GetTerminosDescripcion;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.UpdateTerminoDescripcion;

public record UpdateTerminoDescripcionCommand(
    int Id,
    string Termino,
    TipoTermino Tipo,
    ReglaTermino Regla,
    SeveridadTermino Severidad,
    string Motivo,
    string? Sugerencia) : IRequest<TerminoDescripcionDto>;

public class UpdateTerminoDescripcionCommandValidator : AbstractValidator<UpdateTerminoDescripcionCommand>
{
    public UpdateTerminoDescripcionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Termino).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sugerencia).MaximumLength(300);
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Regla).IsInEnum();
        RuleFor(x => x.Severidad).IsInEnum();
    }
}

public class UpdateTerminoDescripcionCommandHandler(IApplicationDbContext context, IBitacoraService bitacora, IUser actor)
    : IRequestHandler<UpdateTerminoDescripcionCommand, TerminoDescripcionDto>
{
    public async Task<TerminoDescripcionDto> Handle(
        UpdateTerminoDescripcionCommand request, CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var termino = await context.TerminosDescripcion
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"TerminoDescripcion con id '{request.Id}' no fue encontrado.");

        termino.Actualizar(request.Termino, request.Tipo, request.Regla, request.Severidad, request.Motivo, request.Sugerencia);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(
                nameof(request.Termino), "Ya existe un termino equivalente (mismas letras, sin tildes) en el catalogo.")]);
        }

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.TerminoDescripcionActualizado,
            actorId, actor.Email,
            "TerminosDescripcion", termino.Id.ToString(),
            new { termino.Termino, Tipo = termino.Tipo.ToString(), Regla = termino.Regla.ToString(), Severidad = termino.Severidad.ToString() },
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return CreateTerminoDescripcionCommandHandler.ToDto(termino);
    }

    private static bool EsDuplicado(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("IX_TerminosDescripcion_TerminoNormalizado", StringComparison.OrdinalIgnoreCase);
    }
}
