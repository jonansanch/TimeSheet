using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

public class LugarTrabajo : BaseAuditableEntity
{
    private LugarTrabajo() { }

    public LugarTrabajo(string nombre)
    {
        ValidarNombre(nombre);
        Nombre = nombre.Trim();
        Activo = true;
    }

    public string Nombre { get; private set; } = string.Empty;
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
            throw new DomainRuleException("El nombre del lugar de trabajo es requerido.");
        if (nombre.Trim().Length > 200)
            throw new DomainRuleException("El nombre del lugar de trabajo no puede superar 200 caracteres.");
    }
}
