using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Application.Common.Validation;

/// <summary>
/// Regla de FluentValidation compartida por los tres comandos que guardan una
/// descripcion (crear, crear por rango, editar). Solo falla si
/// <see cref="IValidadorDescripcion"/> encuentra un hallazgo de severidad
/// <see cref="SeveridadTermino.Bloquear"/> — los de <see cref="SeveridadTermino.Advertir"/>
/// no impiden guardar, es el aviso en vivo del formulario el que los muestra. Ver
/// Docs/plan-calidad-descripciones.md.
/// </summary>
public static class DescripcionValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> DescripcionValida<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        IValidadorDescripcion validador,
        Func<T, CancellationToken, Task<int?>> resolverProyectoIdAsync) =>
        ruleBuilder.MustAsync(async (instancia, descripcion, contexto, cancellationToken) =>
        {
            var proyectoId = await resolverProyectoIdAsync(instancia, cancellationToken);
            var evaluacion = await validador.EvaluarAsync(descripcion, proyectoId, cancellationToken);

            if (!evaluacion.Bloquea)
                return true;

            var detalle = string.Join(" ", evaluacion.Hallazgos
                .Where(h => h.Severidad == SeveridadTermino.Bloquear)
                .Select(h => h.Sugerencia is null ? h.Mensaje : $"{h.Mensaje} Sugerencia: {h.Sugerencia}"));

            contexto.MessageFormatter.AppendArgument("Detalle", detalle);
            return false;
        })
        .WithMessage("La descripcion no cumple los lineamientos de calidad: {Detalle}");
}
