namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetRegistrosRecientes;

public record RegistroRecienteDto(
    int    ProyectoId,
    string Cliente,
    string Proyecto,
    string Modalidad,
    string Recurso,
    string Descripcion,
    string Lugar);
