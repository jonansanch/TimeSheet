using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetMetricasGlobales;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Dashboard;

public class GetMetricasGlobalesQueryHandler(IDbConnection db)
    : IRequestHandler<GetMetricasGlobalesQuery, MetricasGlobalesResponse>
{
    private const string Sql = """
        SELECT
            (SELECT COUNT(*)
             FROM   RegistrosHoras
             WHERE  FechaRegistro BETWEEN @Desde AND @Hasta)                       AS TotalRegistros,
            ISNULL(
              (SELECT ROUND(SUM(DATEDIFF(MINUTE, HoraEntrada, HoraSalida)) / 60.0, 1)
               FROM   RegistrosHoras
               WHERE  FechaRegistro BETWEEN @Desde AND @Hasta), 0)                 AS TotalHoras,
            (SELECT COUNT(*)
             FROM   AspNetUsers
             WHERE  IsActive = 1)                                                   AS UsuariosActivos,
            (SELECT COUNT(DISTINCT r.Cliente)
             FROM   RegistrosHoras r
             WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
               AND  r.Cliente <> '')                                                AS ClientesActivos,
            (SELECT COUNT(*)
             FROM   AspNetUsers u
             JOIN   AspNetUserRoles ur ON u.Id = ur.UserId
             JOIN   AspNetRoles ro     ON ur.RoleId = ro.Id
             WHERE  u.IsActive = 1
               AND  ro.Name IN ('Empleado', 'Supervisor')
               AND  u.Id NOT IN (
                        SELECT UserId
                        FROM   RegistrosHoras
                        WHERE  FechaRegistro = @Hoy))                              AS PendientesHoy;

        SELECT r.FechaRegistro                                                      AS Fecha,
               COUNT(*)                                                             AS TotalRegistros,
               ROUND(SUM(DATEDIFF(MINUTE, r.HoraEntrada, r.HoraSalida)) / 60.0, 1) AS TotalHoras
        FROM   RegistrosHoras r
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
        GROUP  BY r.FechaRegistro
        ORDER  BY r.FechaRegistro;
        """;

    public async Task<MetricasGlobalesResponse> Handle(
        GetMetricasGlobalesQuery request,
        CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        using var multi = await db.QueryMultipleAsync(
            Sql, new { Desde = request.Desde, Hasta = request.Hasta, Hoy = hoy });

        var resumen   = await multi.ReadSingleAsync<ResumenRow>();
        var tendencia = (await multi.ReadAsync<TendenciaDiaDto>()).ToList();

        return new MetricasGlobalesResponse(
            Desde:           request.Desde,
            Hasta:           request.Hasta,
            TotalRegistros:  resumen.TotalRegistros,
            TotalHoras:      resumen.TotalHoras,
            UsuariosActivos: resumen.UsuariosActivos,
            ClientesActivos: resumen.ClientesActivos,
            PendientesHoy:   resumen.PendientesHoy,
            Tendencia:       tendencia);
    }

    private sealed record ResumenRow(
        int TotalRegistros,
        decimal TotalHoras,
        int UsuariosActivos,
        int ClientesActivos,
        int PendientesHoy);
}
