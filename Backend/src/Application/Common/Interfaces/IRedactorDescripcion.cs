namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Reescribe una descripcion de registro de horas con IA, siguiendo el formato de la guia
/// (ver Docs/plan-calidad-descripciones.md). Vive detras del backend por la misma razon que
/// <see cref="IInterpreteVoz"/>: la API key no puede viajar al navegador.
///
/// <para>
/// Nunca reemplaza el texto por su cuenta: devuelve una propuesta que el usuario acepta o
/// descarta desde el formulario. No inventa hechos nuevos que el usuario no haya mencionado.
/// </para>
/// </summary>
public interface IRedactorDescripcion
{
    /// <summary>
    /// False cuando no hay API key configurada. El boton de mejorar se oculta. La key vive en
    /// <see cref="Domain.Entities.ParametroSistema"/>, por eso la consulta es asincrona.
    /// </summary>
    Task<bool> DisponibleAsync(CancellationToken cancellationToken = default);

    /// <summary>Tope de usos por usuario por dia; lo hace cumplir quien llama (el command handler).</summary>
    int LimiteDiarioPorUsuario { get; }

    Task<PropuestaDescripcionDto> MejorarAsync(
        string texto,
        string? nombreCliente,
        string? nombreProyecto,
        IReadOnlyList<string> hallazgosActuales,
        CancellationToken cancellationToken = default);
}

/// <param name="Propuesta">Null si el modelo no pudo mejorar el texto (rechazo, respuesta vacia).</param>
/// <param name="Cambios">Resumen breve de que se cambio y por que, para mostrarle al usuario.</param>
public record PropuestaDescripcionDto(string? Propuesta, IReadOnlyList<string> Cambios)
{
    public static PropuestaDescripcionDto Vacia { get; } = new(null, []);
}
