namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record SolicitudExcepcionAdminResponse(
    int Id,
    string UserId,
    string SolicitanteEmail,
    DateOnly FechaRegistro,
    string Justificacion,
    string Estado,
    DateTimeOffset Created);
