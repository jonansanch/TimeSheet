using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KPG.Timesheet.Infrastructure.Jobs;

public class NotificacionesPendientesJob(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificacionesPendientesJob> logger) : BackgroundService
{
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
        """;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(8);
            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            logger.LogInformation("NotificacionesPendientesJob: próxima ejecución en {Delay:hh\\:mm\\:ss}.", delay);

            try { await Task.Delay(delay, stoppingToken); }
            catch (OperationCanceledException) { break; }

            if (!stoppingToken.IsCancellationRequested)
                await EjecutarAsync(stoppingToken);
        }
    }

    private async Task EjecutarAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NotificacionesPendientesJob: iniciando proceso de notificaciones.");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var bitacora = scope.ServiceProvider.GetRequiredService<IBitacoraService>();

            var umbral = await db.ExecuteScalarAsync<int>(SqlUmbral, new { Clave = ParametrosSistema.DiasUmbralNotificacion });
            if (umbral <= 0) umbral = 3;

            var fechaCorte = BusinessDayCalculator.GetEarliestAllowedDate(DateOnly.FromDateTime(DateTime.Today), umbral);
            var pendientes = (await db.QueryAsync<PendienteRow>(SqlPendientes, new { FechaCorte = fechaCorte })).ToList();

            logger.LogInformation("NotificacionesPendientesJob: {Count} usuarios pendientes con umbral {Umbral} días.", pendientes.Count, umbral);

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var ventana = DateTime.UtcNow.AddHours(-24);

            foreach (var p in pendientes)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Verificar si ya se envió en las últimas 24h
                var yaEnviado = await context.NotificacionesEnviadas
                    .AnyAsync(n => n.UserId == p.UserId && n.Created >= ventana, cancellationToken);

                if (yaEnviado)
                {
                    logger.LogDebug("NotificacionesPendientesJob: omitiendo {Email} (notificado en las últimas 24h).", p.Email);
                    continue;
                }

                var diasSinRegistro = p.UltimoRegistro.HasValue
                    ? BusinessDayCalculator.CountBusinessDays(p.UltimoRegistro.Value, hoy)
                    : 999;

                var subject = "Recordatorio: horas pendientes de registro en KPG Timesheet";
                var body = $"""
                    Estimado/a {p.Nombre},

                    Notamos que llevas {(diasSinRegistro >= 999 ? "varios" : diasSinRegistro.ToString())} días sin registrar tus horas en KPG Timesheet.

                    Por favor ingresa al sistema y registra tus horas pendientes para evitar inconvenientes en el cierre mensual.

                    KPG Timesheet — Sistema de Gestión de Horas
                    """;

                bool exitoso = false;
                string? errorDetalle = null;

                try
                {
                    exitoso = await emailService.SendAsync(p.Email, subject, body, cancellationToken);
                }
                catch (Exception ex)
                {
                    errorDetalle = ex.Message;
                    logger.LogError(ex, "NotificacionesPendientesJob: fallo al enviar a {Email}.", p.Email);
                }

                var notificacion = new NotificacionEnviada
                {
                    UserId = p.UserId,
                    Email = p.Email,
                    FechaReferencia = p.UltimoRegistro ?? hoy,
                    DiasAcumulados = diasSinRegistro,
                    Exitoso = exitoso,
                    ErrorDetalle = errorDetalle
                };

                context.NotificacionesEnviadas.Add(notificacion);
                await context.SaveChangesAsync(cancellationToken);

                if (exitoso)
                {
                    await bitacora.RegistrarAsync(
                        TipoEventoBitacora.NotificacionEnviada,
                        "system", null,
                        "NotificacionesEnviadas", notificacion.Id.ToString(),
                        new { p.Email, notificacion.DiasAcumulados },
                        cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            logger.LogInformation("NotificacionesPendientesJob: proceso completado.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NotificacionesPendientesJob: error inesperado.");
        }
    }

    private sealed record PendienteRow(string UserId, string Nombre, string Email, DateOnly? UltimoRegistro);
}
