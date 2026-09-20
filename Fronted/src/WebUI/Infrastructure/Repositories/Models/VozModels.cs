namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

/// <summary>
/// Campos detectados en el dictado. Todo opcional: lo que viene null se deja como estaba
/// en el formulario, en vez de pisarlo.
/// </summary>
public record InterpretacionVozResponse(
    DateOnly? Fecha,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    string?   Cliente,
    string?   Proyecto,
    string?   Modalidad,
    string?   Recurso,
    string?   Lugar,
    string?   Descripcion);
