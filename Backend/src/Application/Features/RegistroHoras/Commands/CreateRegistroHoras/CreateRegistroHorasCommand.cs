namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public record CreateRegistroHorasCommand(
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
    string    Lugar) : IRequest<RegistroHorasDto>;
