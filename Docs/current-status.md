# Current Status - KPG Timesheet

Ultima actualizacion: 2026-05-21 — fin sesion 12

## Estado actual

**Epicas 1-5 completamente implementadas y con QA funcional aprobado por Jonathan.**

- Epic 1 (Auth/Shell): stories 1.1-1.6 en `done`.
- Epic 2 (Registro de Horas): stories 2.1-2.7 en `done`.
- Epic 3 (Ventana Retroactiva / Excepciones / Inmutabilidad): stories 3.1-3.6 en `done`. QA funcional revisado y aprobado.
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

## Lo ultimo implementado (sesion 12 — refactor registro unico diario)

**Refactor mayor: modelo de registro de horas cambiado a registro unico diario con bloques AM/PM opcionales.**

- Antes: dos filas por dia por usuario (una AM y una PM), con campo `Turno` y enum `TurnoRegistro`.
- Ahora: un registro por dia por usuario con `HoraEntradaAM?/HoraSalidaAM?/HoraEntradaPM?/HoraSalidaPM?`; se requiere al menos un bloque.
- Eliminado `Backend/src/Domain/Enums/TurnoRegistro.cs` (enum ya no existe).
- `CreateRegistroHorasCommandHandler` ahora hace upsert: si ya existe registro del dia, agrega el bloque faltante via `SetBloqueAM`/`SetBloquePM`.
- Indice unico cambiado de `(UserId, FechaRegistro, Turno)` → `(UserId, FechaRegistro)`.
- `RegistroHorasImmutabilityInterceptor` actualizado: `SiemprePermitidos` incluye ahora los campos de metadata (Cliente, Proyecto, Modalidad, Recurso, Lugar) necesarios para el upsert; `SoloAgregables` cubre los 4 campos de tiempo AM/PM (null→value permitido, value→value prohibido).
- Frontend `KpgShiftForm.razor` reescrito: formulario unificado con secciones AM y PM opcionales, un solo boton "Guardar jornada".
- `KpgSaveConfirmationBanner` actualizado: parametros Turno/HoraEntrada/HoraSalida reemplazados por HoraEntradaAM?/HoraSalidaAM?/HoraEntradaPM?/HoraSalidaPM?.
- `HistorialPage` y `ReportesPage` actualizados con columnas AM/PM separadas.
- Seeder (`ApplicationDbContextInitialiser`) actualizado con nuevo esquema.
- 204/204 tests pasan.

**Sesion 11 — commit 8fb2c1a MODULO REPORTES (referencia anterior):**

- `GET /api/reportes/timesheet/excel?userId=&mes=&anio=` — Excel con ClosedXML para Supervisor/Gerente/Admin.
- Frontend: seccion "Timesheet individual" en `/reportes` con selector empleado + mes + anio.
- CSS fix: DatePicker popover y domingos deshabilitados.

## Estado de compilacion y tests

```powershell
dotnet test Backend\KPG.Timesheet.sln --no-restore
dotnet build Backend\KPG.Timesheet.sln --no-restore
dotnet build Fronted\KPG.Timesheet.WebUI.sln --no-restore
```

Resultado al 2026-05-21:
- Backend tests: **204/204 pasan** (68 domain + 24 application + 112 integration).
- Backend build: 0 errores, 0 warnings.
- Frontend build: 0 errores, 0 warnings.
- Ultimo commit: `54ef173 epic 6 implementation` + refactor sesion 12 (pendiente commit)

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
