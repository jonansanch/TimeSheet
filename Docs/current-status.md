# Current Status - KPG Timesheet

Ultima actualizacion: 2026-09-16 — ronda de ajustes post-UAT en curso

## Estado actual

**Epicas 1-7 completas y GO-LIVE hecho (2026-06-02).** En produccion.

Tras el UAT el cliente entrego un listado de ~45 ajustes, que se estan implementando
en **6 fases**. Prioridad acordada: *"que el registro funcione full full"*.

| Fase | Contenido | Estado |
|------|-----------|--------|
| **1A** | Horario 1/2/3 reemplaza AM/PM (+ tercer bloque) | ✅ |
| **1B** | Calendario completo/incompleto, cuartos de hora, formato numerico, modalidades | ✅ |
| **1C** | Filtro mes actual, 50 filas, tooltips, verde/gris, nombre obligatorio, nombre en login | ✅ |
| **2** | Registros por rango de dias, solicitud de excepcion con registro completo | pendiente |
| **3** | Voz por IA, limpiar al grabar, sugerencias de texto | pendiente |
| **4** | Estructura organizacional (supervisores, organigrama) | ✅ parcial |
| **5** | Aprobaciones en cascada (3 niveles) | pendiente |
| **6** | Exportacion e importacion | pendiente |

## Punto exacto para retomar

**Cerrar Fase 4** — quedan dos items:

1. Parametrizar dias de restriccion por rol o persona.
2. Migrar `RegistroHoras.Cliente`/`Proyecto` (strings) → `ProyectoId`, y autocompletar
   `Recurso` desde `AspNetUsers.PuestoId`.

Al hacer la migracion, cerrar los **huecos diferidos del autocomplete de registro**
(`CoerceValue` acepta texto libre · sin cliente se listan proyectos de todos los clientes ·
`ProyectosFallback` hardcodeado). Se difirieron porque ese control se rehace como selector
sobre IDs.

**El diagnostico de produccion dio 0 parejas invalidas sobre 141 registros**, asi que la
migracion podra emparejar el 100% automaticamente.

Despues: Fase 5 (aprobaciones) es la que mas depende de Fase 4. Fases 2, 3 y 6 son independientes.

## Estado de compilacion y tests

```powershell
dotnet test  Backend\KPG.Timesheet.sln
dotnet build Backend\KPG.Timesheet.sln
dotnet build Fronted\KPG.Timesheet.WebUI.sln
```

Resultado al 2026-09-16:
- Backend tests: **287/287 pasan** (75 Domain + 31 Application + 181 Integration).
- Backend build: 0 errores, 0 warnings.
- Frontend build: 0 errores, 0 warnings.

> El "231/231" de versiones anteriores de este documento estaba desactualizado: el proyecto
> de tests llevaba tiempo sin compilar por dos roturas introducidas en commits previos
> (`39fac2b` y `d8f3b48`), ambas corregidas el 2026-09-16.

## Lo ultimo implementado (2026-09-16)

Detalle completo en `Docs/handoff/handoff-2026-09-16.md`.

### Modelo de datos nuevo

| Cambio | Ubicacion |
|--------|-----------|
| `RegistrosHoras`: AM/PM → `HoraEntrada1/2/3` + `HoraSalida1/2/3` | migracion `sp_rename`, preserva datos |
| `AspNetUsers.SupervisorUserId` (FK auto-referencial) y `PuestoId` | organigrama + 1a/2a aprobacion |
| `Proyectos.SupervisorUserId` | 3a aprobacion |
| Tabla `SupervisoresPuesto` (`PuestoId`, `ClienteId` nullable, `SupervisorUserId`) | 1a aprobacion |
| `ParametrosSistema.HorasDiaCompleto` (default 8) | umbral de dia completo |

### Endpoints nuevos

- `GET /api/registros-horas/resumen-mensual` — minutos por dia + umbral, alimenta el calendario
- `PUT /api/users/{id}/estructura` — asigna jefe directo y puesto
- `GET /api/users/organigrama`
- `GET|POST|PUT /api/supervisores-puesto` + `/{id}/toggle`

### Pantallas nuevas

- `/organigrama` — arbol jerarquico
- `/admin/supervisores-puesto` — reglas de primer nivel

## Advertencias operativas

1. **Las migraciones EF no se aplican en este proyecto.** El esquema lo mantiene el DDL
   idempotente de `ApplicationDbContextInitialiser.EnsureTimesheetTablesAsync()`, que solo
   corre si `ASPNETCORE_ENVIRONMENT=Development` o `RunDatabaseInitialiser=true`
   (ver `Program.cs`). Verificar que este habilitado en el primer arranque tras desplegar.
2. **`Jwt:Key` esta vacia en `appsettings.json`** a proposito (la inyecta Azure). Los tests
   aportan la suya por variable de entorno desde `KpgWebApplicationFactory`.
3. **Ningun usuario tiene jefe ni puesto todavia.** La cadena de aprobacion resuelve vacia
   hasta que un admin los asigne desde `/admin/usuarios`.
4. **El catalogo "Empleados" contiene PUESTOS, no personas** (`Consultor`, `Analista`, ...).
   Las personas viven solo en `AspNetUsers`.

## Usuarios de prueba

| Email | Password | Rol |
|-------|----------|-----|
| admin@kpg.com | Admin1234! | Admin |
| gerente@kpg.com | Gerente1234! | Gerente |
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

- Handoff mas reciente: `Docs/handoff/handoff-2026-09-16.md`
- Diagnostico cliente/proyecto: `Docs/operations/diagnostico-parejas-cliente-proyecto.sql`
- Epics: `_bmad-output/planning-artifacts/epics.md`
- PRD: `_bmad-output/planning-artifacts/prd.md`
- Arquitectura: `_bmad-output/planning-artifacts/architecture.md`
- Manual tecnico: `Docs/manuals/manual-tecnico.md`
- Manual administrador: `Docs/manuals/manual-administrador.md`
- Manual usuario: `Docs/manuals/manual-usuario.md`
