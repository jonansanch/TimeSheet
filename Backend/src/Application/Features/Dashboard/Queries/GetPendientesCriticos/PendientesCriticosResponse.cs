namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetPendientesCriticos;

public record PendientesCriticosResponse(
    int Umbral,
    List<PendienteCriticoDto> Pendientes);

public record PendienteCriticoDto(
    string UserId,
    string Nombre,
    string Email,
    int DiasSinRegistro);
