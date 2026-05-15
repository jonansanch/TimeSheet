# Epic 2 Ready for QA - 2026-05-14

## Estado

Epic 2 queda lista para revision QA.

Stories incluidas:

- `Docs/stories/2.1-registrar-turno-am-pm-con-validacion.md` - Status: review
- `Docs/stories/2.2-seleccionar-cliente-proyecto-y-contexto-con-sugerencias-recientes.md` - Status: review
- `Docs/stories/2.3-confirmacion-de-guardado-y-flujo-am-a-pm.md` - Status: review
- `Docs/stories/2.4-historial-personal-de-registros.md` - Status: review
- `Docs/stories/2.5-eliminar-registro-propio.md` - Status: review
- `Docs/stories/2.6-registro-propio-para-supervisor.md` - Status: review
- `Docs/stories/2.7-accesibilidad-estados-formulario.md` - Status: review

## Alcance funcional cubierto

- Registro de turnos AM/PM con validacion cliente y servidor.
- Sugerencias recientes de cliente/proyecto y filtrado de proyectos por cliente.
- Confirmacion visual de guardado, avance AM a PM y total de jornada completa.
- Historial personal de registros con ordenamiento por fecha.
- Eliminacion de registro propio con confirmacion y proteccion de ownership.
- Soporte de rol Supervisor para registro, historial y eliminacion de sus propios registros.
- Accesibilidad del formulario de registro: radiogroup de modalidad, roving tabindex, `aria-checked`, `aria-labelledby`, `aria-describedby`, mensajes inline y foco visible.

## Ajuste final antes de QA

En `Fronted/src/WebUI/Features/Registro/Components/KpgShiftForm.razor`, los botones Guardar AM/PM quedaron deshabilitados solo durante `_isSaving`.

Motivo:

- QA debe poder intentar guardar un formulario incompleto para verificar errores inline y asociaciones ARIA.
- La persistencia sigue bloqueada dentro de `SaveAsync` con `form.Validate()` + `CanSave(...)`.
- No se permite guardar si faltan campos, si la modalidad esta vacia o si la hora de salida no es mayor que la de entrada.

## Verificacion ejecutada

Backend:

```powershell
dotnet test Backend\KPG.Timesheet.sln
```

Resultado:

- 35/35 tests pasados
- Domain.UnitTests: 3/3
- Application.UnitTests: 12/12
- Infrastructure.IntegrationTests: 20/20

Frontend:

```powershell
dotnet build Fronted\KPG.Timesheet.WebUI.sln
```

Resultado:

- Compilacion correcta
- 0 warnings
- 0 errores

## Puertos para revision manual

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

## Comandos de arranque

Backend:

```powershell
dotnet run --project Backend\src\Api\KPG.Timesheet.Api.csproj --launch-profile https
```

Frontend:

```powershell
dotnet run --project Fronted\src\WebUI\KPG.Timesheet.WebUI.csproj --launch-profile https
```

Abrir:

```text
https://localhost:5201
```

## Foco sugerido para QA

1. Login como `empleado@kpg.com`.
2. Ir a `/registro` y validar fecha de hoy + turno AM por defecto.
3. Intentar guardar sin completar campos y verificar errores inline.
4. Completar AM, guardar y verificar banner + avance automatico a PM.
5. Usar "Copiar configuracion del AM", completar PM y verificar "Jornada completa".
6. Ir a `/mis-registros`, verificar que aparecen solo registros propios.
7. Eliminar un registro propio y confirmar que desaparece.
8. Repetir flujo base con `supervisor@kpg.com` para confirmar registro propio sin mezclar equipo.
9. En modalidad, validar navegacion con teclado y atributos ARIA desde DevTools.

## Decisiones a recordar

- Politica de idioma V1: solo Espanol. Ingles queda como trabajo futuro en Epic 6 / Story 6.7.
- Nota completa: `Docs/decisions/2026-05-14-politica-idioma-localizacion.md`.
