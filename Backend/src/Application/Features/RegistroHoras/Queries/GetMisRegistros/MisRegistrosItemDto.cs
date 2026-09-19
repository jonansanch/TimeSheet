namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public record MisRegistrosItemDto(
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
    string    Descripcion);
