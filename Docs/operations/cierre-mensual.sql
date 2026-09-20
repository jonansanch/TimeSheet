/* ===========================================================================
   Cierre mensual de registros

   PARA QUE
   Los registros se aprueban dia por dia, pero el timesheet se entrega y se
   factura POR MES. Cuando un mes ya se entrego, no tiene sentido que sus
   registros sigan apareciendo como cola pendiente: se cierran.

   Tambien resuelve el arranque: los registros anteriores a la puesta en
   marcha del flujo estan todos en Pendiente. Se cierran mes a mes, del mas
   viejo al mas nuevo, hasta llegar al mes en curso — que se deja abierto para
   que el flujo normal corra sobre el.

   COMO USARLO
   1. Corre el BLOQUE 0: lista que meses hay abiertos y cuanto tiene cada uno.
   2. Para cada mes que quieras cerrar, ajusta @Mes/@Anio en el BLOQUE 1 y
      ejecutalo. Es un mes por corrida, a proposito: cerrar es irreversible
      desde la UI y conviene mirar los numeros de cada mes antes de hacerlo.
   3. El bloque 1 arranca con ROLLBACK. Cambialo a COMMIT cuando cuadre.

   QUE HACE EXACTAMENTE
   Marca los registros del mes como Aprobado (Estado = 3) y deja una fila en
   AprobacionesRegistro explicando que fue un cierre de mes, no una revision
   persona por persona. Sin ese rastro, el historial diria que alguien los
   aprobo uno por uno, que no es lo que paso.

   NO toca los registros ya rechazados (Estado = 4): esos necesitan que el
   empleado los corrija, y cerrarlos taparia el problema.

   Hacer BACKUP antes.
   =========================================================================== */

-- ═══ BLOQUE 0 — Que meses hay y en que estado ══════════════════════════════

SELECT  YEAR(r.FechaRegistro)  AS Anio,
        MONTH(r.FechaRegistro) AS Mes,
        COUNT(*)                                                  AS Registros,
        SUM(CASE WHEN r.Estado = 0 THEN 1 ELSE 0 END)             AS Pendientes,
        SUM(CASE WHEN r.Estado IN (1,2) THEN 1 ELSE 0 END)        AS EnCurso,
        SUM(CASE WHEN r.Estado = 3 THEN 1 ELSE 0 END)             AS Aprobados,
        SUM(CASE WHEN r.Estado = 4 THEN 1 ELSE 0 END)             AS Rechazados,
        COUNT(DISTINCT r.UserId)                                  AS Empleados,
        CAST(ROUND(SUM(
            ISNULL(DATEDIFF(MINUTE, r.HoraEntrada1, r.HoraSalida1), 0) +
            ISNULL(DATEDIFF(MINUTE, r.HoraEntrada2, r.HoraSalida2), 0) +
            ISNULL(DATEDIFF(MINUTE, r.HoraEntrada3, r.HoraSalida3), 0)
        ) / 60.0, 2) AS decimal(10,2))                            AS Horas
FROM    RegistrosHoras r
GROUP   BY YEAR(r.FechaRegistro), MONTH(r.FechaRegistro)
ORDER   BY Anio, Mes;

GO

-- ═══ BLOQUE 1 — Cerrar UN mes. Ajustar y ejecutar por cada mes. ════════════

DECLARE @Mes  int = 8;        -- <<< mes a cerrar
DECLARE @Anio int = 2026;     -- <<< año

DECLARE @Actor nvarchar(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'admin@kpg.com');
DECLARE @Motivo nvarchar(1000) =
    N'Cierre mensual ' + RIGHT('0' + CAST(@Mes AS varchar), 2) + N'/' + CAST(@Anio AS varchar)
  + N': el timesheet del mes ya fue entregado.';

IF @Actor IS NULL THROW 50030, 'No se encontro el usuario que figura como responsable del cierre.', 1;

-- No cerrar un mes que todavia esta corriendo: quedaria a medias.
IF @Anio = YEAR(GETDATE()) AND @Mes = MONTH(GETDATE())
    THROW 50031, 'Ese es el mes en curso: se cierra cuando termine.', 1;

BEGIN TRANSACTION;

DECLARE @Desde date = DATEFROMPARTS(@Anio, @Mes, 1);
DECLARE @Hasta date = DATEADD(MONTH, 1, @Desde);   -- exclusivo

-- Se toman los ids primero: despues del UPDATE ya no se distinguen de los que
-- venian aprobados por el flujo normal.
DECLARE @Cerrados TABLE (Id int PRIMARY KEY);

INSERT  INTO @Cerrados (Id)
SELECT  r.Id
FROM    RegistrosHoras r
WHERE   r.FechaRegistro >= @Desde
  AND   r.FechaRegistro <  @Hasta
  AND   r.Estado IN (0, 1, 2);      -- pendiente o a medio aprobar; el rechazado NO

UPDATE  r
SET     r.Estado = 3,               -- Aprobado
        r.LastModified = SYSDATETIMEOFFSET()
FROM    RegistrosHoras r
JOIN    @Cerrados c ON c.Id = r.Id;

INSERT  INTO AprobacionesRegistro
        (RegistroHorasId, Accion, Nivel, ActorUserId, Comentario, Created, LastModified)
SELECT  c.Id, 'Aprobar', 3, @Actor, @Motivo, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
FROM    @Cerrados c;

-- ── Verificacion ──────────────────────────────────────────────────────────
SELECT  'Mes cerrado'        AS Concepto, CAST(@Mes AS varchar) + '/' + CAST(@Anio AS varchar) AS Valor
UNION ALL SELECT 'Registros cerrados',    CAST((SELECT COUNT(*) FROM @Cerrados) AS varchar)
UNION ALL SELECT 'Rechazados intactos',   CAST((SELECT COUNT(*) FROM RegistrosHoras
                                                WHERE FechaRegistro >= @Desde AND FechaRegistro < @Hasta
                                                  AND Estado = 4) AS varchar)
UNION ALL SELECT 'Quedan sin cerrar',     CAST((SELECT COUNT(*) FROM RegistrosHoras
                                                WHERE FechaRegistro >= @Desde AND FechaRegistro < @Hasta
                                                  AND Estado IN (0,1,2)) AS varchar);

ROLLBACK TRANSACTION;   -- <<< cambiar a COMMIT cuando los numeros cuadren
