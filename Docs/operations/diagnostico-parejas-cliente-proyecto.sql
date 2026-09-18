/*
    Diagnostico: registros de horas cuya pareja (Cliente, Proyecto) no existe en el catalogo.

    Para que sirve
    --------------
    RegistrosHoras guarda Cliente y Proyecto como texto suelto, no como ProyectoId.
    Hasta que se agrego CreateRegistroHorasCatalogoValidator, la API aceptaba cualquier
    combinacion. Toda fila que aparezca aqui NO va a poder emparejarse automaticamente
    cuando se migre a ProyectoId (Fase 4) y habra que resolverla a mano.

    Como usarlo
    -----------
    Ejecutar contra la base de produccion. Si la consulta 1 devuelve 0, la migracion
    a ProyectoId sera limpia y no hace falta mirar el resto.

    Solo lee: no modifica nada.
*/

-- ─────────────────────────────────────────────────────────────────────────────
-- 1. Resumen: cuantos registros tienen pareja invalida
-- ─────────────────────────────────────────────────────────────────────────────
SELECT
    COUNT(*)                                                      AS TotalRegistros,
    SUM(CASE WHEN cat.ProyectoId IS NULL THEN 1 ELSE 0 END)       AS RegistrosInvalidos,
    CAST(100.0 * SUM(CASE WHEN cat.ProyectoId IS NULL THEN 1 ELSE 0 END)
         / NULLIF(COUNT(*), 0) AS decimal(5,2))                   AS PorcentajeInvalido
FROM       [dbo].[RegistrosHoras] r
LEFT JOIN (
    SELECT p.Id AS ProyectoId, c.Nombre AS Cliente, p.Nombre AS Proyecto
    FROM   [dbo].[Proyectos] p
    JOIN   [dbo].[Clientes]  c ON c.Id = p.ClienteId
) cat ON cat.Cliente = r.Cliente AND cat.Proyecto = r.Proyecto;

-- ─────────────────────────────────────────────────────────────────────────────
-- 2. Detalle agrupado: que parejas invalidas existen y cuantas veces
--    (incluye el motivo, para saber si se arregla creando el proyecto o corrigiendo el dato)
-- ─────────────────────────────────────────────────────────────────────────────
SELECT
    r.Cliente,
    r.Proyecto,
    COUNT(*)              AS Registros,
    MIN(r.FechaRegistro)  AS PrimeraFecha,
    MAX(r.FechaRegistro)  AS UltimaFecha,
    CASE
        WHEN NOT EXISTS (SELECT 1 FROM [dbo].[Clientes] c WHERE c.Nombre = r.Cliente)
            THEN 'Cliente inexistente'
        WHEN NOT EXISTS (SELECT 1 FROM [dbo].[Proyectos] p WHERE p.Nombre = r.Proyecto)
            THEN 'Proyecto inexistente'
        ELSE 'Proyecto existe pero pertenece a otro cliente'
    END                   AS Motivo
FROM       [dbo].[RegistrosHoras] r
LEFT JOIN (
    SELECT c.Nombre AS Cliente, p.Nombre AS Proyecto
    FROM   [dbo].[Proyectos] p
    JOIN   [dbo].[Clientes]  c ON c.Id = p.ClienteId
) cat ON cat.Cliente = r.Cliente AND cat.Proyecto = r.Proyecto
WHERE      cat.Cliente IS NULL
GROUP BY   r.Cliente, r.Proyecto
ORDER BY   Registros DESC;

-- ─────────────────────────────────────────────────────────────────────────────
-- 3. Parejas que existen en el catalogo pero con cliente o proyecto DESACTIVADO.
--    No rompen la migracion (el ProyectoId se resuelve igual), pero explican por que
--    un usuario ya no puede volver a registrar sobre esa combinacion.
-- ─────────────────────────────────────────────────────────────────────────────
SELECT
    r.Cliente,
    r.Proyecto,
    COUNT(*)      AS Registros,
    c.Activo      AS ClienteActivo,
    p.Activo      AS ProyectoActivo
FROM       [dbo].[RegistrosHoras] r
JOIN       [dbo].[Clientes]  c ON c.Nombre = r.Cliente
JOIN       [dbo].[Proyectos] p ON p.Nombre = r.Proyecto AND p.ClienteId = c.Id
WHERE      c.Activo = 0 OR p.Activo = 0
GROUP BY   r.Cliente, r.Proyecto, c.Activo, p.Activo
ORDER BY   Registros DESC;
