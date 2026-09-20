/* ===========================================================================
   Puesta en marcha del flujo de aprobaciones en PRODUCCION

   QUE RESUELVE
   El codigo de aprobaciones esta completo, pero hoy en produccion nadie tiene
   puesto ni jefe, no hay reglas de supervisor por puesto y ningun proyecto
   tiene responsable. Con eso, los tres niveles de la cadena resuelven vacios y
   NINGUN registro se puede aprobar (un nivel sin aprobador no lo aprueba
   nadie, por diseño). Este script carga esa estructura.

   COMO USARLO
   1. Corre el BLOQUE 0 para ver el estado actual y los ids reales.
   2. Llena las tablas del BLOQUE 1 con la estructura real de KPG.
   3. Corre los bloques 1 a 4 dentro de la transaccion.
   4. Para los registros historicos, usar Docs/operations/cierre-mensual.sql
      (el cierre es por mes, no por una unica fecha de corte).

   El script es idempotente: no pisa lo que ya este configurado a mano.
   Hacer BACKUP antes. Los bloques 1-4 estan en una transaccion con ROLLBACK
   al final: cambia a COMMIT cuando los conteos te cuadren.
   =========================================================================== */

-- ═══ BLOQUE 0 — Diagnostico. Correr primero, no modifica nada. ═════════════

SELECT 'Usuarios sin puesto'      AS Situacion, COUNT(*) AS Cantidad FROM AspNetUsers WHERE IsActive = 1 AND PuestoId IS NULL
UNION ALL SELECT 'Usuarios sin jefe',          COUNT(*) FROM AspNetUsers WHERE IsActive = 1 AND SupervisorUserId IS NULL
UNION ALL SELECT 'Proyectos sin responsable',  COUNT(*) FROM Proyectos   WHERE Activo = 1 AND SupervisorUserId IS NULL
UNION ALL SELECT 'Reglas de supervisor puesto',COUNT(*) FROM SupervisoresPuesto WHERE Activo = 1
UNION ALL SELECT 'Registros pendientes',       COUNT(*) FROM RegistrosHoras WHERE Estado = 0;

-- Los puestos disponibles (el catalogo "Empleados" son PUESTOS, no personas).
SELECT Id, Nombre, Activo FROM Empleados ORDER BY Nombre;

-- Las personas reales, con lo que ya tengan asignado.
SELECT  u.Id, u.Email, u.NombreCompleto, r.Name AS Rol,
        pue.Nombre AS Puesto, ISNULL(jefe.NombreCompleto, jefe.Email) AS JefeDirecto
FROM    AspNetUsers u
LEFT    JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT    JOIN AspNetRoles r      ON ur.RoleId = r.Id
LEFT    JOIN Empleados pue      ON u.PuestoId = pue.Id
LEFT    JOIN AspNetUsers jefe   ON u.SupervisorUserId = jefe.Id
WHERE   u.IsActive = 1
ORDER   BY r.Name, u.NombreCompleto;

GO

-- ═══ BLOQUES 1-4 — Carga de la estructura ══════════════════════════════════

BEGIN TRANSACTION;

-- ── BLOQUE 1: llena estas tres tablas con la estructura real ───────────────

-- 1.a  Quien ocupa que puesto, y quien es su jefe directo (= nivel 2).
--      El jefe se identifica por email. TODOS tienen jefe: la cabeza de la
--      organizacion se pone a si misma, y ahi termina la cadena.
DECLARE @Personas TABLE (Email nvarchar(256), Puesto nvarchar(200), EmailJefe nvarchar(256));
INSERT INTO @Personas (Email, Puesto, EmailJefe) VALUES
    -- ('gerente@kpg.com',     'Lider tecnico',  'gerente@kpg.com'),   -- la cabeza: su propio jefe
    -- ('supervisor@kpg.com',  'Lider tecnico',  'gerente@kpg.com'),
    -- ('empleado@kpg.com',    'Desarrollador',  'supervisor@kpg.com'),
    (NULL, NULL, NULL);   -- <<< BORRAR esta fila y poner las reales

-- 1.b  Supervisor de cada puesto (= nivel 1, la primera aprobacion).
--      Cliente NULL = regla general para todos los clientes.
--      Con cliente  = regla especifica, que GANA sobre la general del mismo puesto.
DECLARE @SupervisoresPuesto TABLE (Puesto nvarchar(200), EmailSupervisor nvarchar(256), Cliente nvarchar(200));
INSERT INTO @SupervisoresPuesto (Puesto, EmailSupervisor, Cliente) VALUES
    -- ('Desarrollador', 'supervisor@kpg.com', NULL),
    -- ('Consultor SAP', 'gerente@kpg.com',    'Petrocol'),
    (NULL, NULL, NULL);   -- <<< BORRAR esta fila y poner las reales

-- 1.c  Responsable de cada proyecto (= nivel 3, la ultima aprobacion).
--      El nombre de proyecto solo es unico por cliente: por eso van los dos.
DECLARE @SupervisoresProyecto TABLE (Cliente nvarchar(200), Proyecto nvarchar(200), EmailSupervisor nvarchar(256));
INSERT INTO @SupervisoresProyecto (Cliente, Proyecto, EmailSupervisor) VALUES
    -- ('Banco Nacional', 'Core Bancario', 'gerente@kpg.com'),
    (NULL, NULL, NULL);   -- <<< BORRAR esta fila y poner las reales

DELETE FROM @Personas             WHERE Email IS NULL;
DELETE FROM @SupervisoresPuesto   WHERE Puesto IS NULL;
DELETE FROM @SupervisoresProyecto WHERE Cliente IS NULL;


-- ── BLOQUE 2: validacion. Aborta si algo no existe, antes de tocar nada ────
-- Vale mas fallar aqui que dejar la estructura a medias y no saberlo.

DECLARE @errores nvarchar(max) = N'';

SELECT @errores = @errores + N'Email sin usuario activo: ' + p.Email + N'. '
FROM @Personas p WHERE NOT EXISTS (SELECT 1 FROM AspNetUsers u WHERE u.Email = p.Email AND u.IsActive = 1);

SELECT @errores = @errores + N'Puesto inexistente: ' + p.Puesto + N'. '
FROM @Personas p WHERE NOT EXISTS (SELECT 1 FROM Empleados e WHERE e.Nombre = p.Puesto AND e.Activo = 1);

SELECT @errores = @errores + N'Persona sin jefe asignado: ' + p.Email
     + N'. La cabeza debe ponerse a si misma. '
FROM @Personas p WHERE p.EmailJefe IS NULL;

SELECT @errores = @errores + N'Jefe sin usuario activo: ' + p.EmailJefe + N'. '
FROM @Personas p WHERE p.EmailJefe IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM AspNetUsers u WHERE u.Email = p.EmailJefe AND u.IsActive = 1);

SELECT @errores = @errores + N'Supervisor de puesto sin usuario activo: ' + s.EmailSupervisor + N'. '
FROM @SupervisoresPuesto s
WHERE NOT EXISTS (SELECT 1 FROM AspNetUsers u WHERE u.Email = s.EmailSupervisor AND u.IsActive = 1);

SELECT @errores = @errores + N'Cliente inexistente en regla de puesto: ' + s.Cliente + N'. '
FROM @SupervisoresPuesto s WHERE s.Cliente IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Clientes c WHERE c.Nombre = s.Cliente);

SELECT @errores = @errores + N'Proyecto inexistente: ' + sp.Cliente + N' / ' + sp.Proyecto + N'. '
FROM @SupervisoresProyecto sp
WHERE NOT EXISTS (SELECT 1 FROM Proyectos p JOIN Clientes c ON c.Id = p.ClienteId
                  WHERE c.Nombre = sp.Cliente AND p.Nombre = sp.Proyecto);

IF LEN(@errores) > 0
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50010, @errores, 1;
END


-- ── BLOQUE 3: aplicar. Solo completa lo vacio, no pisa lo ya configurado ───

UPDATE  u
SET     u.PuestoId = e.Id
FROM    AspNetUsers u
JOIN    @Personas p ON p.Email = u.Email
JOIN    Empleados e ON e.Nombre = p.Puesto
WHERE   u.PuestoId IS NULL;

UPDATE  u
SET     u.SupervisorUserId = jefe.Id
FROM    AspNetUsers u
JOIN    @Personas p     ON p.Email = u.Email
JOIN    AspNetUsers jefe ON jefe.Email = p.EmailJefe
WHERE   u.SupervisorUserId IS NULL;   -- la cabeza se pone a si misma: es valido

INSERT  INTO SupervisoresPuesto (PuestoId, ClienteId, SupervisorUserId, Activo, Created, LastModified)
SELECT  e.Id, c.Id, u.Id, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
FROM    @SupervisoresPuesto s
JOIN    Empleados e    ON e.Nombre = s.Puesto
JOIN    AspNetUsers u  ON u.Email  = s.EmailSupervisor
LEFT    JOIN Clientes c ON c.Nombre = s.Cliente
WHERE   NOT EXISTS (
            SELECT 1 FROM SupervisoresPuesto sp
            WHERE sp.PuestoId = e.Id
              AND ((sp.ClienteId IS NULL AND c.Id IS NULL) OR sp.ClienteId = c.Id));

UPDATE  p
SET     p.SupervisorUserId = u.Id
FROM    Proyectos p
JOIN    Clientes c            ON c.Id = p.ClienteId
JOIN    @SupervisoresProyecto sp ON sp.Cliente = c.Nombre AND sp.Proyecto = p.Nombre
JOIN    AspNetUsers u         ON u.Email = sp.EmailSupervisor
WHERE   p.SupervisorUserId IS NULL;


-- ── BLOQUE 4: verificar que la cadena queda completa para todos ────────────
-- Un registro cuyo nivel 1, 2 o 3 quede sin aprobador se traba para siempre.

SELECT  ISNULL(u.NombreCompleto, u.Email) AS Empleado,
        c.Nombre  AS Cliente,
        p.Nombre  AS Proyecto,
        CASE WHEN sup1.SupervisorUserId IS NULL THEN 'FALTA' ELSE 'ok' END AS Nivel1_Puesto,
        CASE WHEN u.SupervisorUserId     IS NULL THEN 'FALTA' ELSE 'ok' END AS Nivel2_Jefe,
        CASE WHEN p.SupervisorUserId     IS NULL THEN 'FALTA' ELSE 'ok' END AS Nivel3_Proyecto
FROM    AspNetUsers u
CROSS   JOIN Proyectos p
JOIN    Clientes c ON c.Id = p.ClienteId
OUTER   APPLY (
            SELECT TOP 1 sp.SupervisorUserId
            FROM   SupervisoresPuesto sp
            WHERE  sp.Activo = 1
              AND  sp.PuestoId = u.PuestoId
              AND (sp.ClienteId IS NULL OR sp.ClienteId = p.ClienteId)
            ORDER  BY CASE WHEN sp.ClienteId IS NULL THEN 1 ELSE 0 END
        ) sup1
WHERE   u.IsActive = 1 AND p.Activo = 1 AND c.Activo = 1
  AND  (sup1.SupervisorUserId IS NULL OR u.SupervisorUserId IS NULL OR p.SupervisorUserId IS NULL)
ORDER   BY Empleado, Cliente, Proyecto;
-- Vacio = la cadena resuelve completa para toda combinacion empleado/proyecto.

-- Cambiar a COMMIT cuando los conteos y la verificacion cuadren.
ROLLBACK TRANSACTION;

GO

/* ═══ BLOQUE 5 — Registros historicos ═══════════════════════════════════════
   Se movio a su propio script: Docs/operations/cierre-mensual.sql
   El cierre es MENSUAL (el timesheet se entrega por mes), no por una unica
   fecha de corte, asi que se ejecuta un mes por corrida.
   ═══════════════════════════════════════════════════════════════════════════ */
