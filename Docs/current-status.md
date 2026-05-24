# Current Status - KPG Timesheet

Ultima actualizacion: 2026-05-23 — fin sesion 18

## Estado actual

**Epicas 1-7 completamente implementadas. QA funcional en progreso (sesion 18).**

- Epic 1 (Auth/Shell): stories 1.1-1.6 — done.
- Epic 2 (Registro de Horas): stories 2.1-2.7 — done.
- Epic 3 (Ventana Retroactiva / Excepciones / Inmutabilidad): stories 3.1-3.6 — done.
- Epic 4 (Administracion de Catalogos): stories 4.1-4.6 — done.
- Epic 5 (Dashboards / Reportes / Notificaciones): stories 5.1-5.11 — done.
- Epic 6 (Bitacora, Logging, Go-Live, Localizacion): stories 6.1-6.7 — done.
- Epic 7 (Soporte multiidioma ES/EN): done.

## Punto exacto para retomar

**Proxima tarea: Continuar QA funcional con todos los roles.**

La sesion 18 corrigio bugs encontrados durante QA. Continuar probando con los usuarios de prueba listados abajo, en particular:
- Flujo completo de Gerente (dashboards, reportes, bitacora)
- Flujo completo de Supervisor (dashboard, notificaciones, bitacora)
- Solicitudes de excepcion (empleado solicita, admin aprueba/rechaza)

## Lo ultimo implementado (sesion 18 — QA bugs + optimizaciones)

### Optimizaciones de rendimiento (sesion 17-18)

| # | Cambio | Archivo principal |
|---|--------|------------------|
| #1 | Indices `IX_RegistrosHoras_FechaRegistro` e `IX_AspNetUsers_IsActive` | `ApplicationDbContextInitialiser.cs` |
| #2 | `GetRegistrosRecientes`: GROUP BY en SQL | `GetRegistrosRecientesQueryHandler.cs` |
| #3 | `GetMetricasGlobales`: CTE reemplaza 5 subqueries | `DashboardRepository.cs` |
| #4 | `IdentityService.GetUsersAsync`: 1 round-trip Dapper | `IdentityService.cs` |
| #5 | `SqlPendientes`: LEFT JOIN acotado 90 dias | `DashboardRepository.cs` |
| #6 | `GetCatalogoClientesConProyectos`: 1 LEFT JOIN | `GetCatalogoClientesConProyectosQueryHandler.cs` |
| #7 | `GetSolicitudesExcepcion`: filtro 180 dias | `GetSolicitudesExcepcionQueryHandler.cs` |
| #8 | Output cache 60s en endpoints de dashboard | `DependencyInjection.cs`, `Program.cs` |
| #9 | `Task.WhenAll` en RegistroPage | `RegistroPage.razor` |

### QA bugs corregidos (sesion 18)

- **`SetFecha` no actualizaba `_dateAvailability`**: registro fuera de ventana no se bloqueaba en frontend al seleccionar desde calendario.
- **Campo Descripcion sin MaxLength**: frontend no limitaba los 1000 chars permitidos por el backend.
- **`GET /api/users` bloqueado para Gerente**: endpoint y query record solo permitia Admin; corregido a Admin/Gerente/Supervisor (lectura).
- **"Mi Perfil" movido al AppBar**: dropdown del email en la barra superior en lugar del menu lateral.

### Dapper TypeHandlers

Registrados en `DependencyInjection.cs` para compatibilidad SQLite (tests) / SQL Server (produccion):
- `DateOnlyTypeHandler` / `NullableDateOnlyTypeHandler`
- `DateTimeOffsetTypeHandler` / `NullableDateTimeOffsetTypeHandler`

## Estado de compilacion y tests

```powershell
dotnet test Backend\KPG.Timesheet.sln
dotnet build Backend\KPG.Timesheet.sln
dotnet build Fronted\KPG.Timesheet.WebUI.sln
```

Resultado al 2026-05-23:
- Backend tests: **231/231 pasan** (68 Domain unit + 24 Application unit + 139 Integration).
- Backend build: 0 errores, 0 warnings.
- Frontend build: 0 errores, 0 warnings.

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

- Epics: `_bmad-output/planning-artifacts/epics.md`
- PRD: `_bmad-output/planning-artifacts/prd.md`
- Arquitectura: `_bmad-output/planning-artifacts/architecture.md`
- Manual tecnico: `Docs/manuals/manual-tecnico.md`
- Manual administrador: `Docs/manuals/manual-administrador.md`
- Manual usuario: `Docs/manuals/manual-usuario.md`
- Handoff sesion 18: `Docs/handoff/handoff-2026-05-23.md`
