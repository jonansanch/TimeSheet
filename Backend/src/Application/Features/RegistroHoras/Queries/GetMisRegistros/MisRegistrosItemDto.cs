using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public record MisRegistrosItemDto(
    int Id,
    DateOnly FechaRegistro,
    TurnoRegistro Turno,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Descripcion);
