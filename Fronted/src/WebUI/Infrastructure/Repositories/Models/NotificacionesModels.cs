namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record HistorialNotificacionesResponse(int Total, List<NotificacionItemDto> Items);

public record NotificacionItemDto(
    int Id,
    string UserId,
    string Nombre,
    string Email,
    DateOnly FechaReferencia,
    int DiasAcumulados,
    bool Exitoso,
    string? ErrorDetalle,
    DateTimeOffset FechaEnvio);
