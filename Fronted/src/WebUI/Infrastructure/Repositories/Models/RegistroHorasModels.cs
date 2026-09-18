namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record CreateRegistroHorasRequest(
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    string    Cliente,
    string    Proyecto,
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
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Recurso,
    string    Descripcion,
    string    Lugar,
    bool      EsRetroactivo);

public record RegistroRecienteResponse(
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Descripcion,
    string Lugar);

public record UpdateDescripcionRegistroRequest(string Descripcion);

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
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Recurso,
    string    Descripcion);
