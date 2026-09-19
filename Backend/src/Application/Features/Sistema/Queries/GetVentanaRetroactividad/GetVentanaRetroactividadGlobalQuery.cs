using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Sistema.Queries.GetVentanaRetroactividad;

/// <summary>
/// Valor global de la ventana, sin aplicar excepciones por persona o rol.
/// Es lo que la pantalla de parametros edita; para saber cuanto puede registrar
/// alguien en concreto se usa <see cref="GetVentanaRetroactividadQuery"/>.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public record GetVentanaRetroactividadGlobalQuery : IRequest<int>;

public class GetVentanaRetroactividadGlobalQueryHandler(IVentanaRetroactividadService ventana)
    : IRequestHandler<GetVentanaRetroactividadGlobalQuery, int>
{
    public Task<int> Handle(GetVentanaRetroactividadGlobalQuery request, CancellationToken cancellationToken)
        => ventana.GetDiasGlobalAsync(cancellationToken);
}

/// <summary>
/// Corte con el que el supervisor revisa: "Semanal" o "Quincenal". Lo consulta la
/// pantalla de aprobaciones para saber de cuantos dias es el periodo inicial.
/// </summary>
public record GetPeriodoAprobacionQuery : IRequest<string>;

public class GetPeriodoAprobacionQueryHandler(IParametrosSistemaService parametros)
    : IRequestHandler<GetPeriodoAprobacionQuery, string>
{
    /// <summary>Si el parametro falta o trae basura, semanal es el corte mas comun.</summary>
    private const string PorDefecto = "Semanal";

    public async Task<string> Handle(GetPeriodoAprobacionQuery request, CancellationToken cancellationToken)
    {
        var valor = await parametros.GetTextoAsync(
            KPG.Timesheet.Domain.Constants.ParametrosSistema.PeriodoAprobacion, PorDefecto, cancellationToken);

        return string.Equals(valor, "Quincenal", StringComparison.OrdinalIgnoreCase)
            ? "Quincenal"
            : PorDefecto;
    }
}
