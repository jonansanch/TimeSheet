namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record BitacoraResponse(int Total, List<BitacoraItemDto> Items);

public record BitacoraItemDto(
    int Id,
    string TipoEvento,
    string ActorId,
    string? ActorEmail,
    string? ActorNombre,
    string EntidadAfectada,
    string? EntidadId,
    DateTimeOffset Timestamp,
    string? MetadataJson);
