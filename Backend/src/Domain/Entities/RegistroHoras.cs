using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

public class RegistroHoras : BaseAuditableEntity
{
    /// <summary>Cantidad de bloques horarios que admite un registro diario.</summary>
    public const int TotalBloques = 3;

    private RegistroHoras() { }

    public RegistroHoras(
        string userId,
        DateOnly fechaRegistro,
        TimeOnly? horaEntrada1,
        TimeOnly? horaSalida1,
        TimeOnly? horaEntrada2,
        TimeOnly? horaSalida2,
        TimeOnly? horaEntrada3,
        TimeOnly? horaSalida3,
        string cliente,
        string proyecto,
        string modalidad,
        string recurso,
        string descripcion,
        string lugar,
        bool esRetroactivo = false)
    {
        ThrowIfBlank(userId, nameof(userId));
        ThrowIfBlank(cliente, nameof(cliente));
        ThrowIfBlank(proyecto, nameof(proyecto));
        ThrowIfBlank(modalidad, nameof(modalidad));
        ThrowIfBlank(recurso, nameof(recurso));
        ThrowIfBlank(descripcion, nameof(descripcion));
        ThrowIfBlank(lugar, nameof(lugar));

        ValidateBloque(horaEntrada1, horaSalida1, 1);
        ValidateBloque(horaEntrada2, horaSalida2, 2);
        ValidateBloque(horaEntrada3, horaSalida3, 3);

        if (!horaEntrada1.HasValue && !horaEntrada2.HasValue && !horaEntrada3.HasValue)
            throw new DomainRuleException("Debe registrar al menos un horario.");

        UserId        = userId;
        FechaRegistro = fechaRegistro;
        HoraEntrada1  = horaEntrada1;
        HoraSalida1   = horaSalida1;
        HoraEntrada2  = horaEntrada2;
        HoraSalida2   = horaSalida2;
        HoraEntrada3  = horaEntrada3;
        HoraSalida3   = horaSalida3;
        Cliente       = cliente.Trim();
        Proyecto      = proyecto.Trim();
        Modalidad     = modalidad.Trim();
        Recurso       = recurso.Trim();
        Descripcion   = descripcion.Trim();
        Lugar         = lugar.Trim();
        EsRetroactivo = esRetroactivo;
    }

    public string  UserId        { get; private set; } = string.Empty;
    public DateOnly FechaRegistro { get; private set; }

    public TimeOnly? HoraEntrada1 { get; private set; }
    public TimeOnly? HoraSalida1  { get; private set; }
    public TimeOnly? HoraEntrada2 { get; private set; }
    public TimeOnly? HoraSalida2  { get; private set; }
    public TimeOnly? HoraEntrada3 { get; private set; }
    public TimeOnly? HoraSalida3  { get; private set; }

    public string Cliente     { get; private set; } = string.Empty;
    public string Proyecto    { get; private set; } = string.Empty;
    public string Modalidad   { get; private set; } = string.Empty;
    public string Recurso     { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Lugar       { get; private set; } = string.Empty;
    public bool   EsRetroactivo { get; private set; }

    // Computed helpers
    public bool TieneHorario1 => HoraEntrada1.HasValue;
    public bool TieneHorario2 => HoraEntrada2.HasValue;
    public bool TieneHorario3 => HoraEntrada3.HasValue;

    /// <summary>Suma en minutos de los bloques horarios completos del dia.</summary>
    public int TotalMinutos =>
        Minutos(HoraEntrada1, HoraSalida1) +
        Minutos(HoraEntrada2, HoraSalida2) +
        Minutos(HoraEntrada3, HoraSalida3);

    public void UpdateDescripcion(string nuevaDescripcion)
    {
        ThrowIfBlank(nuevaDescripcion, nameof(nuevaDescripcion));
        if (nuevaDescripcion.Length > 1000)
            throw new DomainRuleException("La descripcion no puede superar 1000 caracteres.");
        Descripcion = nuevaDescripcion.Trim();
    }

    /// <summary>
    /// Agrega un bloque horario que aun no existia (upsert de un horario adicional del dia).
    /// Un bloque ya registrado es inmutable.
    /// </summary>
    public void SetBloque(int numero, TimeOnly horaEntrada, TimeOnly horaSalida)
    {
        if (numero is < 1 or > TotalBloques)
            throw new DomainRuleException($"El numero de horario debe estar entre 1 y {TotalBloques}.");

        if (GetHoraEntrada(numero).HasValue)
            throw new DomainRuleException($"El horario {numero} ya fue registrado y no puede modificarse.");

        if (horaSalida <= horaEntrada)
            throw new DomainRuleException(
                $"La hora de salida del horario {numero} debe ser mayor que la hora de entrada.");

        switch (numero)
        {
            case 1: HoraEntrada1 = horaEntrada; HoraSalida1 = horaSalida; break;
            case 2: HoraEntrada2 = horaEntrada; HoraSalida2 = horaSalida; break;
            case 3: HoraEntrada3 = horaEntrada; HoraSalida3 = horaSalida; break;
        }
    }

    public TimeOnly? GetHoraEntrada(int numero) => numero switch
    {
        1 => HoraEntrada1,
        2 => HoraEntrada2,
        3 => HoraEntrada3,
        _ => throw new DomainRuleException($"El numero de horario debe estar entre 1 y {TotalBloques}.")
    };

    public TimeOnly? GetHoraSalida(int numero) => numero switch
    {
        1 => HoraSalida1,
        2 => HoraSalida2,
        3 => HoraSalida3,
        _ => throw new DomainRuleException($"El numero de horario debe estar entre 1 y {TotalBloques}.")
    };

    public void UpdateMetadata(string cliente, string proyecto, string modalidad, string recurso, string lugar)
    {
        ThrowIfBlank(cliente,   nameof(cliente));
        ThrowIfBlank(proyecto,  nameof(proyecto));
        ThrowIfBlank(modalidad, nameof(modalidad));
        ThrowIfBlank(recurso,   nameof(recurso));
        ThrowIfBlank(lugar,     nameof(lugar));
        Cliente   = cliente.Trim();
        Proyecto  = proyecto.Trim();
        Modalidad = modalidad.Trim();
        Recurso   = recurso.Trim();
        Lugar     = lugar.Trim();
    }

    private static int Minutos(TimeOnly? entrada, TimeOnly? salida) =>
        entrada.HasValue && salida.HasValue
            ? (int)(salida.Value - entrada.Value).TotalMinutes
            : 0;

    private static void ValidateBloque(TimeOnly? entrada, TimeOnly? salida, int numero)
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
