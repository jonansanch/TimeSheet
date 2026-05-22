namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public record MisRegistrosItemDto(
    int       Id,
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntradaAM,
    TimeOnly? HoraSalidaAM,
    TimeOnly? HoraEntradaPM,
    TimeOnly? HoraSalidaPM,
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Descripcion);
