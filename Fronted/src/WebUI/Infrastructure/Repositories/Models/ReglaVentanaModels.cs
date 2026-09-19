namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

/// <summary>
/// Excepción a la ventana de registro retroactivo. Aplica a una persona
/// (<c>UserId</c>) o a un rol (<c>Rol</c>), nunca a ambos.
/// </summary>
public record ReglaVentanaResponse(
    int Id,
    string? UserId,
    string? Rol,
    int Dias,
    bool Activo);

public record GuardarReglaVentanaRequest(string? UserId, string? Rol, int Dias);
