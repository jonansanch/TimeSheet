namespace KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

public record ReporteHorasResponse(
    DateOnly Desde,
    DateOnly Hasta,
    int TotalRegistros,
    decimal TotalHoras,
    List<ReporteHorasItemDto> Items);

public record ReporteHorasItemDto(
    string UserId,
    string NombreEmpleado,
    string Email,
    DateOnly FechaRegistro,
    int Turno,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    decimal Horas,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Lugar,
    string Descripcion);
