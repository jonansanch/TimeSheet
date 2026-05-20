namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetDistribucionHoras;

public record DistribucionHorasResponse(
    DateOnly Desde,
    DateOnly Hasta,
    decimal TotalHorasEquipo,
    List<DistribucionConsultorDto> Consultores);

public record DistribucionConsultorDto(
    string UserId,
    string Nombre,
    decimal TotalHoras);
