# Current Status - KPG Timesheet

Ultima actualizacion: 2026-05-14

## Estado actual

Epic 2 esta cerrada:

- Implementacion completada.
- QA manual aprobado por Jonathan.
- Stories 2.1 a 2.7 en `Status: done`.

Epic 3 esta implementada y fue validada funcionalmente por Jonathan durante la sesion:

- Stories 3.1 a 3.6 implementadas.
- Los archivos de story siguen en `Status: review` porque no se ejecuto cierre documental individual a `done`.
- Ultima story: `Docs/stories/3.6-reglas-inmutabilidad-dominio-api.md`.

Epic 4 esta en progreso:

- Story 4.1: Gestionar Cuentas de Usuario, implementada y validada funcionalmente por Jonathan.
- Story 4.2: Asignar y Modificar Roles, implementada y lista para QA funcional.
- Siguiente historia recomendada despues de QA 4.2: Story 4.3 Gestionar Empleados.

## Punto exacto para retomar

Retomar desde:

- QA funcional de `Docs/stories/4.2-asignar-y-modificar-roles.md`.
- Pantalla principal: `/admin/usuarios`.
- Usuario inicial recomendado: `admin@kpg.com`.

Checklist rapido de QA 4.2:

- Cambiar rol de un usuario no-admin, por ejemplo `empleado@kpg.com`, a `Supervisor`.
- Confirmar que la fila actualiza el rol sin recargar.
- Confirmar que el usuario refleja el nuevo rol despues de relogin/refresh.
- Intentar cambiar el propio rol del Admin autenticado: debe ser rechazado.
- Intentar quitar Admin al ultimo administrador activo: debe ser rechazado.
- Cambiar rol a un usuario inactivo: debe cambiar rol sin activarlo.

## Documentos importantes

- Handoff actualizado: `Docs/handoff/handoff-2026-05-14.md`
- Session log de cierre actual: `Docs/session-logs/2026-05-14-epic-4-story-4-2-handoff.md`
- Story 4.1: `Docs/stories/4.1-gestionar-cuentas-de-usuario.md`
- Story 4.2: `Docs/stories/4.2-asignar-y-modificar-roles.md`
- QA Epic 2 aprobado: `Docs/session-logs/2026-05-14-epic-2-qa-approved.md`
- Decision idioma/localizacion: `Docs/decisions/2026-05-14-politica-idioma-localizacion.md`
- Epics: `_bmad-output/planning-artifacts/epics.md`
- PRD: `_bmad-output/planning-artifacts/prd.md`
- Arquitectura: `_bmad-output/planning-artifacts/architecture.md`
- UX: `_bmad-output/planning-artifacts/ux-design-specification.md`

## Validacion conocida

Ultima validacion ejecutada tras Story 4.2:

```powershell
dotnet test Backend\KPG.Timesheet.sln --no-restore
dotnet build Backend\KPG.Timesheet.sln --no-restore
dotnet build Fronted\KPG.Timesheet.WebUI.sln --no-restore
```

Resultado:

- Backend tests: 88/88 pasan.
- Backend build: 0 errores, 0 warnings.
- Frontend build: 0 errores, 0 warnings.

## Usuarios de prueba

- Admin: `admin@kpg.com` / `Admin1234!`
- Supervisor: `supervisor@kpg.com` / `Supervisor1234!`
- Empleado: `empleado@kpg.com` / `Empleado1234!`
- Empleado adicional: `ana.garcia@kpg.com` / `Empleado1234!`
- Empleado adicional: `carlos.ruiz@kpg.com` / `Empleado1234!`

## Comandos para retomar

Verificar puertos:

```powershell
Get-NetTCPConnection -LocalPort 5100,5101,5200,5201 -ErrorAction SilentlyContinue |
    Select-Object LocalAddress,LocalPort,State,OwningProcess |
    Sort-Object LocalPort
```

Arrancar backend:

```powershell
dotnet run --project Backend\src\Api\KPG.Timesheet.Api.csproj --launch-profile https
```

Arrancar frontend:

```powershell
dotnet run --project Fronted\src\WebUI\KPG.Timesheet.WebUI.csproj --launch-profile https
```

Abrir:

```text
https://localhost:5201
```

## Notas de trabajo

- El repo tiene muchos archivos sin commitear y artefactos `bin/obj/.vs`; no revertir cambios no relacionados.
- No existe `_bmad-output/implementation-artifacts/sprint-status.yaml`; el avance se esta siguiendo en `Docs/current-status.md`, handoff y story files.
- Story 4.2 quedo en `Status: review`; no marcar `done` hasta que Jonathan valide en navegador.
