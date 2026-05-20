namespace KPG.Timesheet.Application.Features.Dashboard.Queries.GetDashboardGerencial;

public record DashboardGerencialResponse(
    DateOnly Desde,
    DateOnly Hasta,
    decimal TotalHoras,
    int TotalClientes,
    List<HorasPorClienteDto> PorCliente,
    List<HorasPorProyectoDto> PorProyecto);

public record HorasPorClienteDto(string Cliente, decimal TotalHoras);

public record HorasPorProyectoDto(string Proyecto, string Cliente, decimal TotalHoras);
