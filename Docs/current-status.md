# Current Status - KPG Timesheet

Ultima actualizacion: 2026-05-19 — fin sesion 11

## Estado actual

**Epicas 1-5 completamente implementadas y con QA funcional aprobado por Jonathan.**

- Epic 1 (Auth/Shell): stories 1.1-1.6 en `done`.
- Epic 2 (Registro de Horas): stories 2.1-2.7 en `done`.
- Epic 3 (Ventana Retroactiva / Excepciones / Inmutabilidad): stories 3.1-3.6 en `review`.
- Epic 4 (Administracion de Catalogos): stories 4.1-4.6 en `review`.
- Epic 5 (Dashboards / Reportes / Notificaciones): stories 5.1-5.11 en `review`. QA funcional aprobado 2026-05-19.

## Punto exacto para retomar

**Proxima tarea: Iniciar la Epic 6 — Auditoria, Trazabilidad y Preparacion de Go-Live.**

No existen story files para Epic 6 todavia. Crearlos a partir de:
`_bmad-output/planning-artifacts/epics.md` (buscar "Epic 6", linea ~1191)

Historias a implementar:

| Historia | Descripcion |
|----------|-------------|
| 6.1 | Registrar Eventos Sensibles en Bitacora |
| 6.2 | Consultar Bitacora como Admin |
| 6.3 | Consultar Bitacora por Supervisor y Gerente |
| 6.4 | Exportar Bitacora para Auditoria |
| 6.5 | Logging, Backups y Recuperacion Operativa |
| 6.6 | Checklist de Go-Live, QA y UAT |
| 6.7 | Politica de Idioma y Preparacion para Localizacion |

## Lo ultimo implementado (sesion 11 — commit 8fb2c1a MODULO REPORTES)

**Nuevo endpoint:** `GET /api/reportes/timesheet/excel?userId=&mes=&anio=`
- Genera Excel con formato oficial KPG para un empleado y mes
- Usa ClosedXML (no MiniExcel — tiene formato avanzado con estilos)
- Solo accesible para Supervisor/Gerente/Admin
- Handler: `Backend/src/Infrastructure/Reportes/ExportarTimesheetQueryHandler.cs`
- Query: `Application/Features/Reportes/Queries/ExportarTimesheet/ExportarTimesheetQuery.cs`

**Frontend:** Seccion "Timesheet individual" en `/reportes` (`ReportesPage.razor`)
- Selector empleado + mes + anio + boton "Descargar Timesheet" con spinner

**CSS fix (`wwwroot/css/app.css`):** DatePicker popover no se salia del viewport. Domingos deshabilitados con opacity 0.3.

## Estado de compilacion y tests

```powershell
dotnet test Backend\KPG.Timesheet.sln --no-restore
dotnet build Backend\KPG.Timesheet.sln --no-restore
dotnet build Fronted\KPG.Timesheet.WebUI.sln --no-restore
```

Resultado al 2026-05-19:
- Backend tests: **187/187 pasan** (62 domain + 21 application + 104 integration).
- Backend build: 0 errores, 0 warnings.
- Frontend build: 0 errores, 0 warnings.
- Ultimo commit: `8fb2c1a MODULO REPORTES`

## Usuarios de prueba

| Email | Password | Rol |
|-------|----------|-----|
| admin@kpg.com | Admin1234! | Admin |
| supervisor@kpg.com | Supervisor1234! | Supervisor |
| empleado@kpg.com | Empleado1234! | Empleado |
| ana.garcia@kpg.com | Empleado1234! | Empleado |
| carlos.ruiz@kpg.com | Empleado1234! | Empleado |

## Puertos de desarrollo

| App | URL |
|-----|-----|
| Frontend Blazor WASM | http://localhost:5200 / https://localhost:5201 |
| Backend API | https://localhost:7035 |

## Comandos para arrancar

```powershell
# Backend
dotnet run --project Backend\src\Api\KPG.Timesheet.Api.csproj --launch-profile https

# Frontend
dotnet run --project Fronted\src\WebUI\KPG.Timesheet.WebUI.csproj --launch-profile https
```

## Documentos importantes

- Epics: `_bmad-output/planning-artifacts/epics.md`
- PRD: `_bmad-output/planning-artifacts/prd.md`
- Arquitectura: `_bmad-output/planning-artifacts/architecture.md`
- UX: `_bmad-output/planning-artifacts/ux-design-specification.md`
- Handoff anterior (mayo 14): `Docs/handoff/handoff-2026-05-14.md`
- Handoff actual (mayo 19): `Docs/handoff/handoff-2026-05-19.md`
