namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record ClienteResponse(int Id, string Nombre, bool Activo);
/// <summary>Proyecto activo del cliente. Lleva Id porque el registro se guarda por ProyectoId.</summary>
public record ProyectoActivoResponse(int Id, string Nombre);

public record ClienteConProyectosResponse(int Id, string Nombre, List<ProyectoActivoResponse> ProyectosActivos);
public record CreateClienteRequest(string Nombre);
public record UpdateClienteRequest(string Nombre);

public record ProyectoResponse(int Id, string Nombre, int ClienteId, bool Activo, string? SupervisorUserId);
public record CreateProyectoRequest(string Nombre);
public record UpdateProyectoRequest(string Nombre, string? SupervisorUserId = null);
