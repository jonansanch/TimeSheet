namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

/// <summary>
/// Dia de la semana en el que una persona (<c>UserId</c>) o un rol (<c>Rol</c>) no puede
/// registrar horas, salvo que tenga una excepcion aprobada.
/// </summary>
public record RestriccionDiaResponse(
    int Id,
    DayOfWeek DiaDelaSemana,
    string? UserId,
    string? Rol,
    bool Activo);

public record GuardarRestriccionDiaRequest(DayOfWeek DiaDelaSemana, string? UserId, string? Rol);
