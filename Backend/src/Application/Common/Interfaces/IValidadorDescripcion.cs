using KPG.Timesheet.Application.Common.Services;

namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Punto unico para evaluar la calidad de una descripcion de registro de horas contra el
/// catalogo de <see cref="Domain.Entities.TerminoDescripcion"/> y los parametros vigentes
/// (ver Docs/plan-calidad-descripciones.md). Lo usan por igual los validadores de los
/// comandos de guardado y el endpoint <c>/api/descripciones/evaluar</c> del aviso en vivo,
/// asi ambos dan siempre el mismo resultado.
/// </summary>
public interface IValidadorDescripcion
{
    /// <param name="texto">La descripcion a evaluar.</param>
    /// <param name="proyectoId">
    /// Proyecto del registro, si se conoce. Habilita el prefijo "[Proyecto] - " en el
    /// conteo de palabras y descarta el nombre del proyecto como "contexto" de un termino
    /// generico. Null o &lt;= 0 evalua sin ese contexto.
    /// </param>
    Task<EvaluacionDescripcion> EvaluarAsync(
        string? texto, int? proyectoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evalua varias descripciones de una sola vez, cargando el catalogo y los parametros
    /// una unica vez en vez de una consulta por fila. Pensado para listados (bandeja de
    /// aprobaciones, reportes), donde <see cref="EvaluarAsync"/> uno por uno seria
    /// N+1 contra la base. El resultado respeta el orden de <paramref name="items"/>.
    /// </summary>
    Task<IReadOnlyList<EvaluacionDescripcion>> EvaluarVariasAsync(
        IReadOnlyList<(string? Texto, int? ProyectoId)> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Igual que <see cref="EvaluarVariasAsync"/>, pero para consultas (como los reportes
    /// de Dapper) que ya traen el nombre del proyecto en la fila y no tendria sentido
    /// resolver de vuelta a su Id solo para volver a buscar el nombre.
    /// </summary>
    Task<IReadOnlyList<EvaluacionDescripcion>> EvaluarVariasPorNombreProyectoAsync(
        IReadOnlyList<(string? Texto, string? NombreProyecto)> items, CancellationToken cancellationToken = default);
}
