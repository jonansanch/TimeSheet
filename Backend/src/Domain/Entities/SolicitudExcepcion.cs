using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

/// <summary>
/// Peticion para registrar un dia fuera de la ventana permitida.
///
/// <para>
/// Puede llevar adjunto el registro completo: el empleado llena el formulario y la
/// justificacion de una vez, y al aprobarse el registro se crea solo. Antes tenia que
/// pedir el permiso, esperar, y volver a llenar todo.
/// </para>
/// <para>
/// El adjunto es opcional para no invalidar las solicitudes creadas antes de esto.
/// </para>
/// </summary>
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

    // ── Registro adjunto (opcional) ──────────────────────────────────────────

    public int?      ProyectoId     { get; private set; }
    public string?   ClienteNombre  { get; private set; }
    public string?   ProyectoNombre { get; private set; }
    public TimeOnly? HoraEntrada1   { get; private set; }
    public TimeOnly? HoraSalida1    { get; private set; }
    public TimeOnly? HoraEntrada2   { get; private set; }
    public TimeOnly? HoraSalida2    { get; private set; }
    public TimeOnly? HoraEntrada3   { get; private set; }
    public TimeOnly? HoraSalida3    { get; private set; }
    public string?   Modalidad      { get; private set; }
    public string?   Recurso        { get; private set; }
    public string?   Lugar          { get; private set; }
    public string?   Descripcion    { get; private set; }

    /// <summary>Si trae registro adjunto, al aprobar la solicitud se crea automaticamente.</summary>
    public bool TieneRegistro => ProyectoId.HasValue;

    /// <summary>
    /// Adjunta el registro que el empleado lleno junto con la justificacion. Se valida
    /// aqui lo mismo que exigiria el registro, para no descubrir el problema al aprobar.
    /// </summary>
    public void AdjuntarRegistro(
        int proyectoId,
        string clienteNombre,
        string proyectoNombre,
        TimeOnly? horaEntrada1, TimeOnly? horaSalida1,
        TimeOnly? horaEntrada2, TimeOnly? horaSalida2,
        TimeOnly? horaEntrada3, TimeOnly? horaSalida3,
        string modalidad,
        string recurso,
        string lugar,
        string descripcion)
    {
        if (Estado != EstadoSolicitud.Pendiente)
            throw new DomainRuleException("Solo se puede adjuntar el registro a una solicitud pendiente.");
        if (proyectoId <= 0)
            throw new DomainRuleException("El proyecto es requerido.");
        if (!horaEntrada1.HasValue && !horaEntrada2.HasValue && !horaEntrada3.HasValue)
            throw new DomainRuleException("Debe registrar al menos un horario.");

        ValidarBloque(horaEntrada1, horaSalida1, 1);
        ValidarBloque(horaEntrada2, horaSalida2, 2);
        ValidarBloque(horaEntrada3, horaSalida3, 3);

        ThrowIfBlank(clienteNombre,  nameof(clienteNombre));
        ThrowIfBlank(proyectoNombre, nameof(proyectoNombre));
        ThrowIfBlank(modalidad,      nameof(modalidad));
        ThrowIfBlank(recurso,        nameof(recurso));
        ThrowIfBlank(lugar,          nameof(lugar));
        ThrowIfBlank(descripcion,    nameof(descripcion));

        ProyectoId     = proyectoId;
        ClienteNombre  = clienteNombre.Trim();
        ProyectoNombre = proyectoNombre.Trim();
        HoraEntrada1   = horaEntrada1;
        HoraSalida1    = horaSalida1;
        HoraEntrada2   = horaEntrada2;
        HoraSalida2    = horaSalida2;
        HoraEntrada3   = horaEntrada3;
        HoraSalida3    = horaSalida3;
        Modalidad      = modalidad.Trim();
        Recurso        = recurso.Trim();
        Lugar          = lugar.Trim();
        Descripcion    = descripcion.Trim();
    }

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

    private static void ValidarBloque(TimeOnly? entrada, TimeOnly? salida, int numero)
    {
        if (entrada.HasValue != salida.HasValue)
            throw new DomainRuleException(
                $"El horario {numero} requiere tanto hora de entrada como hora de salida.");
        if (entrada.HasValue && salida!.Value <= entrada.Value)
            throw new DomainRuleException(
                $"La hora de salida del horario {numero} debe ser mayor que la hora de entrada.");
    }

    private static void ThrowIfBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainRuleException($"'{parameterName}' es requerido.");
    }
}
