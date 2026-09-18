namespace KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

public record ReporteHorasResponse(
    DateOnly Desde,
    DateOnly Hasta,
    int PageNumber,
    int PageSize,
    int TotalRegistros,
    decimal TotalHoras,
    List<ReporteHorasItemDto> Items);

public record ReporteHorasItemDto(
    string    UserId,
    string    NombreEmpleado,
    string    Email,
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    decimal   Horas,
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Lugar,
    string    Descripcion);
