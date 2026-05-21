using KPG.Timesheet.Domain.Common;

namespace KPG.Timesheet.Domain.Entities;

public class BitacoraAuditoria : BaseEntity
{
    public string TipoEvento { get; private set; } = string.Empty;
    public string ActorId { get; private set; } = string.Empty;
    public string? ActorEmail { get; private set; }
    public string EntidadAfectada { get; private set; } = string.Empty;
    public string? EntidadId { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string? MetadataJson { get; private set; }

    private BitacoraAuditoria() { }

    public static BitacoraAuditoria Crear(
        string tipoEvento,
        string actorId,
        string? actorEmail,
        string entidadAfectada,
        string? entidadId,
        object? metadata,
        DateTimeOffset timestamp)
    {
        return new BitacoraAuditoria
        {
            TipoEvento = tipoEvento,
            ActorId = actorId,
            ActorEmail = actorEmail,
            EntidadAfectada = entidadAfectada,
            EntidadId = entidadId,
            Timestamp = timestamp,
            MetadataJson = metadata is not null
                ? System.Text.Json.JsonSerializer.Serialize(metadata)
                : null
        };
    }
}
