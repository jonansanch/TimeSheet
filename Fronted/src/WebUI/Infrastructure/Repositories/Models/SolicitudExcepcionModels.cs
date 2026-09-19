namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

/// <summary>
/// La justificacion basta para pedir el permiso, pero si el empleado ya lleno el
/// formulario se envia todo junto: al aprobarse, el registro se crea solo y no
/// tiene que volver a teclearlo.
/// </summary>
public record CreateSolicitudExcepcionRequest(
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
    string?   Descripcion = null);

public record SolicitudExcepcionResponse(
    int Id,
    string UserId,
    DateOnly FechaRegistro,
    string Justificacion,
    string Estado);

public record MiSolicitudExcepcionResponse(
    int Id,
    DateOnly FechaRegistro,
    string Justificacion,
    string Estado,
    DateTimeOffset Created,
    RegistroAdjuntoResponse? Registro = null);

/// <summary>Resumen del registro que va adjunto: lo que se creara si se aprueba.</summary>
public record RegistroAdjuntoResponse(
    int     ProyectoId,
    string  Cliente,
    string  Proyecto,
    string  Horario,
    decimal TotalHoras,
    string  Modalidad,
    string  Recurso,
    string  Lugar,
    string  Descripcion);
