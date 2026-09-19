using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Aprobaciones;
using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Infrastructure.Organizacion;

public class AprobacionesRepository(IDbConnection db) : IAprobacionesRepository
{
    /// <summary>
    /// Resuelve los tres aprobadores <b>dentro</b> de la consulta y devuelve solo los
    /// registros cuyo nivel pendiente le toca al revisor. Hacerlo fila por fila desde C#
    /// significaria una consulta por registro.
    ///
    /// <para>
    /// La regla de la primera aprobacion prefiere la especifica del cliente sobre la
    /// general (ClienteId NULL); por eso el OUTER APPLY con ORDER BY sobre ClienteId.
    /// </para>
    /// </summary>
    private const string Sql = """
        WITH Base AS (
            SELECT r.Id,
                   r.UserId,
                   r.FechaRegistro,
                   r.ProyectoId,
                   r.ClienteNombre,
                   r.ProyectoNombre,
                   r.Recurso,
                   r.Modalidad,
                   r.Descripcion,
                   r.Estado,
                   r.ComentarioRechazo,
                   p.ClienteId,
                   ISNULL(u.NombreCompleto, u.Email)                       AS NombreEmpleado,
                   u.PuestoId,
                   u.SupervisorUserId                                      AS AprobadorNivel2,
                   p.SupervisorUserId                                      AS AprobadorNivel3,
                   sup.SupervisorUserId                                    AS AprobadorNivel1,
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada1, r.HoraSalida1), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada2, r.HoraSalida2), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada3, r.HoraSalida3), 0) AS TotalMinutos,
                   -- Nivel que toca revisar segun el estado (0=Pendiente .. 3=Aprobado)
                   CASE r.Estado WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 3 ELSE NULL END AS NivelPendiente
            FROM   RegistrosHoras r
            JOIN   Proyectos   p ON p.Id = r.ProyectoId
            JOIN   AspNetUsers u ON u.Id = r.UserId
            OUTER  APPLY (
                SELECT TOP 1 s.SupervisorUserId
                FROM   SupervisoresPuesto s
                WHERE  s.Activo = 1
                  AND  s.PuestoId = u.PuestoId
                  AND  (s.ClienteId IS NULL OR s.ClienteId = p.ClienteId)
                ORDER  BY CASE WHEN s.ClienteId IS NULL THEN 1 ELSE 0 END
            ) sup
            WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
              AND  (@UserId     IS NULL OR r.UserId    = @UserId)
              AND  (@ClienteId  IS NULL OR p.ClienteId = @ClienteId)
              AND  (@ProyectoId IS NULL OR r.ProyectoId = @ProyectoId)
              AND  (@PuestoId   IS NULL OR u.PuestoId  = @PuestoId)
        )
        SELECT b.Id,
               b.UserId,
               b.NombreEmpleado,
               b.FechaRegistro,
               b.ProyectoId,
               b.ClienteNombre  AS Cliente,
               b.ProyectoNombre AS Proyecto,
               b.Recurso,
               b.Modalidad,
               b.Descripcion,
               b.TotalMinutos,
               b.Estado,
               b.NivelPendiente,
               b.ComentarioRechazo,
               CASE
                   WHEN b.AprobadorNivel1 = @Revisor THEN 1
                   WHEN b.AprobadorNivel2 = @Revisor THEN 2
                   ELSE 3
               END AS NivelDelRevisor
        FROM   Base b
        WHERE  (
                   -- Pendientes que le tocan AHORA al revisor
                   (b.NivelPendiente = 1 AND b.AprobadorNivel1 = @Revisor)
                OR (b.NivelPendiente = 2 AND b.AprobadorNivel2 = @Revisor)
                OR (b.NivelPendiente = 3 AND b.AprobadorNivel3 = @Revisor)
                   -- Ya revisados: solo si se piden, y solo los que el revisor toco
                OR (@IncluirRevisados = 1
                    AND (b.AprobadorNivel1 = @Revisor
                      OR b.AprobadorNivel2 = @Revisor
                      OR b.AprobadorNivel3 = @Revisor))
               )
        ORDER  BY b.NombreEmpleado, b.FechaRegistro, b.ProyectoNombre;
        """;

    public async Task<PendientesAprobacionResponse> GetPendientesAsync(
        string revisorUserId,
        GetPendientesAprobacionQuery filtros,
        CancellationToken cancellationToken = default)
    {
        var filas = (await db.QueryAsync<RawRow>(new CommandDefinition(Sql, new
        {
            Revisor          = revisorUserId,
            filtros.Desde,
            filtros.Hasta,
            UserId           = string.IsNullOrWhiteSpace(filtros.UserId) ? null : filtros.UserId,
            filtros.ClienteId,
            filtros.ProyectoId,
            filtros.PuestoId,
            IncluirRevisados = filtros.IncluirRevisados ? 1 : 0
        }, cancellationToken: cancellationToken))).ToList();

        // El agrupamiento por (empleado, dia) se hace en memoria: la consulta ya vino
        // acotada al rango y al revisor, asi que es un conjunto pequeno.
        var dias = filas
            .GroupBy(f => new { f.UserId, f.NombreEmpleado, f.FechaRegistro })
            .Select(g => new DiaPendienteDto(
                g.Key.UserId,
                g.Key.NombreEmpleado,
                DateOnly.FromDateTime(g.Key.FechaRegistro),
                g.Sum(f => f.TotalMinutos),
                g.Select(f => new RegistroPendienteDto(
                    f.Id,
                    f.ProyectoId,
                    f.Cliente,
                    f.Proyecto,
                    f.Recurso,
                    f.Modalidad,
                    f.Descripcion,
                    f.TotalMinutos,
                    (EstadoAprobacion)f.Estado,
                    f.NivelPendiente,
                    f.ComentarioRechazo,
                    f.NivelDelRevisor)).ToList()))
            .OrderBy(d => d.NombreEmpleado).ThenBy(d => d.Fecha)
            .ToList();

        return new PendientesAprobacionResponse(
            filtros.Desde,
            filtros.Hasta,
            filas.Count,
            Math.Round(filas.Sum(f => f.TotalMinutos) / 60m, 2),
            dias);
    }

    private sealed record RawRow(
        int      Id,
        string   UserId,
        string   NombreEmpleado,
        DateTime FechaRegistro,
        int      ProyectoId,
        string   Cliente,
        string   Proyecto,
        string   Recurso,
        string   Modalidad,
        string   Descripcion,
        int      TotalMinutos,
        int      Estado,
        int?     NivelPendiente,
        string?  ComentarioRechazo,
        int      NivelDelRevisor);
}
