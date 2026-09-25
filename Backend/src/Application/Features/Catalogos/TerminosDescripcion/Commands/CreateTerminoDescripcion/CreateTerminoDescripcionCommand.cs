using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Queries.GetTerminosDescripcion;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.CreateTerminoDescripcion;

public record CreateTerminoDescripcionCommand(
    string Termino,
    TipoTermino Tipo,
    ReglaTermino Regla,
    SeveridadTermino Severidad,
    string Motivo,
    string? Sugerencia) : IRequest<TerminoDescripcionDto>;

public class CreateTerminoDescripcionCommandValidator : AbstractValidator<CreateTerminoDescripcionCommand>
{
    public CreateTerminoDescripcionCommandValidator()
    {
        RuleFor(x => x.Termino).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sugerencia).MaximumLength(300);
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Regla).IsInEnum();
        RuleFor(x => x.Severidad).IsInEnum();
    }
}

public class CreateTerminoDescripcionCommandHandler(IApplicationDbContext context, IBitacoraService bitacora, IUser actor)
    : IRequestHandler<CreateTerminoDescripcionCommand, TerminoDescripcionDto>
{
    public async Task<TerminoDescripcionDto> Handle(
        CreateTerminoDescripcionCommand request, CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var termino = new TerminoDescripcion(
            request.Termino, request.Tipo, request.Regla, request.Severidad, request.Motivo, request.Sugerencia);
        context.TerminosDescripcion.Add(termino);

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
            TipoEventoBitacora.TerminoDescripcionCreado,
            actorId, actor.Email,
            "TerminosDescripcion", termino.Id.ToString(),
            new { termino.Termino, Tipo = termino.Tipo.ToString(), Regla = termino.Regla.ToString(), Severidad = termino.Severidad.ToString() },
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return ToDto(termino);
    }

    private static bool EsDuplicado(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("IX_TerminosDescripcion_TerminoNormalizado", StringComparison.OrdinalIgnoreCase);
    }

    internal static TerminoDescripcionDto ToDto(TerminoDescripcion t) =>
        new(t.Id, t.Termino, t.Tipo, t.Regla, t.Severidad, t.Sugerencia, t.Motivo, t.Activo);
}
