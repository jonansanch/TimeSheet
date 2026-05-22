namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public record RegistroHorasDto(
    int       Id,
    string    UserId,
    DateOnly  FechaRegistro,
    TimeOnly? HoraEntradaAM,
    TimeOnly? HoraSalidaAM,
    TimeOnly? HoraEntradaPM,
    TimeOnly? HoraSalidaPM,
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Recurso,
    string    Descripcion,
    string    Lugar,
    bool      EsRetroactivo);
