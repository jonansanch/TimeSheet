namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record CreateSolicitudExcepcionRequest(DateOnly FechaRegistro, string Justificacion);

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
    DateTimeOffset Created);
