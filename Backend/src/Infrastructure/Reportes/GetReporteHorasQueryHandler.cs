using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Reportes;

public class GetReporteHorasQueryHandler(IDbConnection db)
    : IRequestHandler<GetReporteHorasQuery, ReporteHorasResponse>
{
    private const string Sql = """
        SELECT r.UserId,
               ISNULL(u.NombreCompleto, u.Email)                          AS NombreEmpleado,
               u.Email,
               r.FechaRegistro,
               r.Turno,
               r.HoraEntrada,
               r.HoraSalida,
               ROUND(DATEDIFF(MINUTE, r.HoraEntrada, r.HoraSalida) / 60.0, 2) AS Horas,
               r.Cliente,
               r.Proyecto,
               r.Modalidad,
               r.Lugar,
               r.Descripcion
        FROM   RegistrosHoras r
        JOIN   AspNetUsers u ON r.UserId = u.Id
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  (@UserId  IS NULL OR r.UserId  = @UserId)
          AND  (@Cliente IS NULL OR r.Cliente LIKE '%' + @Cliente + '%')
          AND  (@Proyecto IS NULL OR r.Proyecto LIKE '%' + @Proyecto + '%')
        ORDER  BY r.FechaRegistro DESC, NombreEmpleado, r.Turno
        OFFSET 0 ROWS FETCH NEXT 1000 ROWS ONLY
        """;

    public async Task<ReporteHorasResponse> Handle(
        GetReporteHorasQuery request,
        CancellationToken cancellationToken)
    {
        var items = (await db.QueryAsync<ReporteHorasItemDto>(Sql, new
        {
            Desde    = request.Desde,
            Hasta    = request.Hasta,
            UserId   = string.IsNullOrWhiteSpace(request.UserId)   ? null : request.UserId,
            Cliente  = string.IsNullOrWhiteSpace(request.Cliente)  ? null : request.Cliente.Trim(),
            Proyecto = string.IsNullOrWhiteSpace(request.Proyecto) ? null : request.Proyecto.Trim()
        })).ToList();

        return new ReporteHorasResponse(
            Desde:          request.Desde,
            Hasta:          request.Hasta,
            TotalRegistros: items.Count,
            TotalHoras:     Math.Round(items.Sum(i => i.Horas), 1),
            Items:          items);
    }
}
