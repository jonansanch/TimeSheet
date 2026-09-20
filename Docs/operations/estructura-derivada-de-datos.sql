/* ===========================================================================
   Estructura organizacional DERIVADA de los datos que ya existen

   LA IDEA
   Falta asignarle un puesto a cada persona para que la cadena de aprobacion
   funcione. Pero ese dato ya existe: el campo Recurso de RegistrosHoras sale
   del mismo catalogo (Empleados = PUESTOS, no personas) y los empleados llevan
   meses escribiendolo en cada registro. El puesto de alguien es, simplemente,
   el Recurso que mas ha usado.

   Eso resuelve el NIVEL 1 sin preguntarle nada a nadie.

   QUE NO SE PUEDE DERIVAR
   - Nivel 2 (jefe directo): no hay ninguna señal en los timesheets sobre quien
     le reporta a quien. Se propone un arranque provisional.
   - Nivel 3 (responsable de proyecto): idem.
   Los dos se ajustan despues desde la UI sin tocar la base.

   COMO USARLO
   Los bloques 1 y 2 solo MIRAN: corrilos y revisa que el puesto propuesto para
   cada persona tenga sentido. El bloque 3 aplica, en transaccion con ROLLBACK.

   Hacer BACKUP antes. Complementa a estructura-inicial-produccion.sql: este
   sirve para arrancar con lo que hay; aquel, para cargar la estructura real.
   =========================================================================== */

-- ═══ BLOQUE 1 — Puesto propuesto por persona, con su evidencia ═════════════
-- Mira la columna Registros: si alguien tiene 40 registros como "Desarrollador"
-- la propuesta es solida. Si tiene 3 y 2, revisala a mano.

WITH Conteo AS (
    SELECT  r.UserId,
            r.Recurso,
            COUNT(*)                                       AS Veces,
            MAX(r.FechaRegistro)                           AS UltimoUso,
            ROW_NUMBER() OVER (PARTITION BY r.UserId
                               ORDER BY COUNT(*) DESC, MAX(r.FechaRegistro) DESC) AS Ranking
    FROM    RegistrosHoras r
    WHERE   r.Recurso IS NOT NULL AND LTRIM(RTRIM(r.Recurso)) <> ''
    GROUP   BY r.UserId, r.Recurso
)
SELECT  ISNULL(u.NombreCompleto, u.Email)  AS Empleado,
        u.Email,
        c.Recurso                          AS PuestoPropuesto,
        c.Veces                            AS RegistrosConEsePuesto,
        (SELECT SUM(c2.Veces) FROM Conteo c2 WHERE c2.UserId = c.UserId) AS RegistrosTotales,
        c.UltimoUso,
        CASE WHEN e.Id IS NULL THEN 'NO EXISTE EN EL CATALOGO' ELSE 'ok' END AS EstadoDelPuesto,
        CASE WHEN u.PuestoId IS NOT NULL   THEN 'ya tiene, no se toca' ELSE 'se asignaria' END AS Accion
FROM    Conteo c
JOIN    AspNetUsers u ON u.Id = c.UserId
LEFT    JOIN Empleados e ON e.Nombre = c.Recurso AND e.Activo = 1
WHERE   c.Ranking = 1 AND u.IsActive = 1
ORDER   BY RegistrosTotales DESC;


-- ═══ BLOQUE 2 — Quien queda afuera y por que ═══════════════════════════════
-- Estos son los casos que el script NO puede resolver solo.

-- 2.a  Activos que nunca registraron horas: no hay de donde derivar su puesto.
SELECT  ISNULL(u.NombreCompleto, u.Email) AS Empleado, u.Email,
        'Sin registros: asignar el puesto a mano' AS Motivo
FROM    AspNetUsers u
WHERE   u.IsActive = 1 AND u.PuestoId IS NULL
  AND   NOT EXISTS (SELECT 1 FROM RegistrosHoras r WHERE r.UserId = u.Id);

-- 2.b  Recursos escritos que no estan en el catalogo activo (texto viejo o
--      un puesto que se desactivo). Hay que crearlo o corregir el registro.
SELECT  DISTINCT r.Recurso AS RecursoHuerfano, COUNT(*) OVER (PARTITION BY r.Recurso) AS Registros
FROM    RegistrosHoras r
WHERE   LTRIM(RTRIM(ISNULL(r.Recurso, ''))) <> ''
  AND   NOT EXISTS (SELECT 1 FROM Empleados e WHERE e.Nombre = r.Recurso AND e.Activo = 1);

-- 2.c  Candidatos a supervisor: quien tiene rol para aprobar.
--      Si hay exactamente uno de cada, el bloque 3 puede arrancar solo.
SELECT  ISNULL(u.NombreCompleto, u.Email) AS Persona, u.Email, rol.Name AS Rol
FROM    AspNetUsers u
JOIN    AspNetUserRoles ur ON ur.UserId = u.Id
JOIN    AspNetRoles rol    ON rol.Id = ur.RoleId
WHERE   u.IsActive = 1 AND rol.Name IN ('Supervisor', 'Gerente', 'Admin')
ORDER   BY rol.Name;

GO

-- ═══ BLOQUE 3 — Aplicar. Revisar los bloques 1 y 2 antes de correr esto. ═══

BEGIN TRANSACTION;

-- ── 3.a  Puesto derivado del Recurso mas usado (NIVEL 1) ──────────────────
WITH Conteo AS (
    SELECT  r.UserId, r.Recurso,
            ROW_NUMBER() OVER (PARTITION BY r.UserId
                               ORDER BY COUNT(*) DESC, MAX(r.FechaRegistro) DESC) AS Ranking
    FROM    RegistrosHoras r
    WHERE   r.Recurso IS NOT NULL AND LTRIM(RTRIM(r.Recurso)) <> ''
    GROUP   BY r.UserId, r.Recurso
)
UPDATE  u
SET     u.PuestoId = e.Id
FROM    AspNetUsers u
JOIN    Conteo c   ON c.UserId = u.Id AND c.Ranking = 1
JOIN    Empleados e ON e.Nombre = c.Recurso AND e.Activo = 1
WHERE   u.IsActive = 1 AND u.PuestoId IS NULL;

-- ── 3.b  Arranque provisional de los niveles 1, 2 y 3 ─────────────────────
-- Esto NO sale de los datos: es un punto de partida para que el flujo corra
-- hoy. Se corrige desde la UI (Usuarios, /admin/supervisores-puesto y el
-- dialogo de proyecto) sin volver a tocar la base.

DECLARE @SupervisorId nvarchar(450) = (
    SELECT TOP 1 u.Id FROM AspNetUsers u
    JOIN AspNetUserRoles ur ON ur.UserId = u.Id
    JOIN AspNetRoles r      ON r.Id = ur.RoleId
    WHERE u.IsActive = 1 AND r.Name = 'Supervisor' ORDER BY u.Created);

DECLARE @GerenteId nvarchar(450) = (
    SELECT TOP 1 u.Id FROM AspNetUsers u
    JOIN AspNetUserRoles ur ON ur.UserId = u.Id
    JOIN AspNetRoles r      ON r.Id = ur.RoleId
    WHERE u.IsActive = 1 AND r.Name IN ('Gerente', 'Admin')
    ORDER BY CASE WHEN r.Name = 'Gerente' THEN 0 ELSE 1 END, u.Created);

IF @SupervisorId IS NULL OR @GerenteId IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50020, 'No hay un Supervisor y un Gerente/Admin activos: sin ellos no hay a quien asignarle los niveles.', 1;
END

-- Nivel 1: una regla general por cada puesto que alguien este usando.
INSERT  INTO SupervisoresPuesto (PuestoId, ClienteId, SupervisorUserId, Activo, Created, LastModified)
SELECT  DISTINCT u.PuestoId, NULL, @SupervisorId, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
FROM    AspNetUsers u
WHERE   u.IsActive = 1 AND u.PuestoId IS NOT NULL
  AND   NOT EXISTS (SELECT 1 FROM SupervisoresPuesto sp
                    WHERE sp.PuestoId = u.PuestoId AND sp.ClienteId IS NULL);

-- Nivel 2: el equipo cuelga del supervisor; el supervisor, del gerente.
UPDATE  u SET u.SupervisorUserId = @SupervisorId
FROM    AspNetUsers u
WHERE   u.IsActive = 1 AND u.SupervisorUserId IS NULL AND u.Id <> @SupervisorId AND u.Id <> @GerenteId;

UPDATE  u SET u.SupervisorUserId = @GerenteId
FROM    AspNetUsers u
WHERE   u.IsActive = 1 AND u.SupervisorUserId IS NULL AND u.Id = @SupervisorId;

-- Nivel 3: el gerente responde por los proyectos hasta que se reparta.
UPDATE  p SET p.SupervisorUserId = @GerenteId
FROM    Proyectos p
WHERE   p.Activo = 1 AND p.SupervisorUserId IS NULL;


-- ── 3.c  Verificar que ningun registro quede trabado ──────────────────────
-- Un nivel sin aprobador no lo aprueba nadie: si esto devuelve filas, esos
-- registros no se podrian aprobar jamas.

SELECT  ISNULL(u.NombreCompleto, u.Email) AS Empleado,
        c.Nombre AS Cliente, p.Nombre AS Proyecto,
        CASE WHEN sup1.SupervisorUserId IS NULL THEN 'FALTA' ELSE 'ok' END AS Nivel1_Puesto,
        CASE WHEN u.SupervisorUserId     IS NULL THEN 'FALTA' ELSE 'ok' END AS Nivel2_Jefe,
        CASE WHEN p.SupervisorUserId     IS NULL THEN 'FALTA' ELSE 'ok' END AS Nivel3_Proyecto
FROM    RegistrosHoras r
JOIN    AspNetUsers u ON u.Id = r.UserId
JOIN    Proyectos p   ON p.Id = r.ProyectoId
JOIN    Clientes c    ON c.Id = p.ClienteId
OUTER   APPLY (
            SELECT TOP 1 sp.SupervisorUserId
            FROM   SupervisoresPuesto sp
            WHERE  sp.Activo = 1 AND sp.PuestoId = u.PuestoId
              AND (sp.ClienteId IS NULL OR sp.ClienteId = p.ClienteId)
            ORDER  BY CASE WHEN sp.ClienteId IS NULL THEN 1 ELSE 0 END
        ) sup1
WHERE   r.Estado = 0
  AND  (sup1.SupervisorUserId IS NULL OR u.SupervisorUserId IS NULL OR p.SupervisorUserId IS NULL)
GROUP   BY ISNULL(u.NombreCompleto, u.Email), c.Nombre, p.Nombre,
           sup1.SupervisorUserId, u.SupervisorUserId, p.SupervisorUserId;
-- Vacio = todo registro pendiente tiene quien lo apruebe en los tres niveles.

-- Resumen de lo aplicado.
SELECT 'Usuarios con puesto'        AS Concepto, COUNT(*) AS Cantidad FROM AspNetUsers WHERE IsActive = 1 AND PuestoId IS NOT NULL
UNION ALL SELECT 'Usuarios con jefe',           COUNT(*) FROM AspNetUsers WHERE IsActive = 1 AND SupervisorUserId IS NOT NULL
UNION ALL SELECT 'Proyectos con responsable',   COUNT(*) FROM Proyectos   WHERE Activo = 1 AND SupervisorUserId IS NOT NULL
UNION ALL SELECT 'Reglas de supervisor puesto', COUNT(*) FROM SupervisoresPuesto WHERE Activo = 1;

ROLLBACK TRANSACTION;   -- cambiar a COMMIT cuando la verificacion salga vacia
