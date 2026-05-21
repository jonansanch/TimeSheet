# Plan de Hiper Care — Semana 1 Post Go-Live — KPG Timesheet

**Versión:** 1.0  
**Fecha de inicio de hiper care:** ___________________  
**Fecha de fin de hiper care:** ___________________ (7 días corridos desde go-live)  
**Responsable técnico:** ___________________  
**Canal de reporte de incidentes:** ___________________  

---

## Qué es el Hiper Care

El hiper care es el período de soporte activo intensificado inmediatamente después del go-live. Su propósito es:

- Detectar y resolver problemas antes de que impacten en la operación de KPG
- Acompañar a los usuarios en el uso inicial del sistema
- Confirmar que los flujos críticos funcionan en condiciones reales de uso

La primera semana post go-live está identificada en el PRD de KPG Timesheet como el período de mayor riesgo de adopción y mayor probabilidad de incidentes.

---

## Canal de Reporte de Incidentes

| Canal | Detalle | Disponibilidad |
|-------|---------|----------------|
| Teléfono / WhatsApp | ___________________ | Lun–Vie 7am–8pm |
| Correo | ___________________ | 24hs (respuesta en horario laboral) |
| Presencial / Teams | ___________________ | Según disponibilidad |

**Escalar a:** ___________________ si el responsable técnico no responde en 30 minutos (incidente P1).

---

## Actividades Diarias de Monitoreo (Lun–Vie, Semana 1)

### Cada mañana (07:15–07:30)

- [ ] Verificar que el servicio/IIS está corriendo (login de prueba rápido)
- [ ] Verificar que el log del día anterior se generó: `logs/kpg-timesheet-YYYYMMDD.log`
- [ ] Verificar que el backup nocturno se ejecutó:
  ```sql
  SELECT TOP 3 bs.backup_finish_date, bmf.physical_device_name,
         bs.backup_size/1024/1024 AS SizeMB
  FROM   msdb.dbo.backupset bs
  JOIN   msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
  WHERE  bs.database_name = N'TimesheetDb'
  ORDER  BY bs.backup_finish_date DESC;
  ```
- [ ] Verificar espacio en disco del servidor (mínimo 5 GB libres)
- [ ] Si algún check falla: registrar en la tabla de incidentes y escalar según severidad

### Al mediodía (12:30–13:00)

- [ ] Revisar si llegaron reportes de problemas de usuarios
- [ ] Si hay incidentes P2 abiertos: verificar estado de resolución
- [ ] Confirmar que los registros de la mañana se están procesando correctamente (revisar `/admin/bitacora`)

### Al cierre del día (18:00–18:30)

- [ ] Contar cuántos usuarios registraron al menos una vez en el día (Dashboard del supervisor)
- [ ] Si hay usuarios que NO registraron: notificar a su supervisor para seguimiento
- [ ] Revisar errores en los logs del día:
  ```
  Buscar: [ERR] en logs/kpg-timesheet-YYYYMMDD.log
  ```
- [ ] Registrar el estado del día en la tabla de seguimiento (abajo)

---

## Tabla de Registro de Incidentes — Semana 1

| Día | Hora | Descripción | Severidad | Reportado por | Estado | Resolución |
|-----|------|-------------|-----------|---------------|--------|------------|
| Día 1 | | | | | | |
| Día 1 | | | | | | |
| Día 2 | | | | | | |
| Día 2 | | | | | | |
| Día 3 | | | | | | |
| Día 4 | | | | | | |
| Día 5 | | | | | | |

---

## Tabla de Seguimiento Diario

| Día | Fecha | Usuarios activos | Registros creados | Errores en logs | Backup OK | Incidentes |
|-----|-------|-----------------|-------------------|-----------------|-----------|------------|
| 1 (Lun) | | / total | | | [ ] | |
| 2 (Mar) | | / total | | | [ ] | |
| 3 (Mié) | | / total | | | [ ] | |
| 4 (Jue) | | / total | | | [ ] | |
| 5 (Vie) | | / total | | | [ ] | |

---

## Criterios de Escala de Incidentes

| Severidad | Descripción | Tiempo de respuesta | Acción |
|-----------|-------------|---------------------|--------|
| **P1 — Crítico** | Sistema completamente inaccesible o datos corruptos | 30 min | Escalar de inmediato. Evaluar rollback según `backup-recovery.md` |
| **P2 — Alto** | Función crítica degradada (no se puede registrar, no se puede exportar) | 1 hora | Investigar causa raíz. Parche en caliente si es posible. |
| **P3 — Medio** | Comportamiento inesperado pero hay workaround disponible | 4 horas | Registrar, priorizar para próximo deployment |
| **P4 — Bajo** | Inconveniente menor, solo cosmético | 1 día hábil | Registrar para backlog |

---

## Criterios de Rollback

Considerar rollback (ver procedimiento en `backup-recovery.md`) si:

- Sistema completamente inaccesible por > 1 hora en horario laboral (P1 sin resolución)
- Se detecta corrupción de datos ingresados por usuarios
- Vulnerabilidad de seguridad crítica sin parche inmediato disponible

**Restaurar desde:** backup `TimesheetDb_PREDEPLOYMENT.bak` (guardado en el go-live)

---

## Preguntas Frecuentes de Usuarios — Semana 1

**"No puedo entrar al sistema"**
→ Verificar URL correcta. Verificar que usan el navegador correcto (Chrome/Edge/Firefox). Si el problema persiste, el Admin puede verificar el estado del servicio.

**"Mis registros del día anterior desaparecieron"**
→ Los registros están guardados. Ir a "Mis Registros" y verificar filtros de fecha. Si el problema persiste, reportar al responsable técnico con capturas.

**"El sistema carga muy lento"**
→ La primera carga de Blazor WASM puede tomar 5–10 segundos — es normal. Las cargas posteriores son más rápidas. Si la lentitud es generalizada y persistente, verificar el servidor.

**"No veo el botón de X"**
→ Verificar que el rol del usuario es el correcto. Los menús y funciones dependen del rol asignado por el Admin. Contactar al Admin si el rol parece incorrecto.

**"Quiero registrar un día de la semana pasada"**
→ La ventana de retroactividad es 3 días hábiles. Si necesita más días atrás, debe solicitar una excepción al Admin desde la pantalla de Registro.

---

## Cierre del Hiper Care

El hiper care se cierra cuando se cumplen **todos** los siguientes criterios:

- [ ] **≥ 3 días hábiles consecutivos** sin incidentes de severidad P1 o P2
- [ ] **≥ 80% de los empleados** registraron al menos una jornada completa en el sistema
- [ ] **Todos los supervisores** generaron al menos un reporte (aunque sea de prueba)
- [ ] El Admin completó la alta de todos los empleados activos de KPG
- [ ] No hay defectos críticos conocidos sin resolución

**Fecha de cierre oficial:** ___________________  
**Firma del responsable técnico:** ___________________  
**Firma del Product Owner:** ___________________  

---

## Transición a Operación Normal

Tras el cierre del hiper care, el sistema pasa a operación normal con:

- Monitoreo semanal de backups (ver `backup-recovery.md`, sección 3)
- Revisión de logs solo ante incidentes reportados
- Soporte ante incidentes conforme al proceso en `availability-sla.md`
- Backlog de mejoras según feedback de la semana 1

> Ver también: [`golive-checklist.md`](golive-checklist.md) — checklist de go-live  
> Ver también: [`availability-sla.md`](availability-sla.md) — SLA y proceso de incidentes en operación normal  
> Ver también: [`backup-recovery.md`](backup-recovery.md) — procedimiento de rollback y recuperación
