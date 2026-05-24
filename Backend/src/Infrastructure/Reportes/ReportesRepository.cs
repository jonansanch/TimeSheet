using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

namespace KPG.Timesheet.Infrastructure.Reportes;

public class ReportesRepository(IDbConnection db) : IReportesRepository
{
    private const string SqlBase = """
        FROM   RegistrosHoras r
        JOIN   AspNetUsers u ON r.UserId = u.Id
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  (@UserId  IS NULL OR r.UserId  = @UserId)
          AND  (@ClientePattern IS NULL OR r.Cliente LIKE @ClientePattern ESCAPE '\')
          AND  (@ProyectoPattern IS NULL OR r.Proyecto LIKE @ProyectoPattern ESCAPE '\')
        """;

    private const string SqlResumen = $"""
        SELECT COUNT(*) AS TotalRegistros,
               ISNULL(ROUND((
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntradaAM, r.HoraSalidaAM)), 0) +
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntradaPM, r.HoraSalidaPM)), 0)
               ) / 60.0, 1), 0) AS TotalHoras
        {SqlBase};
        """;

    private const string SqlItems = """
        SELECT r.UserId,
               ISNULL(u.NombreCompleto, u.Email)                          AS NombreEmpleado,
               u.Email,
               r.FechaRegistro,
               r.HoraEntradaAM,
               r.HoraSalidaAM,
               r.HoraEntradaPM,
               r.HoraSalidaPM,
               ROUND((
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntradaAM, r.HoraSalidaAM), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntradaPM, r.HoraSalidaPM), 0)
               ) / 60.0, 2) AS Horas,
               r.Cliente,
               r.Proyecto,
               r.Modalidad,
               r.Lugar,
               r.Descripcion
        """;

    public async Task<ReporteHorasResponse> GetReporteHorasAsync(
        DateOnly desde,
        DateOnly hasta,
        string? userId,
        string? cliente,
        string? proyecto,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var offset = (pageNumber - 1) * pageSize;
        var orderBy = BuildOrderBy(sortBy, sortDescending);
        var sql = $"""
            {SqlResumen}

            {SqlItems}
            {SqlBase}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await db.QueryMultipleAsync(new CommandDefinition(sql, new
        {
            Desde    = desde,
            Hasta    = hasta,
            UserId   = string.IsNullOrWhiteSpace(userId)   ? null : userId,
            ClientePattern  = BuildPrefixLikePattern(cliente),
            ProyectoPattern = BuildPrefixLikePattern(proyecto),
            Offset   = offset,
            PageSize = pageSize
        }, cancellationToken: cancellationToken));

        var resumen = await multi.ReadSingleAsync<ResumenRow>();
        var rows = (await multi.ReadAsync<RawRow>()).ToList();

        var items = rows.Select(r => new ReporteHorasItemDto(
            r.UserId,
            r.NombreEmpleado,
            r.Email,
            DateOnly.FromDateTime(r.FechaRegistro),
            r.HoraEntradaAM.HasValue ? TimeOnly.FromTimeSpan(r.HoraEntradaAM.Value) : null,
            r.HoraSalidaAM.HasValue  ? TimeOnly.FromTimeSpan(r.HoraSalidaAM.Value)  : null,
            r.HoraEntradaPM.HasValue ? TimeOnly.FromTimeSpan(r.HoraEntradaPM.Value) : null,
            r.HoraSalidaPM.HasValue  ? TimeOnly.FromTimeSpan(r.HoraSalidaPM.Value)  : null,
            r.Horas,
            r.Cliente,
            r.Proyecto,
            r.Modalidad,
            r.Lugar,
            r.Descripcion
        )).ToList();

        return new ReporteHorasResponse(
            Desde:          desde,
            Hasta:          hasta,
            PageNumber:     pageNumber,
            PageSize:       pageSize,
            TotalRegistros: resumen.TotalRegistros,
            TotalHoras:     resumen.TotalHoras,
            Items:          items);
    }

    private static string BuildOrderBy(string? sortBy, bool descending)
    {
        var direction = descending ? "DESC" : "ASC";
        var column = sortBy?.Trim().ToLowerInvariant() switch
        {
            "fecharegistro"  => "r.FechaRegistro",
            "nombreempleado" => "NombreEmpleado",
            "email"          => "u.Email",
            "horas"          => "Horas",
            "cliente"        => "r.Cliente",
            "proyecto"       => "r.Proyecto",
            "modalidad"      => "r.Modalidad",
            "lugar"          => "r.Lugar",
            _                => "r.FechaRegistro"
        };

        return $"{column} {direction}, NombreEmpleado ASC, r.Id ASC";
    }

    private static string? BuildPrefixLikePattern(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return EscapeLikePattern(value.Trim()) + "%";
    }

    private static string EscapeLikePattern(string value)
        => value
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_")
            .Replace("[", @"\[");

    private sealed record ResumenRow(int TotalRegistros, decimal TotalHoras);

    private sealed record RawRow(
        string    UserId,
        string    NombreEmpleado,
        string    Email,
        DateTime  FechaRegistro,
        TimeSpan? HoraEntradaAM,
        TimeSpan? HoraSalidaAM,
        TimeSpan? HoraEntradaPM,
        TimeSpan? HoraSalidaPM,
        decimal   Horas,
        string    Cliente,
        string    Proyecto,
        string    Modalidad,
        string    Lugar,
        string    Descripcion);
}
