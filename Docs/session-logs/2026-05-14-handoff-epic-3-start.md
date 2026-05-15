# Handoff de Sesion - 2026-05-14 - Inicio Epic 3

## Punto exacto en el que quedamos

Epic 2 quedo implementada, revisada por QA y aprobada por Jonathan.

Todas las stories de Epic 2 estan en `Status: done`:

- `Docs/stories/2.1-registrar-turno-am-pm-con-validacion.md`
- `Docs/stories/2.2-seleccionar-cliente-proyecto-y-contexto-con-sugerencias-recientes.md`
- `Docs/stories/2.3-confirmacion-de-guardado-y-flujo-am-a-pm.md`
- `Docs/stories/2.4-historial-personal-de-registros.md`
- `Docs/stories/2.5-eliminar-registro-propio.md`
- `Docs/stories/2.6-registro-propio-para-supervisor.md`
- `Docs/stories/2.7-accesibilidad-estados-formulario.md`

Siguiente paso al retomar:

- Iniciar Epic 3.
- Crear/implementar `Story 3.1: Aplicar Ventana de Registro Retroactivo`.
- Referencia principal: `_bmad-output/planning-artifacts/epics.md`, seccion `Epic 3`, `Story 3.1`.

## Cierres y documentos relevantes

- QA aprobado Epic 2: `Docs/session-logs/2026-05-14-epic-2-qa-approved.md`
- Handoff pre-QA Epic 2: `Docs/session-logs/2026-05-14-epic-2-ready-for-qa.md`
- Decision de idioma/localizacion: `Docs/decisions/2026-05-14-politica-idioma-localizacion.md`
- PRD: `_bmad-output/planning-artifacts/prd.md`
- Epics: `_bmad-output/planning-artifacts/epics.md`
- Arquitectura: `_bmad-output/planning-artifacts/architecture.md`
- UX: `_bmad-output/planning-artifacts/ux-design-specification.md`

## Ajustes aplicados durante QA de Epic 2

- Agregado `MudPopoverProvider` en `Fronted/src/WebUI/App.razor`.
- Corregida renderizacion del email en:
  - `Fronted/src/WebUI/Layout/MainLayout.razor`
  - `Fronted/src/WebUI/Pages/Home.razor`
- En `Fronted/src/WebUI/Features/Registro/Components/KpgShiftForm.razor`:
  - Botones Guardar AM/PM permiten disparar validacion y solo se deshabilitan durante guardado.
  - `Lugar` va antes de `Descripcion`.
  - `Lugar` es selector fijo con:
    - `Presencial Oficina`
    - `Presencial Viaje`
    - `Presencial Cliente`
    - `Remoto`
  - `MudDatePicker` usa `DateFormat="dd/MM/yyyy"`.
- Se documento que V1 es solo Espanol; Ingles queda para Epic 6 / Story 6.7.

## Verificacion ejecutada

Backend:

```powershell
dotnet test Backend\KPG.Timesheet.sln
```

Resultado:

- 35/35 tests pasados.

Frontend:

```powershell
dotnet build Fronted\KPG.Timesheet.WebUI.sln
```

Resultado:

- 0 errores.
- 0 warnings.

## Puertos fijos

Backend API:

- HTTPS: `https://localhost:5101`
- HTTP: `http://localhost:5100`
- Scalar/OpenAPI: `https://localhost:5101/scalar`

Frontend WebUI:

- HTTPS: `https://localhost:5201`
- HTTP: `http://localhost:5200`

## Usuarios de prueba

Empleado:

- Email: `empleado@kpg.com`
- Password: `Empleado1234!`

Supervisor:

- Email: `supervisor@kpg.com`
- Password: `Supervisor1234!`

Admin:

- Email: `admin@kpg.com`
- Password: `Admin1234!`

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

Ejecutar verificacion:

```powershell
dotnet test Backend\KPG.Timesheet.sln
dotnet build Fronted\KPG.Timesheet.WebUI.sln
```

## Recordatorios para Epic 3

Epic 3 trata:

- Ventana de registro retroactivo.
- Solicitudes de excepcion.
- Aprobacion/rechazo Admin.
- Inmutabilidad post-guardado.
- Edicion limitada de descripcion.
- Eliminacion por roles segun reglas.

Para Story 3.1, revisar antes de implementar:

- `FR7`
- `FR15`
- `UX-DR8`
- `UX-DR9`
- `UX-DR10`
- Arquitectura: `ParametroSistema`, ventana retroactiva configurable, reglas de dominio y Problem Details.

No asumir que la ventana queda hardcodeada. La arquitectura indica que debe venir de parametros del sistema; si aun no existe el catalogo real, documentar claramente el puente tecnico temporal o implementar el minimo necesario segun el alcance de la story.
