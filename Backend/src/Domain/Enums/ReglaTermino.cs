namespace KPG.Timesheet.Domain.Enums;

/// <summary>
/// Cuando un <see cref="Entities.TerminoDescripcion"/> se considera una falla de calidad.
/// </summary>
public enum ReglaTermino
{
    /// <summary>Se marca cada vez que el termino aparece, sin importar el resto del texto.</summary>
    ProhibidoSiempre = 0,

    /// <summary>
    /// Se marca solo si, quitando el termino, las stopwords y el nombre del proyecto, no
    /// queda suficiente contexto (ver <c>Descripcion.MinPalabrasContexto</c>). Permite
    /// palabras legitimas como "reunion" cuando van acompañadas de detalle.
    /// </summary>
    GenericoSiVaSolo = 1
}
