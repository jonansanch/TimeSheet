namespace KPG.Timesheet.Domain.Enums;

/// <summary>Como se busca un <see cref="Entities.TerminoDescripcion"/> dentro del texto.</summary>
public enum TipoTermino
{
    /// <summary>Coincide como palabra completa (respeta limites de palabra).</summary>
    Palabra = 0,

    /// <summary>Coincide como frase exacta, tambien respetando limites de palabra al inicio y fin.</summary>
    Frase = 1
}
