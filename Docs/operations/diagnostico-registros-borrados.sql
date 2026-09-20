/* ===========================================================================
   Diagnostico: ¿el DELETE sin guard del inicializador borro registros?

   CONTEXTO
   El paso 5 de la migracion "registro unico diario" incluia este DELETE, que
   quedo SIN GUARD y corria en CADA arranque del servidor:

       DELETE r FROM RegistrosHoras r
       WHERE EXISTS (SELECT 1 FROM RegistrosHoras r2
                     WHERE r2.UserId = r.UserId
                       AND r2.FechaRegistro = r.FechaRegistro
                       AND r2.Id < r.Id)

   Con el modelo viejo (un registro por dia) borraba duplicados de verdad. Con
   el modelo actual —donde un empleado puede imputar VARIOS PROYECTOS el mismo
   dia— borraba todos los registros de un dia menos el de menor Id.

   Ya esta corregido (lleva el mismo guard que su UPDATE companero), pero falta
   saber si alcanzo a destruir datos antes del arreglo.

   COMO LEERLO
   La consulta 1 es la que responde la pregunta. Si devuelve 0 filas Y sabes que
   hay gente que imputa a dos proyectos el mismo dia, entonces SI hubo perdida.
   Si devuelve filas, el borrado no llego a ocurrir (o no alcanzo a esos dias).

   Ejecutar contra la base de PRODUCCION. Solo lee, no modifica nada.
   =========================================================================== */

-- ── 1. ¿Existe HOY algun dia con mas de un proyecto por empleado? ────────────
--    Es la evidencia directa: si el DELETE hubiera corrido despues del ultimo
--    registro de esos dias, no quedaria ninguno.
SELECT  r.UserId,
        ISNULL(u.NombreCompleto, u.Email) AS Empleado,
        r.FechaRegistro,
        COUNT(*)                          AS RegistrosEseDia,
        STRING_AGG(r.ProyectoNombre, ' | ') AS Proyectos
FROM    RegistrosHoras r
JOIN    AspNetUsers u ON u.Id = r.UserId
GROUP   BY r.UserId, ISNULL(u.NombreCompleto, u.Email), r.FechaRegistro
HAVING  COUNT(*) > 1
ORDER   BY r.FechaRegistro DESC;


-- ── 2. Resumen: cuantos dias multi-proyecto sobreviven ───────────────────────
SELECT  COUNT(*) AS DiasConMasDeUnProyecto
FROM   (SELECT r.UserId, r.FechaRegistro
        FROM   RegistrosHoras r
        GROUP  BY r.UserId, r.FechaRegistro
        HAVING COUNT(*) > 1) AS d;


-- ── 3. Huecos en la secuencia de Id ──────────────────────────────────────────
--    Un Id faltante puede ser un borrado (por el bug o por el usuario) o un
--    INSERT que fallo. No prueba nada por si solo, pero da la magnitud: si
--    faltan muchos Id consecutivos, conviene mirar la bitacora.
SELECT  MIN(Id)                      AS PrimerId,
        MAX(Id)                      AS UltimoId,
        COUNT(*)                     AS RegistrosExistentes,
        MAX(Id) - MIN(Id) + 1        AS RangoDeIds,
        MAX(Id) - MIN(Id) + 1 - COUNT(*) AS IdsFaltantes
FROM    RegistrosHoras;


-- ── 4. Borrados registrados en la bitacora ───────────────────────────────────
--    El bug borraba por SQL directo, SIN pasar por la aplicacion: NO deja
--    rastro aqui. Por eso, un Id faltante que no aparezca en esta lista es
--    sospechoso de haber sido borrado por el bug.
SELECT  b.Timestamp, b.TipoEvento, b.EntidadId, b.ActorEmail, b.MetadataJson
FROM    BitacoraAuditoria b
WHERE   b.EntidadAfectada = 'RegistrosHoras'
  AND   b.TipoEvento LIKE '%liminad%'
ORDER   BY b.Timestamp DESC;


-- ── 5. Dias con un solo registro y menos de 8 horas ──────────────────────────
--    Candidatos a haber perdido su segundo proyecto: quedo un registro corto
--    donde probablemente habia dos. Es un indicio, no una prueba.
SELECT  ISNULL(u.NombreCompleto, u.Email) AS Empleado,
        r.FechaRegistro,
        r.ProyectoNombre,
        (ISNULL(DATEDIFF(MINUTE, r.HoraEntrada1, r.HoraSalida1), 0) +
         ISNULL(DATEDIFF(MINUTE, r.HoraEntrada2, r.HoraSalida2), 0) +
         ISNULL(DATEDIFF(MINUTE, r.HoraEntrada3, r.HoraSalida3), 0)) / 60.0 AS Horas
FROM    RegistrosHoras r
JOIN    AspNetUsers u ON u.Id = r.UserId
WHERE   (ISNULL(DATEDIFF(MINUTE, r.HoraEntrada1, r.HoraSalida1), 0) +
         ISNULL(DATEDIFF(MINUTE, r.HoraEntrada2, r.HoraSalida2), 0) +
         ISNULL(DATEDIFF(MINUTE, r.HoraEntrada3, r.HoraSalida3), 0)) < 480
  AND   NOT EXISTS (SELECT 1 FROM RegistrosHoras r2
                    WHERE r2.UserId = r.UserId
                      AND r2.FechaRegistro = r.FechaRegistro
                      AND r2.Id <> r.Id)
ORDER   BY r.FechaRegistro DESC;
