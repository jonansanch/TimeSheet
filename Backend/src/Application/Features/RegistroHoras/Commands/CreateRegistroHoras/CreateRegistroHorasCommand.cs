using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public record CreateRegistroHorasCommand(
    DateOnly FechaRegistro,
    TurnoRegistro Turno,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Descripcion,
    string Lugar) : IRequest<RegistroHorasDto>;
