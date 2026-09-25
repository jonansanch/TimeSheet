---
title: 'Nacionalidad de empleados y bandera en el organigrama'
type: 'feature'
created: '2026-09-24'
status: 'done'
baseline_commit: '14dbe06762fcdaa80802f6e8185840f9b7a8a79a'
context:
  - '{project-root}/PRODUCT.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Las personas administradas como usuarios no tienen nacionalidad, por lo que ese dato no puede asignarse ni reconocerse visualmente en el organigrama.

**Approach:** Guardar por usuario un código de país ISO 3166-1 alfa-2 opcional, permitir seleccionarlo al crear o actualizar sus datos organizacionales y derivar de ese código una bandera accesible junto a su nombre en el organigrama.

## Boundaries & Constraints

**Always:** Asociar la nacionalidad a `ApplicationUser`/`AspNetUsers`, que representa a la persona; conservar los usuarios existentes mediante una columna nullable; aceptar únicamente códigos ISO alfa-2 conocidos y normalizados en mayúsculas; mantener localización español/inglés, navegación por teclado y una etiqueta textual accesible para la bandera; preservar las reglas actuales de supervisor, puesto y roles.

**Ask First:** Hacer obligatoria la nacionalidad para todos los usuarios; convertir las nacionalidades en un catálogo CRUD administrable; mostrar banderas en el PDF exportado; asignar automáticamente una nacionalidad a registros existentes.

**Never:** Guardar la nacionalidad en la entidad `Empleado`, porque actualmente representa el catálogo de puestos/recursos; almacenar emojis o nombres libres como fuente de verdad; incorporar una librería pesada o imágenes externas de banderas; romper la deserialización de usuarios sin nacionalidad.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Alta o actualización | Código ISO válido, por ejemplo `CO` | Persiste `CO`, lo devuelve el API y queda disponible en empleados y organigrama | N/A |
| Usuario histórico | Nacionalidad nula | Continúa visible y editable; el organigrama no muestra una bandera vacía | N/A |
| Código inválido | Vacío no nulo, longitud distinta de 2 o código desconocido | No se persiste | Error específico de validación del campo |
| Organigrama | Usuario con código ISO válido | Muestra la bandera inmediatamente al lado del nombre con nombre de país accesible | Si no puede derivarse, omite la bandera sin ocultar el nombre |

</frozen-after-approval>

## Code Map

- `Backend/src/Infrastructure/Identity/ApplicationUser.cs` -- persistencia de los datos personales del usuario.
- `Backend/src/Infrastructure/Identity/IdentityService.cs` -- altas, edición estructural, listado y consulta SQL del organigrama.
- `Backend/src/Application/Features/Users` y `Backend/src/Application/Common/Interfaces/IIdentityService.cs` -- contratos, validación y comandos de usuarios.
- `Backend/src/Infrastructure/Migrations` -- migración nullable de `AspNetUsers` y snapshot EF.
- `Fronted/src/WebUI/Infrastructure/Repositories/Models/UserAdminModels.cs` -- contratos HTTP para usuarios y organigrama.
- `Fronted/src/WebUI/Features/Admin/Components/KpgUserDialog.razor` -- selección de nacionalidad durante el alta.
- `Fronted/src/WebUI/Features/Admin/Components/KpgUserEstructuraDialog.razor` -- actualización de supervisor, puesto y nacionalidad.
- `Fronted/src/WebUI/Features/Admin/Pages/UsuariosAdminPage.razor` -- presenta y envía el dato administrable.
- `Fronted/src/WebUI/Features/Admin/Pages/OrganigramaPage.razor` y `Fronted/src/WebUI/wwwroot/css/app.css` -- bandera junto al nombre y ajuste visual.
- `Fronted/src/WebUI/Resources/SharedResource*.resx` -- etiquetas, ayudas y validación localizadas.

## Tasks & Acceptance

**Execution:**
- [x] Extender `ApplicationUser`, DTOs, comandos, interfaz y `IdentityService` con `CodigoPais`, normalización/validación compartida y proyección en usuarios y organigrama.
- [x] Crear una migración EF nullable para `AspNetUsers.CodigoPais` y actualizar el snapshot sin valores predeterminados.
- [x] Extender los modelos y formularios Blazor con un selector de países basado en ISO, localizado, buscable y con opción de limpiar.
- [x] Renderizar una bandera derivada del ISO junto al nombre del nodo, con `title`/texto accesible, sin cambiar la jerarquía ni la exportación PDF.
- [x] Actualizar recursos ES/EN y estilos mínimos consistentes con las tarjetas actuales.
- [x] Ampliar pruebas de validadores e integración para alta, actualización, listado, organigrama, nulos y códigos inválidos.

**Acceptance Criteria:**
- Given un administrador que crea o edita una persona, when selecciona una nacionalidad y guarda, then el valor queda persistido y vuelve seleccionado al reabrir sus datos.
- Given un usuario con nacionalidad configurada, when se carga el organigrama, then aparece su bandera inmediatamente junto al nombre y los lectores de pantalla reciben el nombre del país.
- Given usuarios sin nacionalidad o datos organizacionales existentes, when se despliega la migración y se consultan las pantallas, then continúan funcionando sin asignaciones implícitas ni errores.
- Given un código de país alterado o inválido enviado directamente al API, when el backend lo valida, then rechaza la solicitud con un error de campo y no modifica al usuario.

## Spec Change Log

## Design Notes

La bandera es un complemento visual del nombre, no un sustituto textual. Se deriva en UI desde las dos letras ISO mediante indicadores regionales; el código persistido permite cambiar presentación o localización sin migrar datos. La edición se integra al diálogo organizacional existente para evitar un segundo flujo de perfil que compita con supervisor y puesto.

## Verification

**Commands:**
- `dotnet test Backend/KPG.Timesheet.sln` -- todas las pruebas backend terminan correctamente.
- `dotnet build Fronted/src/WebUI/KPG.Timesheet.WebUI.csproj` -- contratos y componentes Blazor compilan sin advertencias nuevas atribuibles al cambio.
- `impeccable.cmd detect --json <changed UI targets>` -- sin hallazgos mecánicos de diseño pendientes.

**Manual checks (if no CLI):**
- Crear y editar un usuario con nacionalidad, recargar y comprobar persistencia; revisar el organigrama con y sin nacionalidad en escritorio y viewport estrecho; verificar teclado, foco y etiqueta accesible.

## Suggested Review Order

**Contrato y compatibilidad**

- El endpoint distingue clientes antiguos de actualizaciones o limpiezas explícitas.
  [`Users.cs:44`](../../Backend/src/Api/Endpoints/Users.cs#L44)

- El servicio normaliza, valida y preserva nacionalidad cuando el campo se omite.
  [`IdentityService.cs:113`](../../Backend/src/Infrastructure/Identity/IdentityService.cs#L113)

- El catálogo versionado acepta exactamente los 249 códigos ISO alfa-2.
  [`CodigoPaisIso.cs:4`](../../Backend/src/Application/Common/Models/CodigoPaisIso.cs#L4)

**Persistencia**

- La persona guarda el código ISO opcional sin alterar puestos ni jerarquía.
  [`ApplicationUser.cs:26`](../../Backend/src/Infrastructure/Identity/ApplicationUser.cs#L26)

- La migración añade una columna nullable compatible con usuarios históricos.
  [`20260926000000_AgregarCodigoPaisUsuarios.cs:11`](../../Backend/src/Infrastructure/Migrations/20260926000000_AgregarCodigoPaisUsuarios.cs#L11)

**Experiencia administrativa y organigrama**

- El diálogo organizacional permite seleccionar, cambiar o limpiar la nacionalidad.
  [`KpgUserEstructuraDialog.razor:48`](../../Fronted/src/WebUI/Features/Admin/Components/KpgUserEstructuraDialog.razor#L48)

- La administración presenta la nacionalidad persistida por cada persona.
  [`UsuariosAdminPage.razor:81`](../../Fronted/src/WebUI/Features/Admin/Pages/UsuariosAdminPage.razor#L81)

- La bandera acompaña al nombre con etiqueta accesible y fallback seguro.
  [`OrganigramaPage.razor:142`](../../Fronted/src/WebUI/Features/Admin/Pages/OrganigramaPage.razor#L142)

**Cobertura**

- Las pruebas HTTP protegen mapping, omisión legacy y actualización explícita.
  [`UsersEndpointMappingTests.cs:10`](../../Backend/tests/Infrastructure.IntegrationTests/Users/UsersEndpointMappingTests.cs#L10)

- Las pruebas de estructura cubren normalización, preservación, limpieza e inválidos.
  [`IdentityServiceEstructuraTests.cs:178`](../../Backend/tests/Infrastructure.IntegrationTests/Users/IdentityServiceEstructuraTests.cs#L178)
