using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

public class SolicitudExcepcion : BaseAuditableEntity
{
    private SolicitudExcepcion() { }

    public SolicitudExcepcion(string userId, DateOnly fechaRegistro, string justificacion)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainRuleException("El valor es requerido.");
        if (string.IsNullOrWhiteSpace(justificacion))
            throw new DomainRuleException("El valor es requerido.");

        UserId = userId;
        FechaRegistro = fechaRegistro;
        Justificacion = justificacion.Trim();
        Estado = EstadoSolicitud.Pendiente;
    }

    public string UserId { get; private set; } = string.Empty;
    public DateOnly FechaRegistro { get; private set; }
    public string Justificacion { get; private set; } = string.Empty;
    public EstadoSolicitud Estado { get; private set; }

    public void Aprobar()
    {
        if (Estado != EstadoSolicitud.Pendiente)
            throw new DomainRuleException("Solo se pueden aprobar solicitudes pendientes.");
        Estado = EstadoSolicitud.Aprobada;
    }

    public void Rechazar()
    {
        if (Estado != EstadoSolicitud.Pendiente)
            throw new DomainRuleException("Solo se pueden rechazar solicitudes pendientes.");
        Estado = EstadoSolicitud.Rechazada;
    }
}
