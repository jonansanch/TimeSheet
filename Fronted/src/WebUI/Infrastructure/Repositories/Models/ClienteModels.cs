namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record ClienteResponse(int Id, string Nombre, bool Activo);
public record ClienteConProyectosResponse(int Id, string Nombre, List<string> ProyectosActivos);
public record CreateClienteRequest(string Nombre);
public record UpdateClienteRequest(string Nombre);

public record ProyectoResponse(int Id, string Nombre, int ClienteId, bool Activo);
public record CreateProyectoRequest(string Nombre);
public record UpdateProyectoRequest(string Nombre);
