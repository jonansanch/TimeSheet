using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

/// <summary>
/// Quien aprueba en primer nivel los registros hechos bajo un puesto (el campo "Recurso").
///
/// <para>
/// <see cref="ClienteId"/> es opcional a proposito. Sin cliente, la regla vale para todos
/// los clientes —el caso habitual—; con cliente, aplica solo a ese, lo que permite que un
/// mismo puesto tenga aprobador distinto segun la cuenta. La busqueda prefiere siempre la
/// regla especifica sobre la general.
/// </para>
/// <para>
/// El puesto se referencia por <see cref="PuestoId"/>, que apunta al catalogo
/// <see cref="Empleado"/> (que pese al nombre contiene puestos, no personas).
/// </para>
/// </summary>
public class SupervisorPuesto : BaseAuditableEntity
{
    private SupervisorPuesto() { }

    public SupervisorPuesto(int puestoId, string supervisorUserId, int? clienteId = null)
    {
        if (puestoId <= 0)
            throw new DomainRuleException("El puesto es requerido.");
        if (clienteId is <= 0)
            throw new DomainRuleException("El clienteId debe ser un valor positivo.");
        if (string.IsNullOrWhiteSpace(supervisorUserId))
            throw new DomainRuleException("El supervisor es requerido.");

        PuestoId         = puestoId;
        ClienteId        = clienteId;
        SupervisorUserId = supervisorUserId.Trim();
        Activo           = true;
    }

    public int     PuestoId         { get; private set; }
    public int?    ClienteId        { get; private set; }
    public string  SupervisorUserId { get; private set; } = string.Empty;
    public bool    Activo           { get; private set; }

    /// <summary>Una regla con cliente gana sobre la general al resolver el aprobador.</summary>
    public bool EsEspecificaDeCliente => ClienteId.HasValue;

    public void CambiarSupervisor(string supervisorUserId)
    {
        if (string.IsNullOrWhiteSpace(supervisorUserId))
            throw new DomainRuleException("El supervisor es requerido.");
        SupervisorUserId = supervisorUserId.Trim();
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
