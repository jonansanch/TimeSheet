namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record LugarTrabajoResponse(int Id, string Nombre, bool Activo);

public record CreateLugarTrabajoRequest(string Nombre);

public record UpdateLugarTrabajoRequest(string Nombre);
