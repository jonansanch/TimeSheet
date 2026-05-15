namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record EmpleadoResponse(int Id, string Nombre, bool Activo);

public record CreateEmpleadoRequest(string Nombre);

public record UpdateEmpleadoRequest(string Nombre);
