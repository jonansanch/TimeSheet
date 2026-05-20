using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDashboardGerencial;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Dashboard;

public class GetDashboardGerencialQueryHandler(IDbConnection db)
    : IRequestHandler<GetDashboardGerencialQuery, DashboardGerencialResponse>
{
    private const string Sql = """
        SELECT r.Cliente,
               ROUND(SUM(DATEDIFF(MINUTE, r.HoraEntrada, r.HoraSalida)) / 60.0, 1) AS TotalHoras
        FROM   RegistrosHoras r
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  r.Cliente <> ''
        GROUP  BY r.Cliente
        ORDER  BY TotalHoras DESC;

        SELECT r.Proyecto,
               r.Cliente,
               ROUND(SUM(DATEDIFF(MINUTE, r.HoraEntrada, r.HoraSalida)) / 60.0, 1) AS TotalHoras
        FROM   RegistrosHoras r
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  r.Proyecto <> ''
        GROUP  BY r.Proyecto, r.Cliente
        ORDER  BY TotalHoras DESC;
        """;

    public async Task<DashboardGerencialResponse> Handle(
        GetDashboardGerencialQuery request,
        CancellationToken cancellationToken)
    {
        using var multi = await db.QueryMultipleAsync(Sql, new { Desde = request.Desde, Hasta = request.Hasta });

        var porCliente  = (await multi.ReadAsync<HorasPorClienteDto>()).ToList();
        var porProyecto = (await multi.ReadAsync<HorasPorProyectoDto>()).ToList();

        return new DashboardGerencialResponse(
            Desde:         request.Desde,
            Hasta:         request.Hasta,
            TotalHoras:    porCliente.Sum(c => c.TotalHoras),
            TotalClientes: porCliente.Count,
            PorCliente:    porCliente,
            PorProyecto:   porProyecto);
    }
}
