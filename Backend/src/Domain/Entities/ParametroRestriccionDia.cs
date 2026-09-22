using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

/// <summary>
/// Dia de la semana en el que una persona o un rol no puede registrar horas, salvo que
/// tenga una excepcion aprobada (<see cref="SolicitudExcepcion"/>).
///
/// <para>
/// Aplica a una persona concreta (<see cref="UserId"/>) o a un rol (<see cref="Rol"/>),
/// nunca a ambos. Puede haber varias restricciones para el mismo dia (una por rol, una
/// por persona): a diferencia de <see cref="ReglaVentanaRetroactividad"/>, aqui no hay un
/// valor que uno "gane" sobre otro — cualquier restriccion que coincida bloquea.
/// </para>
/// </summary>
public class ParametroRestriccionDia : BaseAuditableEntity
{
    private ParametroRestriccionDia() { }

    private ParametroRestriccionDia(DayOfWeek diaDelaSemana, string? userId, string? rol)
    {
        DiaDelaSemana = diaDelaSemana;
        UserId        = userId;
        Rol           = rol;
        Activo        = true;
    }

    public static ParametroRestriccionDia ParaUsuario(DayOfWeek diaDelaSemana, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainRuleException("El usuario es requerido.");
        return new ParametroRestriccionDia(diaDelaSemana, userId.Trim(), null);
    }

    public static ParametroRestriccionDia ParaRol(DayOfWeek diaDelaSemana, string rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
            throw new DomainRuleException("El rol es requerido.");
        return new ParametroRestriccionDia(diaDelaSemana, null, rol.Trim());
    }

    /// <summary>Dia de la semana que se restringe.</summary>
    public DayOfWeek DiaDelaSemana { get; private set; }

    /// <summary>Si se aplica a un rol completo. Nulo si es especifica de un usuario.</summary>
    public string? Rol { get; private set; }

    /// <summary>Si se aplica a un usuario especifico. Nulo si es general por rol.</summary>
    public string? UserId { get; private set; }

    /// <summary>Si esta activa; permite desactivar sin borrar el historial.</summary>
    public bool Activo { get; private set; }

    public bool EsDeUsuario => UserId is not null;

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
