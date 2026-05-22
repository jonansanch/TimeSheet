namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public record CreateRegistroHorasCommand(
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
    string    Lugar) : IRequest<RegistroHorasDto>;
