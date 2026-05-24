using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Bitacora.Queries.ExportarBitacora;
using MediatR;
using MiniExcelLibs;

namespace KPG.Timesheet.Infrastructure.Bitacora;

public class ExportarBitacoraQueryHandler(IDbConnection db)
    : IRequestHandler<ExportarBitacoraQuery, ExportarBitacoraResult>
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
        WHERE  (@DesdeUtc           IS NULL OR b.Timestamp >= @DesdeUtc)
          AND  (@HastaExclusivoUtc  IS NULL OR b.Timestamp < @HastaExclusivoUtc)
          AND  (@ActorId    IS NULL OR b.ActorId = @ActorId)
          AND  (@TipoEvento IS NULL OR b.TipoEvento = @TipoEvento)
        ORDER  BY b.Timestamp DESC
        OFFSET 0 ROWS FETCH NEXT 500 ROWS ONLY
        """;

    public async Task<ExportarBitacoraResult> Handle(
        ExportarBitacoraQuery request,
        CancellationToken cancellationToken)
    {
        var (desdeUtc, hastaExclusivoUtc) = BuildUtcRange(request.Desde, request.Hasta);
        var rows = (await db.QueryAsync<RawRow>(Sql, new
        {
            DesdeUtc          = desdeUtc,
            HastaExclusivoUtc = hastaExclusivoUtc,
            ActorId           = request.ActorId,
            TipoEvento        = request.TipoEvento
        })).ToList();

        var exportRows = rows.Select(r => new
        {
            FechaHora       = r.Timestamp.LocalDateTime.ToString("dd/MM/yyyy HH:mm"),
            TipoEvento      = r.TipoEvento,
            Actor           = r.ActorNombre ?? r.ActorEmail ?? r.ActorId,
            EntidadAfectada = r.EntidadAfectada,
            EntidadId       = r.EntidadId ?? string.Empty,
            Metadata        = r.MetadataJson ?? string.Empty
        }).ToList();

        using var ms = new MemoryStream();
        ms.SaveAs(exportRows);

        var hoy   = DateOnly.FromDateTime(DateTime.Today);
        var desde = request.Desde ?? hoy;
        var hasta = request.Hasta ?? hoy;
        var fileName = $"bitacora-{desde:yyyyMMdd}-{hasta:yyyyMMdd}.xlsx";

        return new ExportarBitacoraResult(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
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

    private sealed record RawRow(
        int Id,
        string TipoEvento,
        string ActorId,
        string? ActorEmail,
        string? ActorNombre,
        string EntidadAfectada,
        string? EntidadId,
        DateTimeOffset Timestamp,
        string? MetadataJson);
}
