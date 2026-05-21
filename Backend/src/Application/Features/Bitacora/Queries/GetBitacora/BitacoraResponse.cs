namespace KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;

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
