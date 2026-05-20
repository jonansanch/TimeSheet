-- ══════════════════════════════════════════════════════════════════
-- PASO 1: Eliminar los 9 registros con horas solapadas
-- ══════════════════════════════════════════════════════════════════
DELETE FROM RegistrosHoras WHERE Id IN (76,77,78,79,80,81,82,83,84);

-- ══════════════════════════════════════════════════════════════════
-- PASO 2: Acortar los registros originales (primera mitad del turno)
-- ══════════════════════════════════════════════════════════════════

-- empleado — 05-may — AM — ACUDEN queda 08:00-10:30
UPDATE RegistrosHoras SET HoraSalida='10:30' WHERE Id=34;

-- empleado — 05-may — PM — ACUDEN queda 14:00-16:00
UPDATE RegistrosHoras SET HoraSalida='16:00' WHERE Id=35;

-- empleado — 07-may — AM — ACUDEN queda 08:00-10:00
UPDATE RegistrosHoras SET HoraSalida='10:00' WHERE Id=38;

-- ana — 05-may — AM — Banco Central queda 08:00-10:00
UPDATE RegistrosHoras SET HoraSalida='10:00' WHERE Id=45;

-- ana — 08-may — PM — Banco Central queda 13:30-15:30
UPDATE RegistrosHoras SET HoraSalida='15:30' WHERE Id=52;

-- carlos — 06-may — AM — Citi queda 08:30-10:30
UPDATE RegistrosHoras SET HoraSalida='10:30' WHERE Id=58;

-- carlos — 08-may — AM — Citi queda 08:30-11:00
UPDATE RegistrosHoras SET HoraSalida='11:00' WHERE Id=62;

-- supervisor — 06-may — AM — ACUDEN queda 08:00-10:30
UPDATE RegistrosHoras SET HoraSalida='10:30' WHERE Id=69;

-- supervisor — 08-may — PM — ACUDEN queda 14:00-16:00
UPDATE RegistrosHoras SET HoraSalida='16:00' WHERE Id=74;

-- ══════════════════════════════════════════════════════════════════
-- PASO 3: Insertar los segundos registros con horas contiguas (sin cruce)
-- ══════════════════════════════════════════════════════════════════
DECLARE @Emp  NVARCHAR(450) = '987b1ff3-b60e-49f7-9e75-bfd9b4258e8b';
DECLARE @Ana  NVARCHAR(450) = '33DE0281-1706-4ED6-92B5-53B2E8A98B82';
DECLARE @Car  NVARCHAR(450) = 'DBC3F75F-2618-42B1-9751-645414538922';
DECLARE @Sup  NVARCHAR(450) = '912b1018-7ace-41e5-a027-5498d5ff5b25';
DECLARE @Now  DATETIMEOFFSET = GETUTCDATE();

INSERT INTO RegistrosHoras
(UserId,FechaRegistro,Turno,HoraEntrada,HoraSalida,Cliente,Proyecto,Modalidad,Recurso,Descripcion,Lugar,Created,CreatedBy,LastModified,LastModifiedBy,EsRetroactivo)
VALUES
-- empleado — 05-may — AM — Banco Central: 10:30-13:00 (continua tras ACUDEN 08:00-10:30)
(@Emp,'2026-05-05','AM','10:30','13:00','Banco Central','Soporte Tecnico Core','Remoto','Desarrolladores','Hotfix incidente critico en modulo de transferencias. Solucion desplegada en produccion.','Casa',@Now,@Emp,@Now,@Emp,0),

-- empleado — 05-may — PM — Citi: 16:00-18:00 (continua tras ACUDEN 14:00-16:00)
(@Emp,'2026-05-05','PM','16:00','18:00','Citi','Analisis de Riesgos Crediticios','Remoto','Desarrolladores','Revision de reportes de riesgo crediticio para cierre semanal Citi.','Casa',@Now,@Emp,@Now,@Emp,0),

-- empleado — 07-may — AM — Mapfre: 10:00-13:00 (continua tras ACUDEN 08:00-10:00)
(@Emp,'2026-05-07','AM','10:00','13:00','Mapfre','Portal de Agentes Digitales','Remoto','Desarrolladores','Revision de codigo e integracion API cotizaciones Mapfre.','Casa',@Now,@Emp,@Now,@Emp,0),

-- ana — 05-may — AM — ACUDEN: 10:00-12:30 (continua tras Banco Central 08:00-10:00)
(@Ana,'2026-05-05','AM','10:00','12:30','ACUDEN','Desarrollo ACUDEN Digital CIMA','Hibrido','Desarrolladores','Pruebas de integracion modulo beneficiarios CIMA. Validacion de flujos.','Oficina KPG',@Now,@Ana,@Now,@Ana,0),

-- ana — 08-may — PM — Mapfre: 15:30-17:30 (continua tras Banco Central 13:30-15:30)
(@Ana,'2026-05-08','PM','15:30','17:30','Mapfre','Portal de Agentes Digitales','Hibrido','Desarrolladores','Revision de prototipos UI componentes portal agentes Mapfre.','Oficina KPG',@Now,@Ana,@Now,@Ana,0),

-- carlos — 06-may — AM — ACUDEN: 10:30-13:00 (continua tras Citi 08:30-10:30)
(@Car,'2026-05-06','AM','10:30','13:00','ACUDEN','Desarrollo ACUDEN Digital CIMA','Remoto','Desarrolladores','Apoyo tecnico modulo validacion de identidad CIMA. Revision de requerimientos.','Casa',@Now,@Car,@Now,@Car,0),

-- carlos — 08-may — AM — Banco Central: 11:00-13:00 (continua tras Citi 08:30-11:00)
(@Car,'2026-05-08','AM','11:00','13:00','Banco Central','Modernizacion Core Bancario','Remoto','Analistas','Levantamiento de requerimientos modulo de reportes regulatorios Banco Central.','Casa',@Now,@Car,@Now,@Car,0),

-- supervisor — 06-may — AM — KPG Interno: 10:30-13:00 (continua tras ACUDEN 08:00-10:30)
(@Sup,'2026-05-06','AM','10:30','13:00','KPG Interno','Gestion Interna KPG','Remoto','Supervisores','Preparacion propuesta comercial nuevo cliente. Estimacion de esfuerzo y recursos.','Casa',@Now,@Sup,@Now,@Sup,0),

-- supervisor — 08-may — PM — Banco Central: 16:00-18:00 (continua tras ACUDEN 14:00-16:00)
(@Sup,'2026-05-08','PM','16:00','18:00','Banco Central','Modernizacion Core Bancario','Remoto','Supervisores','Reunion seguimiento Banco Central. Revision de entregables y proximos hitos.','Casa',@Now,@Sup,@Now,@Sup,0);

SELECT CAST(@@ROWCOUNT AS VARCHAR) + ' nuevos registros multi-cliente (sin solapamiento) insertados.';

-- Verificacion: mostrar los dias con multiples clientes y sus horas
SELECT u.Email,
       r.FechaRegistro,
       r.Turno,
       r.HoraEntrada,
       r.HoraSalida,
       r.Cliente,
       r.Proyecto
FROM RegistrosHoras r
JOIN AspNetUsers u ON r.UserId = u.Id
WHERE r.FechaRegistro BETWEEN '2026-05-04' AND '2026-05-09'
  AND u.Email IN ('empleado@kpg.com','ana.garcia@kpg.com','carlos.ruiz@kpg.com','supervisor@kpg.com')
  AND r.FechaRegistro IN (
      SELECT r2.FechaRegistro FROM RegistrosHoras r2
      WHERE r2.UserId = r.UserId AND r2.Turno = r.Turno
      GROUP BY r2.FechaRegistro HAVING COUNT(*) > 1
  )
ORDER BY u.Email, r.FechaRegistro, r.Turno, r.HoraEntrada;
