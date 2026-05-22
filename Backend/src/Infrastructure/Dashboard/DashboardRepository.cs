using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDashboardGerencial;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDistribucionHoras;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetEstadoEquipo;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetMetricasGlobales;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetPendientesCriticos;
using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Infrastructure.Dashboard;

public class DashboardRepository(IDbConnection db) : IDashboardRepository
{
    private const string SqlGerencial = """
        SELECT r.Cliente,
               ROUND((
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntradaAM, r.HoraSalidaAM), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntradaPM, r.HoraSalidaPM), 0)
               ) / 60.0, 1) AS TotalHoras
        FROM   RegistrosHoras r
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  r.Cliente <> ''
        GROUP  BY r.Cliente
        ORDER  BY TotalHoras DESC;

        SELECT r.Proyecto,
               r.Cliente,
               ROUND((
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntradaAM, r.HoraSalidaAM), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntradaPM, r.HoraSalidaPM), 0)
               ) / 60.0, 1) AS TotalHoras
        FROM   RegistrosHoras r
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  r.Proyecto <> ''
        GROUP  BY r.Proyecto, r.Cliente
        ORDER  BY TotalHoras DESC;
        """;

    private const string SqlDistribucion = """
        SELECT u.Id                                                        AS UserId,
               ISNULL(u.NombreCompleto, u.Email)                          AS Nombre,
               ROUND((
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntradaAM, r.HoraSalidaAM)), 0) +
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntradaPM, r.HoraSalidaPM)), 0)
               ) / 60.0, 1)                                               AS TotalHoras
        FROM   RegistrosHoras r
        JOIN   AspNetUsers u ON r.UserId = u.Id
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
        GROUP  BY u.Id, u.NombreCompleto, u.Email
        ORDER  BY TotalHoras DESC
        """;

    private const string SqlEstadoEquipo = """
        SELECT u.Id                               AS UserId,
               ISNULL(u.NombreCompleto, u.Email) AS Nombre,
               u.Email,
               CASE WHEN r.HoraEntradaAM IS NOT NULL THEN 1 ELSE 0 END AS TieneAM,
               CASE WHEN r.HoraEntradaPM IS NOT NULL THEN 1 ELSE 0 END AS TienePM
        FROM   AspNetUsers u
        JOIN   AspNetUserRoles ur ON u.Id = ur.UserId
        JOIN   AspNetRoles ro     ON ur.RoleId = ro.Id
        LEFT   JOIN RegistrosHoras r
               ON r.UserId = u.Id AND r.FechaRegistro = @Fecha
        WHERE  u.IsActive = 1
          AND  ro.Name IN ('Empleado', 'Supervisor')
        ORDER  BY ISNULL(u.NombreCompleto, u.Email)
        """;

    private const string SqlMetricas = """
        SELECT
            (SELECT COUNT(*)
             FROM   RegistrosHoras
             WHERE  FechaRegistro BETWEEN @Desde AND @Hasta)                       AS TotalRegistros,
            ISNULL(
              (SELECT ROUND((
                   ISNULL(SUM(DATEDIFF(MINUTE, HoraEntradaAM, HoraSalidaAM)), 0) +
                   ISNULL(SUM(DATEDIFF(MINUTE, HoraEntradaPM, HoraSalidaPM)), 0)
               ) / 60.0, 1)
               FROM   RegistrosHoras
               WHERE  FechaRegistro BETWEEN @Desde AND @Hasta), 0)                 AS TotalHoras,
            (SELECT COUNT(*)
             FROM   AspNetUsers
             WHERE  IsActive = 1)                                                   AS UsuariosActivos,
            (SELECT COUNT(DISTINCT r.Cliente)
             FROM   RegistrosHoras r
             WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
               AND  r.Cliente <> '')                                                AS ClientesActivos,
            (SELECT COUNT(*)
             FROM   AspNetUsers u
             JOIN   AspNetUserRoles ur ON u.Id = ur.UserId
             JOIN   AspNetRoles ro     ON ur.RoleId = ro.Id
             WHERE  u.IsActive = 1
               AND  ro.Name IN ('Empleado', 'Supervisor')
               AND  u.Id NOT IN (
                        SELECT UserId
                        FROM   RegistrosHoras
                        WHERE  FechaRegistro = @Hoy))                              AS PendientesHoy;

        SELECT r.FechaRegistro                                                      AS Fecha,
               COUNT(*)                                                             AS TotalRegistros,
               ROUND((
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntradaAM, r.HoraSalidaAM)), 0) +
                   ISNULL(SUM(DATEDIFF(MINUTE, r.HoraEntradaPM, r.HoraSalidaPM)), 0)
               ) / 60.0, 1)                                                         AS TotalHoras
        FROM   RegistrosHoras r
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
        GROUP  BY r.FechaRegistro
        ORDER  BY r.FechaRegistro;
        """;

    private const string SqlUmbral = """
        SELECT ISNULL(TRY_CAST(Valor AS int), 3)
        FROM   ParametrosSistema
        WHERE  Clave = @Clave
        """;

    private const string SqlPendientes = """
        SELECT u.Id                               AS UserId,
               ISNULL(u.NombreCompleto, u.Email)  AS Nombre,
               u.Email,
               MAX(r.FechaRegistro)               AS UltimoRegistro
        FROM   AspNetUsers u
        JOIN   AspNetUserRoles ur ON u.Id = ur.UserId
        JOIN   AspNetRoles ro     ON ur.RoleId = ro.Id
        LEFT   JOIN RegistrosHoras r ON r.UserId = u.Id
        WHERE  u.IsActive = 1
          AND  ro.Name IN ('Empleado', 'Supervisor')
        GROUP  BY u.Id, u.NombreCompleto, u.Email
        HAVING MAX(r.FechaRegistro) < @FechaCorte
            OR MAX(r.FechaRegistro) IS NULL
        ORDER  BY MAX(r.FechaRegistro) ASC
        """;

    public async Task<DashboardGerencialResponse> GetGerencialAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        using var multi = await db.QueryMultipleAsync(SqlGerencial, new { Desde = desde, Hasta = hasta });
        var porCliente  = (await multi.ReadAsync<HorasPorClienteDto>()).ToList();
        var porProyecto = (await multi.ReadAsync<HorasPorProyectoDto>()).ToList();

        return new DashboardGerencialResponse(
            Desde:         desde,
            Hasta:         hasta,
            TotalHoras:    porCliente.Sum(c => c.TotalHoras),
            TotalClientes: porCliente.Count,
            PorCliente:    porCliente,
            PorProyecto:   porProyecto);
    }

    public async Task<DistribucionHorasResponse> GetDistribucionHorasAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        var consultores = (await db.QueryAsync<DistribucionConsultorDto>(SqlDistribucion, new { Desde = desde, Hasta = hasta })).ToList();

        return new DistribucionHorasResponse(
            Desde:            desde,
            Hasta:            hasta,
            TotalHorasEquipo: consultores.Sum(c => c.TotalHoras),
            Consultores:      consultores);
    }

    public async Task<EstadoEquipoResponse> GetEstadoEquipoAsync(DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var rows = await db.QueryAsync<EstadoRawRow>(SqlEstadoEquipo, new { Fecha = fecha });

        var equipo = rows.Select(r =>
        {
            var tieneAm = r.TieneAM == 1;
            var tienePm = r.TienePM == 1;
            var estado = (tieneAm, tienePm) switch
            {
                (true, true)   => "Completo",
                (false, false) => "Pendiente",
                _              => "Parcial"
            };
            return new MiembroEstadoDto(r.UserId, r.Nombre, r.Email, tieneAm, tienePm, estado);
        }).ToList();

        return new EstadoEquipoResponse(
            Fecha:           fecha,
            TotalEmpleados:  equipo.Count,
            TotalCompletos:  equipo.Count(m => m.Estado == "Completo"),
            TotalParciales:  equipo.Count(m => m.Estado == "Parcial"),
            TotalPendientes: equipo.Count(m => m.Estado == "Pendiente"),
            Equipo:          equipo);
    }

    public async Task<MetricasGlobalesResponse> GetMetricasGlobalesAsync(DateOnly desde, DateOnly hasta, CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        using var multi = await db.QueryMultipleAsync(SqlMetricas, new { Desde = desde, Hasta = hasta, Hoy = hoy });

        var resumen   = await multi.ReadSingleAsync<MetricasResumenRow>();
        var tendencia = (await multi.ReadAsync<TendenciaDiaDto>()).ToList();

        return new MetricasGlobalesResponse(
            Desde:           desde,
            Hasta:           hasta,
            TotalRegistros:  resumen.TotalRegistros,
            TotalHoras:      resumen.TotalHoras,
            UsuariosActivos: resumen.UsuariosActivos,
            ClientesActivos: resumen.ClientesActivos,
            PendientesHoy:   resumen.PendientesHoy,
            Tendencia:       tendencia);
    }

    public async Task<PendientesCriticosResponse> GetPendientesCriticosAsync(CancellationToken cancellationToken = default)
    {
        var umbral = await db.ExecuteScalarAsync<int>(SqlUmbral,
            new { Clave = ParametrosSistema.DiasUmbralNotificacion });
        if (umbral <= 0) umbral = 3;

        var fechaCorte = BusinessDayCalculator.GetEarliestAllowedDate(DateOnly.FromDateTime(DateTime.Today), umbral);
        var rows = await db.QueryAsync<PendientesRawRow>(SqlPendientes, new { FechaCorte = fechaCorte });

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var pendientes = rows.Select(r => new PendienteCriticoDto(
            r.UserId, r.Nombre, r.Email,
            r.UltimoRegistro.HasValue ? BusinessDayCalculator.CountBusinessDays(r.UltimoRegistro.Value, hoy) : 999)).ToList();

        return new PendientesCriticosResponse(umbral, pendientes);
    }

    private sealed record EstadoRawRow(string UserId, string Nombre, string Email, int TieneAM, int TienePM);
    private sealed record MetricasResumenRow(int TotalRegistros, decimal TotalHoras, int UsuariosActivos, int ClientesActivos, int PendientesHoy);
    private sealed record PendientesRawRow(string UserId, string Nombre, string Email, DateOnly? UltimoRegistro);
}
