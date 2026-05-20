namespace KPG.Timesheet.Domain.Entities;

public class NotificacionEnviada : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly FechaReferencia { get; set; }
    public int DiasAcumulados { get; set; }
    public bool Exitoso { get; set; }
    public string? ErrorDetalle { get; set; }
}
