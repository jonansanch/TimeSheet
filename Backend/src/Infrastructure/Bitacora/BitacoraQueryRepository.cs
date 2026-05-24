using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;

namespace KPG.Timesheet.Infrastructure.Bitacora;

public class BitacoraQueryRepository(IDbConnection db) : IBitacoraQueryRepository
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
        WHERE  (@DesdeUtc           IS NULL OR b.Timestamp >= @DesdeUtc)
          AND  (@HastaExclusivoUtc  IS NULL OR b.Timestamp < @HastaExclusivoUtc)
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
          AND  (@DesdeUtc           IS NULL OR b.Timestamp >= @DesdeUtc)
          AND  (@HastaExclusivoUtc  IS NULL OR b.Timestamp < @HastaExclusivoUtc)
          AND  (@ActorId    IS NULL OR b.ActorId = @ActorId)
          AND  (@TipoEvento IS NULL OR b.TipoEvento = @TipoEvento)
        ORDER  BY b.Timestamp DESC
        OFFSET 0 ROWS FETCH NEXT 500 ROWS ONLY
        """;

    public async Task<BitacoraResponse> GetAsync(DateOnly? desde, DateOnly? hasta, string? actorId, string? tipoEvento, CancellationToken cancellationToken = default)
    {
        var (desdeUtc, hastaExclusivoUtc) = BuildUtcRange(desde, hasta);
        var rows = (await db.QueryAsync<BitacoraItemDto>(SqlCompleto, new { DesdeUtc = desdeUtc, HastaExclusivoUtc = hastaExclusivoUtc, ActorId = actorId, TipoEvento = tipoEvento })).ToList();
        return new BitacoraResponse(rows.Count, rows);
    }

    public async Task<BitacoraResponse> GetAlcanceAsync(DateOnly? desde, DateOnly? hasta, string? actorId, string? tipoEvento, bool soloEquipo, CancellationToken cancellationToken = default)
    {
        var sql = soloEquipo ? SqlEquipo : SqlCompleto;
        var (desdeUtc, hastaExclusivoUtc) = BuildUtcRange(desde, hasta);
        var rows = (await db.QueryAsync<BitacoraItemDto>(sql, new { DesdeUtc = desdeUtc, HastaExclusivoUtc = hastaExclusivoUtc, ActorId = actorId, TipoEvento = tipoEvento })).ToList();
        return new BitacoraResponse(rows.Count, rows);
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
