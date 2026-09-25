namespace KPG.Timesheet.Domain.Enums;

/// <summary>
/// Que tan estricto es un hallazgo de calidad de descripcion. Arranca en <see cref="Advertir"/>
/// para todo el catalogo semilla (ver Docs/fase0-linea-base-descripciones.md) y se sube a
/// <see cref="Bloquear"/> por termino, desde la administracion, una vez medido el impacto real.
/// </summary>
public enum SeveridadTermino
{
    /// <summary>Se muestra como aviso; no impide guardar.</summary>
    Advertir = 0,

    /// <summary>Impide guardar mientras la descripcion no se corrija.</summary>
    Bloquear = 1
}
