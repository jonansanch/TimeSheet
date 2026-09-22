using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

/// <summary>
/// Falla o mejora que un usuario reporta desde la aplicacion. Cualquier persona
/// autenticada puede crear uno; solo ve los suyos. Un Admin ve y gestiona todos.
/// </summary>
public class ReporteUsuario : BaseAuditableEntity
{
    private ReporteUsuario() { }

    public ReporteUsuario(string userId, TipoReporte tipo, string titulo, string descripcion)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainRuleException("El usuario es requerido.");
        ValidarTitulo(titulo);
        ValidarDescripcion(descripcion);

        UserId      = userId;
        Tipo        = tipo;
        Titulo      = titulo.Trim();
        Descripcion = descripcion.Trim();
        Estado      = EstadoReporte.Nuevo;
    }

    public string UserId      { get; private set; } = string.Empty;
    public TipoReporte Tipo   { get; private set; }
    public string Titulo      { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public EstadoReporte Estado { get; private set; }

    /// <summary>Nota del Admin al pasar a revision, resolver o rechazar. Opcional salvo al rechazar.</summary>
    public string? ComentarioRespuesta { get; private set; }

    /// <summary>Quien atendio el reporte por ultima vez. Nulo mientras sigue en 'Nuevo'.</summary>
    public string? RespondidoPorUserId { get; private set; }

    public void MarcarEnRevision(string actorUserId, string? comentario)
    {
        if (Estado is EstadoReporte.Resuelto or EstadoReporte.Rechazado)
            throw new DomainRuleException("El reporte ya fue cerrado: no puede volver a revision.");

        Estado               = EstadoReporte.EnRevision;
        ComentarioRespuesta  = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        RespondidoPorUserId  = actorUserId;
    }

    public void Resolver(string actorUserId, string? comentario)
    {
        if (Estado == EstadoReporte.Resuelto)
            throw new DomainRuleException("El reporte ya esta resuelto.");
        if (Estado == EstadoReporte.Rechazado)
            throw new DomainRuleException("El reporte esta rechazado: no puede marcarse como resuelto.");

        Estado               = EstadoReporte.Resuelto;
        ComentarioRespuesta  = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        RespondidoPorUserId  = actorUserId;
    }

    /// <summary>Al rechazar hay que explicar el motivo: mismo criterio que el resto del sistema.</summary>
    public void Rechazar(string actorUserId, string comentario)
    {
        if (Estado == EstadoReporte.Rechazado)
            throw new DomainRuleException("El reporte ya esta rechazado.");
        if (Estado == EstadoReporte.Resuelto)
            throw new DomainRuleException("El reporte esta resuelto: no puede rechazarse.");
        if (string.IsNullOrWhiteSpace(comentario))
            throw new DomainRuleException("El rechazo requiere un comentario que explique el motivo.");

        Estado               = EstadoReporte.Rechazado;
        ComentarioRespuesta  = comentario.Trim();
        RespondidoPorUserId  = actorUserId;
    }

    private static void ValidarTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainRuleException("El titulo es requerido.");
        if (titulo.Trim().Length > 200)
            throw new DomainRuleException("El titulo no puede superar 200 caracteres.");
    }

    private static void ValidarDescripcion(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainRuleException("La descripcion es requerida.");
        if (descripcion.Trim().Length > 2000)
            throw new DomainRuleException("La descripcion no puede superar 2000 caracteres.");
    }
}
