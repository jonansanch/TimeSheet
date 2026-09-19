using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

/// <summary>
/// Cada movimiento de un registro por la cadena de aprobacion: quien, cuando, en que
/// nivel y por que. Es historial, no estado: el estado vive en
/// <see cref="RegistroHoras.Estado"/>. Sirve para auditar y para mostrarle al empleado
/// quien rechazo su dia y con que comentario.
/// </summary>
public class AprobacionRegistro : BaseAuditableEntity
{
    public const string AccionAprobar          = "Aprobar";
    public const string AccionRechazar         = "Rechazar";
    public const string AccionRevertirAprobado = "RevertirAprobacion";
    public const string AccionRevertirRechazo  = "RevertirRechazo";
    public const string AccionReenviar         = "Reenviar";

    private AprobacionRegistro() { }

    public AprobacionRegistro(
        int registroHorasId,
        string accion,
        int? nivel,
        string actorUserId,
        string? comentario = null)
    {
        if (registroHorasId <= 0)
            throw new DomainRuleException("El registro es requerido.");
        if (string.IsNullOrWhiteSpace(accion))
            throw new DomainRuleException("La accion es requerida.");
        if (string.IsNullOrWhiteSpace(actorUserId))
            throw new DomainRuleException("El actor es requerido.");
        if (nivel is < 1 or > 3)
            throw new DomainRuleException("El nivel debe estar entre 1 y 3.");

        RegistroHorasId = registroHorasId;
        Accion          = accion.Trim();
        Nivel           = nivel;
        ActorUserId     = actorUserId.Trim();
        Comentario      = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
    }

    public int     RegistroHorasId { get; private set; }
    public string  Accion          { get; private set; } = string.Empty;

    /// <summary>Nivel de la cadena sobre el que se actuo. Nulo en acciones sin nivel, como reenviar.</summary>
    public int?    Nivel           { get; private set; }

    public string  ActorUserId     { get; private set; } = string.Empty;
    public string? Comentario      { get; private set; }
}
