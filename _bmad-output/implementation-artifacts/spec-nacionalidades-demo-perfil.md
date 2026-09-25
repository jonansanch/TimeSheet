---
title: 'Nacionalidades demostrativas y perfil'
type: 'feature'
created: '2026-09-24'
status: 'in-review'
baseline_commit: '9520457a835ac73f24e6e24312656672426de59f'
context:
  - '{project-root}/PRODUCT.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Los usuarios existentes no tienen nacionalidad, por lo que el organigrama no muestra banderas, y la pantalla Mi perfil tampoco presenta este dato.

**Approach:** Asignar una nacionalidad demostrativa variada y estable a cada usuario que aún tenga el campo vacío, sin sobrescribir valores existentes, y exponer desde la base de datos la bandera y el país del usuario autenticado en Mi perfil.

## Boundaries & Constraints

**Always:** Usar únicamente códigos del catálogo ISO existente; hacer la asignación determinista para que no cambie entre reinicios; limitar el backfill automático a Development; preservar nacionalidades previamente configuradas; consultar el dato actual desde base de datos en `/api/auth/me`; mostrar bandera Unicode, nombre localizado y fallback accesible.

**Ask First:** Ejecutar el backfill de datos ficticios fuera de Development; permitir que el empleado edite su propia nacionalidad; reemplazar banderas Unicode por imágenes o una librería.

**Never:** Sobrescribir valores reales; usar `Random` o `string.GetHashCode`; guardar el emoji; depender solamente de un claim JWT obsoleto; presentar la nacionalidad ficticia como información verificada.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Usuario sin país en Development | `CodigoPais = null` | Recibe un código variado, válido y estable del pool demostrativo | N/A |
| Usuario con país existente | `CodigoPais = CO` | Conserva `CO` | N/A |
| Perfil autenticado | Código válido asignado | Muestra bandera y nombre localizado | Si falla `/me`, conserva email/roles de claims y muestra estado no asignado |
| Organigrama | Usuarios rellenados | Cada nombre muestra su bandera correspondiente | Un código ausente no oculta el nombre |

</frozen-after-approval>

## Code Map

- `Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs` -- backfill demostrativo después de crear usuarios y organigrama QA.
- `Backend/src/Api/Endpoints/Auth.cs` -- endpoint `/api/auth/me` con lectura actual del usuario.
- `Fronted/src/WebUI/Infrastructure/Repositories/IAuthRepository.cs` y `AuthRepository.cs` -- consulta autenticada del perfil.
- `Fronted/src/WebUI/Infrastructure/Repositories/Models/AuthModels.cs` -- contrato del usuario actual.
- `Fronted/src/WebUI/Features/Auth/Pages/PerfilPage.razor` -- presentación de bandera y país.
- `Fronted/src/WebUI/Resources/SharedResource*.resx` -- etiquetas ES/EN.

## Tasks & Acceptance

**Execution:**
- [x] Añadir un selector determinista basado en SHA-256 y un pool ISO variado; aplicarlo solo a usuarios nulos en Development.
- [x] Convertir `/api/auth/me` en consulta async de `ApplicationUser` y devolver nombre completo y `CodigoPais`.
- [x] Extender repositorio/modelos frontend para obtener el perfil con Bearer token y fallback seguro.
- [x] Mostrar nacionalidad con bandera, nombre localizado y estado no asignado dentro de la información personal existente.
- [x] Añadir recursos localizados y pruebas de estabilidad, preservación, endpoint autorizado y respuesta sin país.

**Acceptance Criteria:**
- Given la base de desarrollo con usuarios sin nacionalidad, when corre el inicializador, then todos reciben países variados válidos y una segunda ejecución no cambia los valores.
- Given un usuario con nacionalidad definida, when corre el inicializador, then su valor permanece intacto.
- Given un usuario autenticado con nacionalidad, when abre Mi perfil, then ve su bandera y nombre de país obtenidos desde la base de datos.
- Given el organigrama después del backfill, when se renderizan los nodos, then las banderas aparecen junto a los nombres sin modificar la jerarquía.

## Spec Change Log

## Design Notes

La nacionalidad se integra como una fila más del bloque de información personal, no como una tarjeta adicional. La bandera funciona como dato visual solicitado, acompañada siempre por texto para accesibilidad y consistencia entre plataformas.

## Verification

**Commands:**
- `dotnet test Backend/KPG.Timesheet.sln --no-restore` -- pruebas backend completas aprobadas.
- `dotnet build Fronted/src/WebUI/KPG.Timesheet.WebUI.csproj --no-restore` -- perfil y contratos compilan sin advertencias nuevas.
- `impeccable.cmd detect --json Fronted/src/WebUI/Features/Auth/Pages/PerfilPage.razor` -- sin hallazgos mecánicos atribuibles al cambio.

**Manual checks (if no CLI):**
- Reiniciar API en Development, abrir Mi perfil y organigrama, y confirmar que las nacionalidades son variadas, estables y visibles.
