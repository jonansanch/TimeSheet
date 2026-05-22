using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;

namespace KPG.Timesheet.Infrastructure.Reportes;

public class ReportesRepository(IDbConnection db) : IReportesRepository
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

    public async Task<ReporteHorasResponse> GetReporteHorasAsync(DateOnly desde, DateOnly hasta, string? userId, string? cliente, string? proyecto, CancellationToken cancellationToken = default)
    {
        var rows = (await db.QueryAsync<RawRow>(Sql, new
        {
            Desde    = desde,
            Hasta    = hasta,
            UserId   = string.IsNullOrWhiteSpace(userId)   ? null : userId,
            Cliente  = string.IsNullOrWhiteSpace(cliente)  ? null : cliente.Trim(),
            Proyecto = string.IsNullOrWhiteSpace(proyecto) ? null : proyecto.Trim()
        })).ToList();

        var items = rows.Select(r => new ReporteHorasItemDto(
            r.UserId,
            r.NombreEmpleado,
            r.Email,
            DateOnly.FromDateTime(r.FechaRegistro),
            r.Turno == "AM" ? 1 : 2,
            TimeOnly.FromTimeSpan(r.HoraEntrada),
            TimeOnly.FromTimeSpan(r.HoraSalida),
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
            TotalRegistros: items.Count,
            TotalHoras:     Math.Round(items.Sum(i => i.Horas), 1),
            Items:          items);
    }

    private sealed record RawRow(
        string UserId,
        string NombreEmpleado,
        string Email,
        DateTime FechaRegistro,
        string Turno,
        TimeSpan HoraEntrada,
        TimeSpan HoraSalida,
        decimal Horas,
        string Cliente,
        string Proyecto,
        string Modalidad,
        string Lugar,
        string Descripcion);
}
