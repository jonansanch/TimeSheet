using KPG.Timesheet.Domain.Exceptions;

namespace KPG.Timesheet.Domain.Entities;

public class RegistroHoras : BaseAuditableEntity
{
    private RegistroHoras() { }

    public RegistroHoras(
        string userId,
        DateOnly fechaRegistro,
        TimeOnly? horaEntradaAM,
        TimeOnly? horaSalidaAM,
        TimeOnly? horaEntradaPM,
        TimeOnly? horaSalidaPM,
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

        ValidateBloque(horaEntradaAM, horaSalidaAM, "AM");
        ValidateBloque(horaEntradaPM, horaSalidaPM, "PM");

        if (!horaEntradaAM.HasValue && !horaEntradaPM.HasValue)
            throw new DomainRuleException("Debe registrar al menos un turno (AM o PM).");

        UserId        = userId;
        FechaRegistro = fechaRegistro;
        HoraEntradaAM = horaEntradaAM;
        HoraSalidaAM  = horaSalidaAM;
        HoraEntradaPM = horaEntradaPM;
        HoraSalidaPM  = horaSalidaPM;
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

    public TimeOnly? HoraEntradaAM { get; private set; }
    public TimeOnly? HoraSalidaAM  { get; private set; }
    public TimeOnly? HoraEntradaPM { get; private set; }
    public TimeOnly? HoraSalidaPM  { get; private set; }

    public string Cliente     { get; private set; } = string.Empty;
    public string Proyecto    { get; private set; } = string.Empty;
    public string Modalidad   { get; private set; } = string.Empty;
    public string Recurso     { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Lugar       { get; private set; } = string.Empty;
    public bool   EsRetroactivo { get; private set; }

    // Computed helpers
    public bool TieneAM => HoraEntradaAM.HasValue;
    public bool TienePM => HoraEntradaPM.HasValue;

    public void UpdateDescripcion(string nuevaDescripcion)
    {
        ThrowIfBlank(nuevaDescripcion, nameof(nuevaDescripcion));
        if (nuevaDescripcion.Length > 1000)
            throw new DomainRuleException("La descripcion no puede superar 1000 caracteres.");
        Descripcion = nuevaDescripcion.Trim();
    }

    // Allows adding a previously-null turno block (upsert second turno of the day)
    public void SetBloqueAM(TimeOnly horaEntrada, TimeOnly horaSalida)
    {
        if (HoraEntradaAM.HasValue)
            throw new DomainRuleException("El turno AM ya fue registrado y no puede modificarse.");
        ValidateBloque(horaEntrada, horaSalida, "AM");
        HoraEntradaAM = horaEntrada;
        HoraSalidaAM  = horaSalida;
    }

    public void SetBloquePM(TimeOnly horaEntrada, TimeOnly horaSalida)
    {
        if (HoraEntradaPM.HasValue)
            throw new DomainRuleException("El turno PM ya fue registrado y no puede modificarse.");
        ValidateBloque(horaEntrada, horaSalida, "PM");
        HoraEntradaPM = horaEntrada;
        HoraSalidaPM  = horaSalida;
    }

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

    private static void ValidateBloque(TimeOnly? entrada, TimeOnly? salida, string turno)
    {
        if (entrada.HasValue != salida.HasValue)
            throw new DomainRuleException(
                $"El turno {turno} requiere tanto hora de entrada como hora de salida.");
        if (entrada.HasValue && salida!.Value <= entrada.Value)
            throw new DomainRuleException(
                $"La hora de salida {turno} debe ser mayor que la hora de entrada.");
    }

    private static void ValidateBloque(TimeOnly entrada, TimeOnly salida, string turno)
    {
        if (salida <= entrada)
            throw new DomainRuleException(
                $"La hora de salida {turno} debe ser mayor que la hora de entrada.");
    }

    private static void ThrowIfBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainRuleException($"'{parameterName}' es requerido.");
    }
}
