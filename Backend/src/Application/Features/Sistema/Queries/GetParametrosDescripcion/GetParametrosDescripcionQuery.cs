using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using MediatR;

namespace KPG.Timesheet.Application.Features.Sistema.Queries.GetParametrosDescripcion;

public record ParametrosDescripcionDto(
    bool ValidacionActiva, int MinPalabras, int MinPalabrasContexto, SeveridadTermino SeveridadReglasBase);

/// <summary>
/// Parametros de la calidad de descripciones (ver Docs/plan-calidad-descripciones.md).
/// Solo Admin: es lo que decide si el catalogo avisa o bloquea para todo el mundo.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public record GetParametrosDescripcionQuery : IRequest<ParametrosDescripcionDto>;

public class GetParametrosDescripcionQueryHandler(IParametrosSistemaService parametrosSistema)
    : IRequestHandler<GetParametrosDescripcionQuery, ParametrosDescripcionDto>
{
    public async Task<ParametrosDescripcionDto> Handle(
        GetParametrosDescripcionQuery request, CancellationToken cancellationToken)
    {
        var defecto = ParametrosEvaluacionDescripcion.PorDefecto;

        var activoTexto = await parametrosSistema.GetTextoAsync(
            ParametrosSistema.DescripcionValidacionActiva, defecto.ValidacionActiva.ToString(), cancellationToken);
        var minPalabras = await parametrosSistema.GetIntAsync(
            ParametrosSistema.DescripcionMinPalabras, defecto.MinPalabras, cancellationToken);
        var minContexto = await parametrosSistema.GetIntAsync(
            ParametrosSistema.DescripcionMinPalabrasContexto, defecto.MinPalabrasContexto, cancellationToken);
        var severidadTexto = await parametrosSistema.GetTextoAsync(
            ParametrosSistema.DescripcionSeveridadReglasBase, defecto.SeveridadReglasBase.ToString(), cancellationToken);

        return new ParametrosDescripcionDto(
            bool.TryParse(activoTexto, out var activo) ? activo : defecto.ValidacionActiva,
            minPalabras,
            minContexto,
            Enum.TryParse<SeveridadTermino>(severidadTexto, ignoreCase: true, out var severidad)
                ? severidad
                : defecto.SeveridadReglasBase);
    }
}
