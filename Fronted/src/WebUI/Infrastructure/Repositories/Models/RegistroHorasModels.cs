namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public enum TurnoRegistro
{
    AM = 1,
    PM = 2
}

public record CreateRegistroHorasRequest(
    DateOnly FechaRegistro,
    TurnoRegistro Turno,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Descripcion,
    string Lugar);

public record RegistroHorasResponse(
    int Id,
    string UserId,
    DateOnly FechaRegistro,
    TurnoRegistro Turno,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Descripcion,
    string Lugar);

public record RegistroRecienteResponse(string Cliente, string Proyecto);

public record HistorialRegistroResponse(
    int Id,
    DateOnly FechaRegistro,
    TurnoRegistro Turno,
    TimeOnly HoraEntrada,
    TimeOnly HoraSalida,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Descripcion);
