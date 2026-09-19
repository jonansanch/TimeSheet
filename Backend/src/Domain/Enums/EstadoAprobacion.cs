namespace KPG.Timesheet.Domain.Enums;

/// <summary>
/// Avance de un registro por la cadena de aprobacion de tres niveles:
/// supervisor del puesto → jefe directo → responsable del proyecto.
/// </summary>
public enum EstadoAprobacion
{
    /// <summary>Registrado por el empleado, esperando la primera revision.</summary>
    Pendiente = 0,

    /// <summary>Aprobado por el supervisor del puesto; espera al jefe directo.</summary>
    AprobadoNivel1 = 1,

    /// <summary>Aprobado por el jefe directo; espera al responsable del proyecto.</summary>
    AprobadoNivel2 = 2,

    /// <summary>Aprobado en los tres niveles. Estado final.</summary>
    Aprobado = 3,

    /// <summary>
    /// Rechazado en algun nivel. Vuelve siempre al empleado, que corrige y reenvia;
    /// al reenviar la cadena recomienza desde el primer nivel.
    /// </summary>
    Rechazado = 4
}
