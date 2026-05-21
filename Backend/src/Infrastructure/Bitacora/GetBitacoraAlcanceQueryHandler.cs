using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacoraAlcance;
using KPG.Timesheet.Domain.Constants;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Bitacora;

public class GetBitacoraAlcanceQueryHandler(IDbConnection db, IUser user)
    : IRequestHandler<GetBitacoraAlcanceQuery, BitacoraResponse>
{
    private const string SqlCompleto = """
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

    private const string SqlEquipo = """
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
        WHERE  b.ActorId IN (
                   SELECT ur.UserId
                   FROM   AspNetUserRoles ur
                   JOIN   AspNetRoles r ON r.Id = ur.RoleId
                   WHERE  r.Name IN ('Empleado', 'Supervisor')
               )
          AND  (@Desde      IS NULL OR CAST(b.Timestamp AS date) >= @Desde)
          AND  (@Hasta      IS NULL OR CAST(b.Timestamp AS date) <= @Hasta)
          AND  (@ActorId    IS NULL OR b.ActorId = @ActorId)
          AND  (@TipoEvento IS NULL OR b.TipoEvento = @TipoEvento)
        ORDER  BY b.Timestamp DESC
        OFFSET 0 ROWS FETCH NEXT 500 ROWS ONLY
        """;

    public async Task<BitacoraResponse> Handle(
        GetBitacoraAlcanceQuery request,
        CancellationToken cancellationToken)
    {
        bool isSupervisor = user.Roles?.Contains(Roles.Supervisor) == true
                            && user.Roles?.Contains(Roles.Gerente) != true;

        var sql = isSupervisor ? SqlEquipo : SqlCompleto;

        var rows = (await db.QueryAsync<BitacoraItemDto>(sql, new
        {
            Desde      = request.Desde,
            Hasta      = request.Hasta,
            ActorId    = request.ActorId,
            TipoEvento = request.TipoEvento
        })).ToList();

        return new BitacoraResponse(rows.Count, rows);
    }
}
