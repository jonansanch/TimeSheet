using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Notificaciones.Queries.GetHistorialNotificaciones;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Notificaciones;

public class GetHistorialNotificacionesQueryHandler(IDbConnection db)
    : IRequestHandler<GetHistorialNotificacionesQuery, HistorialNotificacionesResponse>
{
    private const string Sql = """
        SELECT n.Id,
               n.UserId,
               ISNULL(u.NombreCompleto, u.Email) AS Nombre,
               n.Email,
               n.FechaReferencia,
               n.DiasAcumulados,
               n.Exitoso,
               n.ErrorDetalle,
               n.Created                          AS FechaEnvio
        FROM   NotificacionesEnviadas n
        LEFT   JOIN AspNetUsers u ON u.Id = n.UserId
        WHERE  (@Desde   IS NULL OR CAST(n.Created AS date) >= @Desde)
          AND  (@Hasta   IS NULL OR CAST(n.Created AS date) <= @Hasta)
          AND  (@UserId  IS NULL OR n.UserId = @UserId)
          AND  (@Exitoso IS NULL OR n.Exitoso = @Exitoso)
        ORDER  BY n.Created DESC
        OFFSET 0 ROWS FETCH NEXT 500 ROWS ONLY
        """;

    public async Task<HistorialNotificacionesResponse> Handle(
        GetHistorialNotificacionesQuery request,
        CancellationToken cancellationToken)
    {
        bool? exitoso = request.SoloErrores == true ? false : null;

        var rows = (await db.QueryAsync<NotificacionItemDto>(Sql, new
        {
            Desde   = request.Desde,
            Hasta   = request.Hasta,
            UserId  = request.UserId,
            Exitoso = exitoso
        })).ToList();

        return new HistorialNotificacionesResponse(rows.Count, rows);
    }
}
