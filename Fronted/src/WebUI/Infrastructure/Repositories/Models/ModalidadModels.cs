namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record ModalidadResponse(int Id, string Nombre, bool Activo);

public record CreateModalidadRequest(string Nombre);

public record UpdateModalidadRequest(string Nombre);
