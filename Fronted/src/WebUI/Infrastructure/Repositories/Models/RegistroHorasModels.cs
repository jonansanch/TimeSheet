namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record CreateRegistroHorasRequest(
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
    string    Lugar);

public record RegistroHorasResponse(
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

public record RegistroRecienteResponse(
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Descripcion,
    string Lugar);

public record UpdateDescripcionRegistroRequest(string Descripcion);

public record HistorialRegistroResponse(
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
