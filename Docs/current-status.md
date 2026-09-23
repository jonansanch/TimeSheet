# Current Status - KPG Timesheet

Ultima actualizacion: 2026-09-22 — verificado contra el codigo en `4cdb33f`

## Estado actual

**Epicas 1-7 completas, GO-LIVE hecho (2026-06-02), y la ronda de ~45 ajustes post-UAT
tambien esta cerrada.** En produccion.

| Fase | Contenido | Estado |
|------|-----------|--------|
| **1A** | Horario 1/2/3 reemplaza AM/PM (+ tercer bloque) | ✅ |
| **1B** | Calendario completo/incompleto, cuartos de hora, formato numerico, modalidades | ✅ |
| **1C** | Filtro mes actual, 50 filas, tooltips, verde/gris, nombre obligatorio, nombre en login | ✅ |
| **2** | Registros por rango de dias, solicitud de excepcion con registro completo | ✅ |
| **3** | Voz por IA, limpiar al grabar, sugerencias de texto | ✅ |
| **4** | Estructura organizacional (supervisores, organigrama) | ✅ |
| **5** | Aprobaciones en cascada (3 niveles) | ✅ |
| **6** | Exportacion e importacion | ✅ |

**Lo que queda no es desarrollo, es puesta en marcha.** Ver "Pendiente del cliente".

### Evidencia de cada fase (verificada el 2026-09-22)

| Fase | Donde vive |
|------|-----------|
| 1A | `HoraEntrada3`/`HoraSalida3` en `Domain/Entities/RegistroHoras.cs` |
| 1B | `GET /api/registros-horas/resumen-mensual` · `Shared/Utils/KpgFormat.cs` |
| 1C | `Shared/Components/KpgEstadoChip.razor` · claim `Name` en `JwtTokenService.cs` |
| 2 | `POST /api/registros-horas/rango` · `SolicitudExcepcion` carga el registro completo |
| 3 | `Infrastructure/Voz/ClaudeInterpreteVoz.cs` detras de `IInterpreteVoz` |
| 4 | `ApplicationUser.SupervisorUserId`/`PuestoId` · CRUD `/api/supervisores-puesto` |
| 5 | 8 endpoints en `Api/Endpoints/Aprobaciones.cs` (incluye revertir y reenviar) |
| 6 | 4 endpoints de export en `Api/Endpoints/Reportes.cs` · `Reportes/TimesheetImportParser.cs` |

## Pendiente del cliente (no requiere codigo)

1. **Cerrar julio y agosto.** ~139 registros siguen en Pendiente; al primer supervisor que
   abra `/aprobaciones` le cae toda esa cola. Usar `Docs/operations/cierre-mensual.sql`,
   un mes por corrida (el bloque 0 lista que hay antes de tocar nada).
2. **`Anthropic__ApiKey`** en Azure si se quiere la voz por IA. Sin ella todo funciona igual
   con el parser de reglas del navegador — no rompe nada.
3. **Probar a mano lo que ningun test cubre:** el reporte de horas, el Excel del timesheet
   contra la plantilla real del cliente, y el PDF con un mes de datos.
4. **Cargar el organigrama real de KPG** cuando exista, con
   `Docs/operations/estructura-inicial-produccion.sql`. Lo que hay hoy en Azure es data de
   demo (Laura, Miguel, Juan y clientes ficticios).

## Decisiones abiertas

- **No hay override de Admin en las aprobaciones.** Un nivel sin aprobador asignado no lo
  aprueba nadie. Si el cliente lo pide (p. ej. supervisor de vacaciones), hay que decidirlo.
- **Un tramo horario guardado es inmutable.** Corregir un `08:00` mal digitado obliga a
  borrar el registro y rehacerlo. Se podria permitir editarlo mientras este Pendiente.

## Estado de compilacion y tests

```powershell
dotnet test  Backend\KPG.Timesheet.sln
dotnet build Backend\KPG.Timesheet.sln
dotnet build Fronted\KPG.Timesheet.WebUI.sln
```

Resultado al 2026-09-22 sobre `4cdb33f`:

- Backend tests: **461/461 pasan** (125 Domain + 31 Application + 305 Integration).
- Frontend build: 0 errores, 0 advertencias.

> Las versiones anteriores de este documento decian "287/287" y marcaban las fases 2, 3, 5
> y 6 como pendientes. Estaban tres commits atrasadas.

### Lo que el verde NO cubre

El SQL crudo de `AprobacionesRepository`, `DashboardRepository` y `ReportesRepository` no
esta cubierto por tests: el harness stubea `IDbConnection`, asi que esas consultas solo se
validan contra una base real. Es justo donde vivia el reporte de horas que estaba roto.

⚠️ **1 fallo en 23 corridas, no reproducible** (2026-09-20). Despues de eso, verde
sostenido. Si reaparece, perseguirlo en serio.

## Lo ultimo implementado

### `d6a0fea` — Ajustes de retroactividad y puntos faltantes (2026-09-21)

| Cambio | Donde |
|--------|-------|
| Ventana de retroactividad con reglas por persona y por rol | `/api/sistema/ventana-retroactividad` · `/api/reglas-ventana` |
| **Restricciones de dia**: dias de la semana en que una persona o un rol no puede registrar | `ParametroRestriccionDia` · `/api/restricciones-dia` |
| **Reportes de usuario**: el colaborador reporta una falla o una mejora desde `/reportar`; Admin las gestiona en `/admin/reportes-usuario` | `ReporteUsuario`, `TipoReporte` (Falla/Mejora), `EstadoReporte` (Nuevo/EnRevision/Resuelto/Rechazado) |

Las restricciones de dia se acumulan: puede haber una por rol y otra por persona para el
mismo dia y **cualquiera que coincida bloquea** — a diferencia de la ventana retroactiva,
donde una regla gana sobre otra. La excepcion aprobada sigue siendo la valvula de escape.

### `4cdb33f` — Estilos y ajustes UI/UX (2026-09-21)

- **Logo de reportes configurable** desde `/admin/parametros/configuracion`: se guarda como
  data URI en `ParametrosSistema` y lo consumen el Excel y el PDF (`LogoDataUri.cs`).
- Pasada de estilos sobre los 6 catalogos de admin y los dialogos.

## Advertencias operativas

1. **Las migraciones EF no se aplican en este proyecto.** El esquema lo mantiene el DDL
   idempotente de `ApplicationDbContextInitialiser.EnsureTimesheetTablesAsync()`, que solo
   corre si `ASPNETCORE_ENVIRONMENT=Development` o `RunDatabaseInitialiser=true`
   (ver `Program.cs`). Verificar que este habilitado en el primer arranque tras desplegar.
2. **`Jwt:Key` esta vacia en `appsettings.json`** a proposito (la inyecta Azure). Los tests
   aportan la suya por variable de entorno desde `KpgWebApplicationFactory`.
3. **El catalogo "Empleados" contiene PUESTOS, no personas** (`Consultor`, `Analista`, ...).
   Las personas viven solo en `AspNetUsers`.
4. **Nunca usar `dotnet build -t:Compile`** en este repo: deja artefactos inconsistentes.
   Usar `dotnet build` o `-t:Rebuild`.

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

- **Contexto de producto (para trabajo de diseño):** `PRODUCT.md` en la raiz
- Handoff mas reciente: `Docs/handoff/handoff-2026-09-16.md`
- Epics: `_bmad-output/planning-artifacts/epics.md`
- PRD: `_bmad-output/planning-artifacts/prd.md`
- Arquitectura: `_bmad-output/planning-artifacts/architecture.md`
- Manuales: `Docs/manuals/manual-tecnico.md`, `manual-administrador.md`, `manual-usuario.md`
- Operaciones: `Docs/operations/` (go-live, QA, UAT, hypercare, SQL de cierre mensual)
