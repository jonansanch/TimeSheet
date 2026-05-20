using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDistribucionHoras;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Dashboard;

public class GetDistribucionHorasQueryHandler(IDbConnection db)
    : IRequestHandler<GetDistribucionHorasQuery, DistribucionHorasResponse>
{
    private const string Sql = """
        SELECT u.Id                                                        AS UserId,
               ISNULL(u.NombreCompleto, u.Email)                          AS Nombre,
               ROUND(
                   SUM(DATEDIFF(MINUTE, r.HoraEntrada, r.HoraSalida))
                   / 60.0, 1)                                             AS TotalHoras
        FROM   RegistrosHoras r
        JOIN   AspNetUsers u ON r.UserId = u.Id
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
        GROUP  BY u.Id, u.NombreCompleto, u.Email
        ORDER  BY TotalHoras DESC
        """;

    public async Task<DistribucionHorasResponse> Handle(
        GetDistribucionHorasQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await db.QueryAsync<DistribucionConsultorDto>(
            Sql, new { Desde = request.Desde, Hasta = request.Hasta });

        var consultores = rows.ToList();

        return new DistribucionHorasResponse(
            Desde:            request.Desde,
            Hasta:            request.Hasta,
            TotalHorasEquipo: consultores.Sum(c => c.TotalHoras),
            Consultores:      consultores);
    }
}
