namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;

public record SolicitudExcepcionDto(
    int Id,
    string UserId,
    DateOnly FechaRegistro,
    string Justificacion,
    string Estado);
