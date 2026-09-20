namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record CreateRegistroHorasRequest(
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    int       ProyectoId,
    string    Modalidad,
    string    Recurso,
    string    Descripcion,
    string    Lugar);

public record RegistroHorasResponse(
    int       Id,
    string    UserId,
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    int       ProyectoId,
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Recurso,
    string    Descripcion,
    string    Lugar,
    bool      EsRetroactivo);

public record RegistroRecienteResponse(
    int    ProyectoId,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Descripcion,
    string Lugar);

/// <summary>Un metadato en null significa "dejalo como estaba".</summary>
public record UpdateDescripcionRegistroRequest(
    string  Descripcion,
    string? Modalidad = null,
    string? Recurso = null,
    string? Lugar = null);

/// <summary>Minutos registrados por dia del mes y umbral vigente de dia completo.</summary>
public record ResumenMensualResponse(
    int MinutosDiaCompleto,
    List<DiaResumenDto> Dias)
{
    public static ResumenMensualResponse Vacio(int minutosDiaCompleto = 480) => new(minutosDiaCompleto, []);
}

public record DiaResumenDto(DateOnly Fecha, int TotalMinutos);

public record HistorialPaginadoResponse(int TotalCount, List<HistorialRegistroResponse> Items);

public record HistorialRegistroResponse(
    int       Id,
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    int       ProyectoId,
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Recurso,
    string    Descripcion,
    string    Lugar,
    EstadoAprobacion Estado,
    string?   ComentarioRechazo,
    IReadOnlyList<PasoAprobacionResponse>? Aprobaciones)
{
    /// <summary>
    /// Solo las firmas de aprobacion, en orden de nivel.
    ///
    /// <para>
    /// Tolera que el campo no venga: una API mas vieja que este cliente no manda
    /// <c>aprobaciones</c>, y eso no puede tumbar la pantalla entera.
    /// </para>
    /// </summary>
    public IEnumerable<PasoAprobacionResponse> Firmas =>
        (Aprobaciones ?? []).Where(a => a.Accion == "Aprobar").OrderBy(a => a.Nivel);

    /// <summary>Un dia rechazado lo corrige y lo reenvia el propio empleado.</summary>
    public bool EsRechazado => Estado == EstadoAprobacion.Rechazado;

    /// <summary>Aprobado en los tres niveles: ya no se edita ni se elimina.</summary>
    public bool EstaCerrado => Estado == EstadoAprobacion.Aprobado;
}

/// <summary>Un paso del registro por la cadena de aprobacion.</summary>
public record PasoAprobacionResponse(
    int      Nivel,
    string   Accion,
    string   Actor,
    DateTimeOffset Fecha,
    string?  Comentario);
