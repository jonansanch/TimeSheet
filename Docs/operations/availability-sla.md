# Disponibilidad y SLA — KPG Timesheet

## Resumen

| Parámetro | Valor |
|-----------|-------|
| Horario laboral | Lunes a Viernes, 07:00–20:00 (hora Argentina, UTC-3) |
| Meta de disponibilidad | ≥ 99 % en horario laboral |
| Tiempo máximo de inactividad mensual | ≤ 7,8 horas/mes (cálculo abajo) |
| RTO (Recovery Time Objective) | ≤ 4 horas |
| RPO (Recovery Point Objective) | ≤ 24 horas (último backup diario) |

---

## 1. Definición de Horario Laboral

El SLA aplica **exclusivamente durante el horario laboral**:

- **Días**: Lunes a Viernes
- **Horario**: 07:00 a 20:00 (hora Argentina, UTC-3)
- **Excluido del SLA**: fines de semana, feriados nacionales argentinos y fuera del horario laboral

Las tareas de mantenimiento planificado (actualizaciones, backups, migraciones) deben ejecutarse preferentemente **fuera del horario laboral**, salvo acuerdo explícito con KPG.

---

## 2. Cálculo de Disponibilidad

### Horas laborales mensuales

```
Días laborales por mes (promedio):  21,7 días
Horas por día laboral:              13 horas (07:00–20:00)
Total horas laborales/mes:          21,7 × 13 = 282,1 horas/mes
```

### Tiempo máximo de inactividad con 99 % de disponibilidad

```
Inactividad máxima = 282,1 × (1 − 0,99) = 2,821 horas/mes ≈ 2 h 49 min/mes
```

### Tabla de referencia

| Disponibilidad | Inactividad mensual permitida |
|---------------|-------------------------------|
| 99,5 %        | ≤ 1 h 25 min/mes              |
| **99,0 %**    | **≤ 2 h 49 min/mes** ← meta   |
| 98,5 %        | ≤ 4 h 14 min/mes              |
| 98,0 %        | ≤ 5 h 39 min/mes              |

---

## 3. Tipos de Incidente

| Severidad | Descripción | Tiempo de respuesta | Tiempo de resolución |
|-----------|-------------|---------------------|----------------------|
| P1 — Crítico | Sistema completamente inaccesible o datos corruptos | 30 min | ≤ 4 h |
| P2 — Alto | Funcionalidad crítica degradada (ej. no se puede cargar horas) | 1 h | ≤ 8 h |
| P3 — Medio | Funcionalidad secundaria afectada, workaround disponible | 4 h | ≤ 48 h |
| P4 — Bajo | Inconvenientes menores, sin impacto en operación | 1 día hábil | Próximo sprint |

---

## 4. Proceso de Notificación de Incidente

### Paso 1 — Detección (0–15 min)

- Monitoreo manual o reporte de usuario a través de los canales habituales de KPG
- Verificar en `https://timesheet.kpg.com/api/health` (si existe) o intentar login

### Paso 2 — Clasificación y escalada (15–30 min)

- Determinar severidad (P1–P4) según tabla anterior
- **P1/P2**: Notificar de inmediato al responsable técnico KPG vía canal urgente
- **P3/P4**: Registrar y notificar en el próximo reporte diario

### Paso 3 — Diagnóstico (30 min – 1 h)

1. Revisar logs en `logs/kpg-timesheet-YYYYMMDD.log` (servidor IIS o servicio Windows)
2. Verificar estado de SQL Server y espacio en disco
3. Verificar estado del pool de aplicación IIS / servicio Windows
4. Si se sospecha corrupción de datos: **NO reiniciar** hasta evaluar el estado de la base

### Paso 4 — Resolución

- Si el incidente requiere restauración de base de datos: seguir `backup-recovery.md`
- Si es falla de aplicación: reiniciar pool IIS o servicio Windows
- Si es falla de infraestructura: coordinar con el área IT de KPG

### Paso 5 — Comunicación de resolución

Notificar al responsable KPG con:

```
Incidente: [descripción breve]
Inicio: [fecha y hora]
Causa raíz: [descripción]
Resolución aplicada: [acciones tomadas]
Cierre: [fecha y hora]
Datos potencialmente afectados: [período sin cobertura de backup, si aplica]
Acciones preventivas: [cambios a implementar para evitar recurrencia]
```

---

## 5. Mantenimiento Planificado

El tiempo de mantenimiento planificado **no cuenta** como inactividad a los efectos del SLA, siempre que:

1. Se notifique a los usuarios con al menos **48 horas de anticipación**
2. Se ejecute **fuera del horario laboral** (preferentemente entre 22:00 y 06:00)
3. No exceda **2 horas por ventana de mantenimiento**

---

## 6. Métricas y Reporte

| Métrica | Fuente | Frecuencia de revisión |
|---------|--------|------------------------|
| Disponibilidad mensual | Logs Serilog + registro manual de incidentes | Mensual |
| Tiempo de respuesta de backups | `msdb.dbo.backupset` (ver `backup-recovery.md`) | Semanal |
| Espacio en disco (backups + logs) | Monitoreo manual | Semanal |

El responsable técnico KPG debe revisar los backups semanalmente usando la consulta de monitoreo en `backup-recovery.md` (sección 3).
