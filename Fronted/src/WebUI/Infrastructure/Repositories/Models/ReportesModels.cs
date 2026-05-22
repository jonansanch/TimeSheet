namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record ReporteHorasResponse(
    DateOnly Desde,
    DateOnly Hasta,
    int TotalRegistros,
    decimal TotalHoras,
    List<ReporteHorasItemDto> Items);

public record ReporteHorasItemDto(
    string    UserId,
    string    NombreEmpleado,
    string    Email,
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntradaAM,
    TimeOnly? HoraSalidaAM,
    TimeOnly? HoraEntradaPM,
    TimeOnly? HoraSalidaPM,
    decimal   Horas,
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Lugar,
    string    Descripcion);
