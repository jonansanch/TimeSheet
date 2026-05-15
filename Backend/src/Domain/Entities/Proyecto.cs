using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

public class Proyecto : BaseAuditableEntity
{
    private Proyecto() { }

    public Proyecto(int clienteId, string nombre)
    {
        if (clienteId <= 0)
            throw new DomainRuleException("El clienteId debe ser un valor positivo.");
        ValidarNombre(nombre);
        ClienteId = clienteId;
        Nombre = nombre.Trim();
        Activo = true;
    }

    public string Nombre { get; private set; } = string.Empty;
    public int ClienteId { get; private set; }
    public bool Activo { get; private set; }

    public void ActualizarNombre(string nombre)
    {
        ValidarNombre(nombre);
        Nombre = nombre.Trim();
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainRuleException("El nombre del proyecto es requerido.");
        if (nombre.Trim().Length > 200)
            throw new DomainRuleException("El nombre del proyecto no puede superar 200 caracteres.");
    }
}
