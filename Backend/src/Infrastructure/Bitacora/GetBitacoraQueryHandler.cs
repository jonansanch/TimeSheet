using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Bitacora;

public class GetBitacoraQueryHandler(IDbConnection db)
    : IRequestHandler<GetBitacoraQuery, BitacoraResponse>
{
    private const string Sql = """
        SELECT b.Id,
               b.TipoEvento,
               b.ActorId,
               b.ActorEmail,
               ISNULL(u.NombreCompleto, b.ActorEmail) AS ActorNombre,
               b.EntidadAfectada,
               b.EntidadId,
               b.Timestamp,
               b.MetadataJson
        FROM   BitacoraAuditoria b
        LEFT   JOIN AspNetUsers u ON u.Id = b.ActorId
        WHERE  (@Desde      IS NULL OR CAST(b.Timestamp AS date) >= @Desde)
          AND  (@Hasta      IS NULL OR CAST(b.Timestamp AS date) <= @Hasta)
          AND  (@ActorId    IS NULL OR b.ActorId = @ActorId)
          AND  (@TipoEvento IS NULL OR b.TipoEvento = @TipoEvento)
        ORDER  BY b.Timestamp DESC
        OFFSET 0 ROWS FETCH NEXT 500 ROWS ONLY
        """;

    public async Task<BitacoraResponse> Handle(
        GetBitacoraQuery request,
        CancellationToken cancellationToken)
    {
        var rows = (await db.QueryAsync<BitacoraItemDto>(Sql, new
        {
            Desde      = request.Desde,
            Hasta      = request.Hasta,
            ActorId    = request.ActorId,
            TipoEvento = request.TipoEvento
        })).ToList();

        return new BitacoraResponse(rows.Count, rows);
    }
}
