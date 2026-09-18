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
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntrada1, r.HoraSalida1)), 0) +
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntrada2, r.HoraSalida2)), 0) +
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntrada3, r.HoraSalida3)), 0)
               ) / 60.0, 1), 0) AS TotalHoras
        {SqlBase};
        """;

    private const string SqlItems = """
        SELECT r.UserId,
               ISNULL(u.NombreCompleto, u.Email)                          AS NombreEmpleado,
               u.Email,
               r.FechaRegistro,
               r.HoraEntrada1,
               r.HoraSalida1,
               r.HoraEntrada2,
               r.HoraSalida2,
               r.HoraEntrada3,
               r.HoraSalida3,
               ROUND((
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada1, r.HoraSalida1), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada2, r.HoraSalida2), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada3, r.HoraSalida3), 0)
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
            r.HoraEntrada1.HasValue ? TimeOnly.FromTimeSpan(r.HoraEntrada1.Value) : null,
            r.HoraSalida1.HasValue  ? TimeOnly.FromTimeSpan(r.HoraSalida1.Value)  : null,
            r.HoraEntrada2.HasValue ? TimeOnly.FromTimeSpan(r.HoraEntrada2.Value) : null,
            r.HoraSalida2.HasValue  ? TimeOnly.FromTimeSpan(r.HoraSalida2.Value)  : null,
            r.HoraEntrada3.HasValue ? TimeOnly.FromTimeSpan(r.HoraEntrada3.Value) : null,
            r.HoraSalida3.HasValue  ? TimeOnly.FromTimeSpan(r.HoraSalida3.Value)  : null,
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
        TimeSpan? HoraEntrada1,
        TimeSpan? HoraSalida1,
        TimeSpan? HoraEntrada2,
        TimeSpan? HoraSalida2,
        TimeSpan? HoraEntrada3,
        TimeSpan? HoraSalida3,
        decimal   Horas,
        string    Cliente,
        string    Proyecto,
        string    Modalidad,
        string    Lugar,
        string    Descripcion);
}
