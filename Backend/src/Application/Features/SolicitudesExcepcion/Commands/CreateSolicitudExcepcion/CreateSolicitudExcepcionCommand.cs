namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;

public record CreateSolicitudExcepcionCommand(
    DateOnly FechaRegistro,
    string Justificacion) : IRequest<SolicitudExcepcionDto>;
