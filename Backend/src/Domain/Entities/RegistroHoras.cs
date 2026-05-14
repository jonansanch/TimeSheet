using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Domain.Entities;

public class RegistroHoras : BaseAuditableEntity
{
    private RegistroHoras() { }

    public RegistroHoras(
        string userId,
        DateOnly fechaRegistro,
        TurnoRegistro turno,
        TimeOnly horaEntrada,
        TimeOnly horaSalida,
        string cliente,
        string proyecto,
        string modalidad,
        string recurso,
        string descripcion,
        string lugar)
    {
        ThrowIfBlank(userId, nameof(userId));
        ThrowIfBlank(cliente, nameof(cliente));
        ThrowIfBlank(proyecto, nameof(proyecto));
        ThrowIfBlank(modalidad, nameof(modalidad));
        ThrowIfBlank(recurso, nameof(recurso));
        ThrowIfBlank(descripcion, nameof(descripcion));
        ThrowIfBlank(lugar, nameof(lugar));

        if (horaSalida <= horaEntrada)
            throw new ArgumentException("La hora de salida debe ser mayor que la hora de entrada.", nameof(horaSalida));

        UserId = userId;
        FechaRegistro = fechaRegistro;
        Turno = turno;
        HoraEntrada = horaEntrada;
        HoraSalida = horaSalida;
        Cliente = cliente.Trim();
        Proyecto = proyecto.Trim();
        Modalidad = modalidad.Trim();
        Recurso = recurso.Trim();
        Descripcion = descripcion.Trim();
        Lugar = lugar.Trim();
    }

    public string UserId { get; private set; } = string.Empty;
    public DateOnly FechaRegistro { get; private set; }
    public TurnoRegistro Turno { get; private set; }
    public TimeOnly HoraEntrada { get; private set; }
    public TimeOnly HoraSalida { get; private set; }
    public string Cliente { get; private set; } = string.Empty;
    public string Proyecto { get; private set; } = string.Empty;
    public string Modalidad { get; private set; } = string.Empty;
    public string Recurso { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Lugar { get; private set; } = string.Empty;

    private static void ThrowIfBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El valor es requerido.", parameterName);
    }
}
