namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetMetricasGlobales;

public record MetricasGlobalesResponse(
    DateOnly Desde,
    DateOnly Hasta,
    int TotalRegistros,
    decimal TotalHoras,
    int UsuariosActivos,
    int ClientesActivos,
    int PendientesHoy,
    List<TendenciaDiaDto> Tendencia);

public record TendenciaDiaDto(DateOnly Fecha, int TotalRegistros, decimal TotalHoras);
