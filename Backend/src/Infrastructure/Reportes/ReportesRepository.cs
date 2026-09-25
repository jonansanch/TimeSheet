using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

namespace KPG.Timesheet.Infrastructure.Reportes;

public class ReportesRepository(IDbConnection db, IValidadorDescripcion validadorDescripcion) : IReportesRepository
{
    /// <summary>
    /// Tope de filas exploradas cuando <c>soloConObservaciones=true</c>: hay que traer y
    /// evaluar TODO lo que cae dentro de los filtros (no solo la pagina) para poder contar
    /// y paginar sobre el conjunto ya filtrado. Sin tope, un rango de fechas muy amplio
    /// evaluaria miles de filas en una sola llamada.
    /// </summary>
    private const int TopeFilasParaFiltroObservaciones = 3000;

    private const string SqlBase = """
        FROM   RegistrosHoras r
        JOIN   AspNetUsers u ON r.UserId = u.Id
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  (@UserId  IS NULL OR r.UserId  = @UserId)
          AND  (@ClientePattern IS NULL OR r.ClienteNombre LIKE @ClientePattern ESCAPE '\')
          AND  (@ProyectoPattern IS NULL OR r.ProyectoNombre LIKE @ProyectoPattern ESCAPE '\')
          AND  (@RecursoPattern IS NULL OR r.Recurso LIKE @RecursoPattern ESCAPE '\')
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
               r.ClienteNombre  AS Cliente,
               r.ProyectoNombre AS Proyecto,
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
        string? recurso,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        bool soloConObservaciones = false,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var orderBy = BuildOrderBy(sortBy, sortDescending);
        var parametrosSql = new
        {
            Desde    = desde,
            Hasta    = hasta,
            UserId   = string.IsNullOrWhiteSpace(userId)   ? null : userId,
            ClientePattern  = BuildPrefixLikePattern(cliente),
            ProyectoPattern = BuildPrefixLikePattern(proyecto),
            RecursoPattern  = BuildPrefixLikePattern(recurso)
        };

        return soloConObservaciones
            ? await GetReporteFiltradoPorObservacionesAsync(
                desde, hasta, pageNumber, pageSize, orderBy, parametrosSql, cancellationToken)
            : await GetReportePaginadoAsync(
                desde, hasta, pageNumber, pageSize, orderBy, parametrosSql, cancellationToken);
    }

    /// <summary>Camino normal: paginacion en SQL, igual que antes de la calidad de descripciones.</summary>
    private async Task<ReporteHorasResponse> GetReportePaginadoAsync(
        DateOnly desde, DateOnly hasta, int pageNumber, int pageSize, string orderBy,
        object parametrosSql, CancellationToken cancellationToken)
    {
        var offset = (pageNumber - 1) * pageSize;
        var sql = $"""
            {SqlResumen}

            {SqlItems}
            {SqlBase}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await db.QueryMultipleAsync(new CommandDefinition(
            sql, ConParametros(parametrosSql, new { Offset = offset, PageSize = pageSize }),
            cancellationToken: cancellationToken));

        var resumen = await multi.ReadSingleAsync<ResumenRow>();
        var rows = (await multi.ReadAsync<RawRow>()).ToList();

        // Se evalua solo la pagina que se muestra: es lo unico que hace falta pintar, y
        // evita evaluar el reporte entero en el camino normal (sin el filtro activo).
        var conObservaciones = await EvaluarObservacionesAsync(rows, cancellationToken);

        return new ReporteHorasResponse(
            Desde:          desde,
            Hasta:          hasta,
            PageNumber:     pageNumber,
            PageSize:       pageSize,
            TotalRegistros: resumen.TotalRegistros,
            TotalHoras:     resumen.TotalHoras,
            Items:          MapearItems(rows, conObservaciones));
    }

    /// <summary>
    /// "Solo con observaciones": no se puede filtrar en SQL (la calidad depende del
    /// catalogo, no de una columna), asi que se trae todo lo que entra en el filtro (hasta
    /// <see cref="TopeFilasParaFiltroObservaciones"/>), se evalua una vez y se pagina en
    /// memoria sobre el subconjunto ya filtrado.
    /// </summary>
    private async Task<ReporteHorasResponse> GetReporteFiltradoPorObservacionesAsync(
        DateOnly desde, DateOnly hasta, int pageNumber, int pageSize, string orderBy,
        object parametrosSql, CancellationToken cancellationToken)
    {
        var sql = $"""
            {SqlItems}
            {SqlBase}
            ORDER BY {orderBy}
            OFFSET 0 ROWS FETCH NEXT {TopeFilasParaFiltroObservaciones} ROWS ONLY;
            """;

        var rows = (await db.QueryAsync<RawRow>(new CommandDefinition(
            sql, parametrosSql, cancellationToken: cancellationToken))).ToList();

        var conObservaciones = await EvaluarObservacionesAsync(rows, cancellationToken);

        var filasConObservaciones = rows
            .Zip(conObservaciones, (fila, tiene) => (Fila: fila, TieneObservaciones: tiene))
            .Where(x => x.TieneObservaciones)
            .Select(x => x.Fila)
            .ToList();

        var pagina = filasConObservaciones
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new ReporteHorasResponse(
            Desde:          desde,
            Hasta:          hasta,
            PageNumber:     pageNumber,
            PageSize:       pageSize,
            TotalRegistros: filasConObservaciones.Count,
            TotalHoras:     Math.Round(filasConObservaciones.Sum(f => f.Horas), 2),
            // Todas las filas de esta pagina ya pasaron el filtro: el flag es true en todas.
            Items:          MapearItems(pagina, pagina.Select(_ => true).ToList()));
    }

    private async Task<List<bool>> EvaluarObservacionesAsync(List<RawRow> rows, CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
            return [];

        var evaluaciones = await validadorDescripcion.EvaluarVariasPorNombreProyectoAsync(
            rows.Select(r => ((string?)r.Descripcion, (string?)r.Proyecto)).ToList(), cancellationToken);

        return evaluaciones.Select(e => e.Hallazgos.Count > 0).ToList();
    }

    private static List<ReporteHorasItemDto> MapearItems(List<RawRow> rows, IReadOnlyList<bool> conObservaciones) =>
        rows.Select((r, i) => new ReporteHorasItemDto(
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
            r.Descripcion,
            conObservaciones[i]
        )).ToList();

    /// <summary>Combina los parametros base del filtro con los propios de cada camino (paginacion).</summary>
    private static DynamicParameters ConParametros(object baseParams, object extra)
    {
        var parametros = new DynamicParameters(baseParams);
        parametros.AddDynamicParams(extra);
        return parametros;
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
            "cliente"        => "r.ClienteNombre",
            "proyecto"       => "r.ProyectoNombre",
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
