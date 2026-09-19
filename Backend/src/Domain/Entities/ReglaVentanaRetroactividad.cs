using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

/// <summary>
/// Excepcion a la ventana de registro retroactivo global
/// (<c>ParametrosSistema.VentanaRetroactividad</c>).
///
/// <para>
/// Una regla aplica a una persona concreta (<see cref="UserId"/>) o a un rol
/// (<see cref="Rol"/>), nunca a ambos. Al resolver, la regla de la persona gana sobre la
/// del rol, y la del rol sobre el valor global.
/// </para>
/// </summary>
public class ReglaVentanaRetroactividad : BaseAuditableEntity
{
    /// <summary>Tope de dias habiles, para que un error de dedo no abra la ventana de par en par.</summary>
    public const int MaxDias = 365;

    private ReglaVentanaRetroactividad() { }

    private ReglaVentanaRetroactividad(string? userId, string? rol, int dias)
    {
        ValidarDias(dias);
        UserId = userId;
        Rol    = rol;
        Dias   = dias;
        Activo = true;
    }

    public static ReglaVentanaRetroactividad ParaUsuario(string userId, int dias)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainRuleException("El usuario es requerido.");
        return new ReglaVentanaRetroactividad(userId.Trim(), null, dias);
    }

    public static ReglaVentanaRetroactividad ParaRol(string rol, int dias)
    {
        if (string.IsNullOrWhiteSpace(rol))
            throw new DomainRuleException("El rol es requerido.");
        return new ReglaVentanaRetroactividad(null, rol.Trim(), dias);
    }

    public string? UserId { get; private set; }
    public string? Rol    { get; private set; }
    public int     Dias   { get; private set; }
    public bool    Activo { get; private set; }

    /// <summary>Una regla de persona pesa mas que una de rol al resolver la ventana.</summary>
    public bool EsDeUsuario => UserId is not null;

    public void CambiarDias(int dias)
    {
        ValidarDias(dias);
        Dias = dias;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    private static void ValidarDias(int dias)
    {
        if (dias < 0)
            throw new DomainRuleException("Los dias de la ventana no pueden ser negativos.");
        if (dias > MaxDias)
            throw new DomainRuleException($"Los dias de la ventana no pueden superar {MaxDias}.");
    }
}
