using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Sistema.Commands.UpdateParametrosDescripcion;

/// <summary>
/// Actualiza en bloque los 4 parametros de calidad de descripciones (ver
/// Docs/plan-calidad-descripciones.md). Van juntos porque la pantalla de administracion
/// los edita como un solo formulario.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public record UpdateParametrosDescripcionCommand(
    bool ValidacionActiva, int MinPalabras, int MinPalabrasContexto, SeveridadTermino SeveridadReglasBase) : IRequest;

public class UpdateParametrosDescripcionCommandValidator : AbstractValidator<UpdateParametrosDescripcionCommand>
{
    public UpdateParametrosDescripcionCommandValidator()
    {
        RuleFor(x => x.MinPalabras).InclusiveBetween(0, 50);
        RuleFor(x => x.MinPalabrasContexto).InclusiveBetween(0, 50);
        RuleFor(x => x.SeveridadReglasBase).IsInEnum();
    }
}

public class UpdateParametrosDescripcionCommandHandler(IApplicationDbContext context, IBitacoraService bitacora, IUser actor)
    : IRequestHandler<UpdateParametrosDescripcionCommand>
{
    public async Task Handle(UpdateParametrosDescripcionCommand request, CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        await UpsertAsync(ParametrosSistema.DescripcionValidacionActiva, request.ValidacionActiva.ToString(), cancellationToken);
        await UpsertAsync(ParametrosSistema.DescripcionMinPalabras, request.MinPalabras.ToString(), cancellationToken);
        await UpsertAsync(ParametrosSistema.DescripcionMinPalabrasContexto, request.MinPalabrasContexto.ToString(), cancellationToken);
        await UpsertAsync(ParametrosSistema.DescripcionSeveridadReglasBase, request.SeveridadReglasBase.ToString(), cancellationToken);

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.CambioParametrosDescripcion,
            actorId, actor.Email,
            "ParametrosSistema", null,
            new
            {
                request.ValidacionActiva,
                request.MinPalabras,
                request.MinPalabrasContexto,
                SeveridadReglasBase = request.SeveridadReglasBase.ToString()
            },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertAsync(string clave, string valor, CancellationToken cancellationToken)
    {
        var parametro = await context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == clave, cancellationToken);

        if (parametro is not null)
            parametro.Valor = valor;
        else
            context.ParametrosSistema.Add(new ParametroSistema { Clave = clave, Valor = valor });
    }
}
