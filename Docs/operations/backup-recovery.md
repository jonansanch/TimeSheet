# Backup y Recuperación — KPG Timesheet

## Resumen

| Parámetro | Valor |
|-----------|-------|
| Base de datos | `TimesheetDb` en SQL Server |
| Tipo de backup | Full Database Backup |
| Frecuencia | Diaria (SQL Server Agent, 23:00 hs) |
| Retención | 30 días |
| Destino | `D:\Backups\Timesheet\` (ajustar según servidor) |
| RTO objetivo | ≤ 4 horas |
| RPO objetivo | ≤ 24 horas (último backup diario) |

---

## 1. Script SQL Server Agent — Backup Diario

Crear un **Job** en SQL Server Agent con el siguiente paso T-SQL.
Nombre sugerido del job: `KPG_Timesheet_DailyBackup`

```sql
-- ============================================================
-- KPG Timesheet — Backup diario completo
-- SQL Server Agent Job Step 1
-- ============================================================
DECLARE @BackupPath NVARCHAR(500);
DECLARE @FileName   NVARCHAR(500);
DECLARE @Timestamp  NVARCHAR(20);

SET @Timestamp  = CONVERT(NVARCHAR(20), GETDATE(), 112)          -- YYYYMMDD
               + '_'
               + REPLACE(CONVERT(NVARCHAR(8), GETDATE(), 108), ':', ''); -- HHmmss

SET @BackupPath = N'D:\Backups\Timesheet\';
SET @FileName   = @BackupPath + N'TimesheetDb_' + @Timestamp + N'.bak';

-- Backup completo con compresión
BACKUP DATABASE [TimesheetDb]
TO  DISK = @FileName
WITH
    COMPRESSION,
    STATS = 10,
    NAME  = N'KPG Timesheet — Backup diario ' + @Timestamp;

-- Verificación del backup
RESTORE VERIFYONLY
FROM DISK = @FileName
WITH NOUNLOAD, NOREWIND;

-- Eliminar backups con más de 30 días
DECLARE @CutoffDate DATETIME = DATEADD(DAY, -30, GETDATE());
DECLARE @DeleteCmd  NVARCHAR(500);

SELECT @DeleteCmd = N'DEL /Q "' + physical_device_name + '"'
FROM   msdb.dbo.backupset bs
JOIN   msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
WHERE  bs.database_name    = N'TimesheetDb'
  AND  bs.backup_finish_date < @CutoffDate
  AND  bmf.physical_device_name LIKE @BackupPath + N'%';

IF @DeleteCmd IS NOT NULL
    EXEC xp_cmdshell @DeleteCmd;
```

### Programación del Job

- **Frecuencia**: Diaria
- **Hora de inicio**: 23:00:00
- **Notificación en fallo**: Operador SQL Server (configurar según entorno)

---

## 2. Procedimiento de Recuperación (RTO ≤ 4 horas)

### Paso 1 — Identificar el último backup válido (5 min)

```sql
SELECT TOP 10
    bs.backup_finish_date,
    bmf.physical_device_name,
    bs.backup_size / 1024 / 1024 AS SizeMB
FROM   msdb.dbo.backupset bs
JOIN   msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
WHERE  bs.database_name = N'TimesheetDb'
  AND  bs.type          = N'D'  -- Full backup
ORDER  BY bs.backup_finish_date DESC;
```

Anotar el `physical_device_name` del backup más reciente.

### Paso 2 — Poner la base de datos en modo SINGLE_USER (2 min)

```sql
ALTER DATABASE [TimesheetDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
```

### Paso 3 — Restaurar el backup (30–60 min según tamaño)

```sql
RESTORE DATABASE [TimesheetDb]
FROM  DISK = N'D:\Backups\Timesheet\TimesheetDb_YYYYMMDD_HHmmss.bak'
WITH
    REPLACE,
    STATS = 10,
    RECOVERY;
```

Reemplazar el nombre del archivo por el identificado en el Paso 1.

### Paso 4 — Verificar integridad (10 min)

```sql
ALTER DATABASE [TimesheetDb] SET MULTI_USER;

DBCC CHECKDB ([TimesheetDb]) WITH NO_INFOMSGS;
```

### Paso 5 — Reiniciar la aplicación (5 min)

1. Reiniciar el pool de aplicación IIS o el servicio Windows de KPG Timesheet.
2. Verificar que la API responde en `https://timesheet.kpg.com/api/health` (si existe) o con login en la UI.
3. Confirmar con el equipo KPG que los datos son coherentes con el RPO esperado.

### Paso 6 — Comunicar resolución

Notificar al responsable técnico KPG con:
- Hora de inicio del incidente
- Backup utilizado (fecha/hora del backup restaurado)
- Hora de resolución
- Datos potencialmente perdidos (entre el backup restaurado y el incidente)

---

## 3. Monitoreo de Backups

Ejecutar semanalmente para verificar que los backups se completan:

```sql
SELECT
    bs.database_name,
    bs.backup_finish_date,
    bs.backup_size / 1024 / 1024 AS SizeMB,
    CASE bs.type WHEN 'D' THEN 'Full' WHEN 'L' THEN 'Log' ELSE bs.type END AS Tipo,
    bmf.physical_device_name
FROM   msdb.dbo.backupset bs
JOIN   msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
WHERE  bs.database_name    = N'TimesheetDb'
  AND  bs.backup_finish_date > DATEADD(DAY, -7, GETDATE())
ORDER  BY bs.backup_finish_date DESC;
```

Si no aparecen registros de los últimos 2 días, verificar el SQL Server Agent y el espacio en disco.

---

## 4. Consideraciones de Espacio

- Un backup completo de TimesheetDb (estimado ~1 GB en estado maduro) con compresión ocupa ~200–400 MB.
- 30 backups × 400 MB = ~12 GB. Asignar al menos **20 GB** a la carpeta de backups.
- Monitorear espacio en disco semanalmente.
