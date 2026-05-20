namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

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

public record DashboardGerencialResponse(
    DateOnly Desde,
    DateOnly Hasta,
    decimal TotalHoras,
    int TotalClientes,
    List<HorasPorClienteDto> PorCliente,
    List<HorasPorProyectoDto> PorProyecto);

public record HorasPorClienteDto(string Cliente, decimal TotalHoras);

public record HorasPorProyectoDto(string Proyecto, string Cliente, decimal TotalHoras);

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
