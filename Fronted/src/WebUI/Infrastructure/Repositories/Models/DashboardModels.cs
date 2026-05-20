namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record DistribucionHorasResponse(
    DateOnly Desde,
    DateOnly Hasta,
    decimal TotalHorasEquipo,
    List<DistribucionConsultorDto> Consultores);

public record DistribucionConsultorDto(
    string UserId,
    string Nombre,
    decimal TotalHoras);

public record EstadoEquipoResponse(
    DateOnly Fecha,
    int TotalEmpleados,
    int TotalCompletos,
    int TotalParciales,
    int TotalPendientes,
    List<MiembroEstadoDto> Equipo);

public record MiembroEstadoDto(
    string UserId,
    string Nombre,
    string Email,
    bool TieneAm,
    bool TienePm,
    string Estado);
