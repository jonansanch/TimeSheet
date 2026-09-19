namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;

/// <summary>
/// El empleado puede enviar la justificacion sola, o adjuntar el registro completo que
/// lleno en el formulario. Con adjunto, al aprobarse la solicitud el registro se crea
/// automaticamente y el empleado no vuelve a teclear nada.
/// </summary>
public record CreateSolicitudExcepcionCommand(
    DateOnly  FechaRegistro,
    string    Justificacion,
    int?      ProyectoId = null,
    TimeOnly? HoraEntrada1 = null,
    TimeOnly? HoraSalida1 = null,
    TimeOnly? HoraEntrada2 = null,
    TimeOnly? HoraSalida2 = null,
    TimeOnly? HoraEntrada3 = null,
    TimeOnly? HoraSalida3 = null,
    string?   Modalidad = null,
    string?   Recurso = null,
    string?   Lugar = null,
    string?   Descripcion = null) : IRequest<SolicitudExcepcionDto>
{
    /// <summary>True si el empleado lleno el formulario ademas de la justificacion.</summary>
    public bool TraeRegistro => ProyectoId.HasValue;
}
