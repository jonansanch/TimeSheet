using KPG.Timesheet.Domain.Enums;

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
    string    Descripcion,
    string    Lugar,
    // Sin esto el empleado no sabe si le aprobaron o le rechazaron el dia, ni por que:
    // el correo de rechazo llegaba y su propia pantalla no decia nada.
    EstadoAprobacion Estado,
    string?   ComentarioRechazo,
    // Quien firmo cada nivel y cuando. "Aprobado" a secas no dice nada sobre por
    // donde paso el registro ni quien lo reviso.
    IReadOnlyList<PasoAprobacionDto> Aprobaciones);

/// <summary>Un paso del registro por la cadena: quien actuo, en que nivel y cuando.</summary>
public record PasoAprobacionDto(
    int      Nivel,
    string   Accion,
    string   Actor,
    DateTimeOffset Fecha,
    string?  Comentario);
