# Decision: Politica de idioma y localizacion futura

Fecha: 2026-05-14

## Estado

Aceptada para V1.

## Decision

La V1 de KPG Timesheet soporta solo Espanol como idioma de interfaz.

No se implementa selector Espanol/Ingles en V1. Ingles queda documentado como capacidad futura y debe abordarse como una historia transversal, no como ajuste pequeno dentro de una historia funcional.

## Motivo

El manejo multi idioma impacta varias capas:

- Textos visibles de UI
- Labels, botones, menus y estados vacios
- Mensajes de validacion frontend
- Mensajes de validacion backend
- Problem Details / errores de API
- Formatos de fecha y hora
- Reportes, exportaciones y bitacora
- Pruebas QA y UAT por idioma

Meterlo parcialmente en Epic 2 generaria mezcla de textos, inconsistencias y deuda dificil de rastrear.

## Alcance V1

Para V1:

- Todo texto visible al usuario final debe estar en Espanol.
- No debe aparecer mezcla innecesaria Espanol/Ingles en navegacion, formularios, mensajes, tablas o confirmaciones.
- Fechas deben mostrarse como `dd/MM/yyyy`.
- Horas deben mostrarse como `HH:mm`.
- Documentacion tecnica puede usar terminos tecnicos en Ingles cuando sean nombres de tecnologia, patrones o APIs.

## Trabajo futuro

La localizacion Espanol/Ingles debe implementarse en Epic 6 como:

- `Story 6.7: Politica de Idioma y Preparacion para Localizacion`

Requisitos ya agregados:

- `NFR21` en `_bmad-output/planning-artifacts/prd.md`
- `NFR21` y `UX-DR47` en `_bmad-output/planning-artifacts/epics.md`
- Story 6.7 en `_bmad-output/planning-artifacts/epics.md`

## Ruta tecnica sugerida para futuro Ingles

Cuando se implemente multi idioma:

1. Definir culturas soportadas: `es-CO` y `en-US` o `en`.
2. Centralizar textos de UI en recursos localizables (`.resx` o servicio equivalente para Blazor).
3. Evitar textos hardcodeados nuevos en componentes Razor.
4. Exponer codigos de error estables desde API y traducir mensajes en UI cuando aplique.
5. Definir estrategia para mensajes de FluentValidation del backend.
6. Persistir preferencia de idioma por usuario o, si aun no existe perfil, en almacenamiento local del navegador.
7. Cubrir QA por idioma en flujos criticos J1-J4.

## Checklist para no olvidarlo

- [x] Revisar textos hardcodeados antes de go-live. (Story 6.7 — 2026-05-20)
- [x] Confirmar que V1 no muestra selector de idioma.
- [x] Confirmar que todo texto final visible esta en Espanol.
- [x] Mantener Story 6.7 en backlog de Epic 6.
- [ ] Reabrir esta decision antes de implementar Ingles.

---

## Resultado de auditoría V1 (2026-05-20 — Story 6.7)

Se realizó auditoría completa del código fuente. Se encontraron y corrigieron los siguientes textos en inglés:

| Archivo | Texto inglés (antes) | Texto español (después) |
|---------|---------------------|------------------------|
| `Backend/src/Api/Infrastructure/ProblemDetailsExceptionHandler.cs` | `"The specified resource was not found."` | `"El recurso especificado no fue encontrado."` |
| `Backend/src/Api/Infrastructure/ProblemDetailsExceptionHandler.cs` | `"Unauthorized"` | `"No autorizado."` |
| `Backend/src/Api/Infrastructure/ProblemDetailsExceptionHandler.cs` | `"Forbidden"` | `"Acceso prohibido."` |
| `Backend/src/Application/Common/Exceptions/ValidationException.cs` | `"One or more validation failures have occurred."` | `"Uno o más errores de validación ocurrieron."` |

Auditado y confirmado en Español (sin cambios necesarios):
- Todo el NavMenu y layout de Blazor
- Labels, botones y estados vacíos de todas las páginas Razor
- Mensajes FluentValidation en Application layer
- Títulos de errores 400 (DomainRule, BadHttpRequest) y 500 en ProblemDetailsExceptionHandler
- Confirmaciones y diálogos de eliminación

Tests de regresión agregados en `ProblemDetailsExceptionHandlerTests.cs` para los 3 títulos traducidos (NotFoundException, UnauthorizedAccess, ForbiddenAccess).

---

## Textos hardcodeados para migrar en una versión multiidioma futura

Cuando se implemente soporte de Inglés, estos son los archivos y patrones que necesitarán migración a recursos `.resx` con `IStringLocalizer<T>`:

### Backend
- `ProblemDetailsExceptionHandler.cs` — todos los campos `Title` y `Detail` del switch (4 casos con texto español)
- `ValidationException.cs` — mensaje base del constructor
- `Application/Features/**/Validators/*.cs` — todos los `.WithMessage("...")` de FluentValidation

### Frontend (Blazor Razor)
- `Layout/NavMenu.razor` — labels de los MudNavLink (Inicio, Registro, Mis Registros, Dashboard, etc.)
- `Features/**/*.razor` — todos los `Label="..."` en MudTextField, MudSelect, MudDatePicker
- `Features/**/*.razor` — textos de MudButton (Guardar, Cancelar, Buscar, Exportar Excel, etc.)
- `Features/**/*.razor` — bloques de estado vacío (MudText dentro de `@if (!lista.Any())`)
- `Shared/Components/*.razor` — mensajes de error y confirmación en componentes compartidos

### Estrategia técnica recomendada (cuando llegue el momento)
1. Culturas: `es-AR` (default V1) y `en-US` (V2+)
2. `Microsoft.Extensions.Localization` + `IStringLocalizer<T>` en Blazor (nativo en .NET)
3. Archivos `.resx` por componente/feature siguiendo la estructura de carpetas existente
4. API expone `extensions.errorCode` estable; UI mapea código → texto localizado
5. FluentValidation: `WithMessage(() => localizer["ValidationKey"])`
6. Preferencia de idioma: cookie `ui-culture` o `localStorage` del navegador
7. QA por idioma: ejecutar script `qa-script.md` en ambos idiomas para J1-J4
