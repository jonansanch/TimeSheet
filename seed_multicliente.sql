DECLARE @Emp  NVARCHAR(450) = '987b1ff3-b60e-49f7-9e75-bfd9b4258e8b';
DECLARE @Ana  NVARCHAR(450) = '33DE0281-1706-4ED6-92B5-53B2E8A98B82';
DECLARE @Car  NVARCHAR(450) = 'DBC3F75F-2618-42B1-9751-645414538922';
DECLARE @Sup  NVARCHAR(450) = '912b1018-7ace-41e5-a027-5498d5ff5b25';
DECLARE @Now  DATETIMEOFFSET = GETUTCDATE();

-- ══════════════════════════════════════════════════════════════════
-- MISMO USUARIO · MISMO DIA · MISMO TURNO · MISMAS HORAS
-- Diferente cliente y proyecto
-- ══════════════════════════════════════════════════════════════════

INSERT INTO RegistrosHoras
(UserId,FechaRegistro,Turno,HoraEntrada,HoraSalida,Cliente,Proyecto,Modalidad,Recurso,Descripcion,Lugar,Created,CreatedBy,LastModified,LastModifiedBy,EsRetroactivo)
VALUES
-- empleado — 5 mayo — AM — ya tiene ACUDEN, agrego Banco Central
(@Emp,'2026-05-05','AM','08:00','13:00','Banco Central','Soporte Tecnico Core','Remoto','Desarrolladores','Soporte tecnico incidente critico en modulo de transferencias. Hotfix aplicado.','Casa',@Now,@Emp,@Now,@Emp,0),
-- empleado — 5 mayo — PM — ya tiene ACUDEN, agrego Citi
(@Emp,'2026-05-05','PM','14:00','18:00','Citi','Analisis de Riesgos Crediticios','Remoto','Desarrolladores','Apoyo puntual en revision de reportes de riesgo para equipo Citi.','Casa',@Now,@Emp,@Now,@Emp,0),

-- empleado — 7 mayo — AM — ya tiene ACUDEN, agrego Mapfre
(@Emp,'2026-05-07','AM','08:00','13:00','Mapfre','Portal de Agentes Digitales','Remoto','Desarrolladores','Revision de codigo y apoyo en integracion API cotizaciones Mapfre.','Casa',@Now,@Emp,@Now,@Emp,0),

-- ana — 5 mayo — AM — ya tiene Banco Central, agrego ACUDEN
(@Ana,'2026-05-05','AM','08:00','12:30','ACUDEN','Desarrollo ACUDEN Digital CIMA','Hibrido','Desarrolladores','Apoyo puntual en pruebas de integracion modulo beneficiarios CIMA.','Oficina KPG',@Now,@Ana,@Now,@Ana,0),
-- ana — 8 mayo — PM — ya tiene Banco Central, agrego Mapfre
(@Ana,'2026-05-08','PM','13:30','17:30','Mapfre','Portal de Agentes Digitales','Hibrido','Desarrolladores','Consultoria diseno UI componentes portal agentes. Revision de prototipos.','Oficina KPG',@Now,@Ana,@Now,@Ana,0),

-- carlos — 6 mayo — AM — ya tiene Citi, agrego ACUDEN
(@Car,'2026-05-06','AM','08:30','13:00','ACUDEN','Desarrollo ACUDEN Digital CIMA','Remoto','Desarrolladores','Apoyo tecnico en modulo de validacion de identidad. Revision requerimientos.','Casa',@Now,@Car,@Now,@Car,0),
-- carlos — 8 mayo — AM — ya tiene Citi, agrego Banco Central
(@Car,'2026-05-08','AM','08:30','13:00','Banco Central','Modernizacion Core Bancario','Remoto','Analistas','Levantamiento de requerimientos para nuevo modulo de reportes regulatorios.','Casa',@Now,@Car,@Now,@Car,0),

-- supervisor — 6 mayo — AM — ya tiene ACUDEN, agrego KPG Interno
(@Sup,'2026-05-06','AM','08:00','13:00','KPG Interno','Gestion Interna KPG','Remoto','Supervisores','Preparacion propuesta comercial nuevo cliente. Estimacion de esfuerzo.','Casa',@Now,@Sup,@Now,@Sup,0),
-- supervisor — 8 mayo — PM — ya tiene ACUDEN, agrego Banco Central
(@Sup,'2026-05-08','PM','14:00','18:00','Banco Central','Modernizacion Core Bancario','Remoto','Supervisores','Reunion de seguimiento con cliente Banco Central. Revision entregables.','Casa',@Now,@Sup,@Now,@Sup,0);

SELECT CAST(@@ROWCOUNT AS VARCHAR) + ' registros multi-cliente insertados.';

-- Verificacion: ver dias con multiples entradas por turno
SELECT u.Email,
       r.FechaRegistro,
       r.Turno,
       COUNT(*) AS CantidadRegistros,
       STRING_AGG(r.Cliente, ' | ') WITHIN GROUP (ORDER BY r.Cliente) AS Clientes
FROM RegistrosHoras r
JOIN AspNetUsers u ON r.UserId = u.Id
WHERE r.FechaRegistro BETWEEN '2026-05-04' AND '2026-05-09'
  AND u.Email IN ('empleado@kpg.com','ana.garcia@kpg.com','carlos.ruiz@kpg.com','supervisor@kpg.com')
GROUP BY u.Email, r.FechaRegistro, r.Turno
HAVING COUNT(*) > 1
ORDER BY u.Email, r.FechaRegistro, r.Turno;
