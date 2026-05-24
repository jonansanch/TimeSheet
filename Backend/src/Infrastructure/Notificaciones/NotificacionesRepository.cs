using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Notificaciones.Queries.GetHistorialNotificaciones;

namespace KPG.Timesheet.Infrastructure.Notificaciones;

public class NotificacionesRepository(IDbConnection db) : INotificacionesRepository
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
        WHERE  (@DesdeUtc           IS NULL OR n.Created >= @DesdeUtc)
          AND  (@HastaExclusivoUtc  IS NULL OR n.Created < @HastaExclusivoUtc)
          AND  (@UserId  IS NULL OR n.UserId = @UserId)
          AND  (@Exitoso IS NULL OR n.Exitoso = @Exitoso)
        ORDER  BY n.Created DESC
        OFFSET 0 ROWS FETCH NEXT 500 ROWS ONLY
        """;

    public async Task<HistorialNotificacionesResponse> GetHistorialAsync(DateOnly? desde, DateOnly? hasta, string? userId, bool? soloErrores, CancellationToken cancellationToken = default)
    {
        bool? exitoso = soloErrores == true ? false : null;
        var (desdeUtc, hastaExclusivoUtc) = BuildUtcRange(desde, hasta);

        var rows = (await db.QueryAsync<NotificacionItemDto>(Sql, new
        {
            DesdeUtc          = desdeUtc,
            HastaExclusivoUtc = hastaExclusivoUtc,
            UserId            = userId,
            Exitoso           = exitoso
        })).ToList();

        return new HistorialNotificacionesResponse(rows.Count, rows);
    }

    private static (DateTimeOffset? DesdeUtc, DateTimeOffset? HastaExclusivoUtc) BuildUtcRange(DateOnly? desde, DateOnly? hasta)
    {
        var desdeUtc = desde.HasValue
            ? new DateTimeOffset(desde.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : (DateTimeOffset?)null;

        var hastaExclusivoUtc = hasta.HasValue
            ? new DateTimeOffset(hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : (DateTimeOffset?)null;

        return (desdeUtc, hastaExclusivoUtc);
    }
}
