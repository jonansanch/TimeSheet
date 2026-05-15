namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Queries.GetSolicitudesExcepcion;

public record SolicitudExcepcionAdminDto(
    int Id,
    string UserId,
    string SolicitanteEmail,
    DateOnly FechaRegistro,
    string Justificacion,
    string Estado,
    DateTimeOffset Created);
