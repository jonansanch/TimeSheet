---
title: 'Capa base del Design System KPG'
type: 'refactor'
created: '2026-09-23'
status: 'done'
baseline_commit: '5b538d34ceaed6f8974bab158a0e730bb9e891fe'
context:
  - '{project-root}/_bmad-output/planning-artifacts/ux-design-specification.md'
  - '{project-root}/Docs/architecture/frontend-implemented-architecture.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La UI activa usa MudBlazor correctamente, pero su identidad visual está fragmentada entre el tema en `App.razor`, CSS global, estilos inline y componentes con decisiones locales. Esto dificulta modernizar las pantallas sin duplicar CSS ni introducir inconsistencias.

**Approach:** Crear una capa base de Design System KPG sobre MudBlazor: tokens semánticos, tema centralizado, shell moderno y componentes compartidos compatibles con los contratos actuales. El mockup guía la dirección — SaaS corporativo, navy KPG, fondos claros, superficies blancas, radios de 8–12 px, sombras discretas y whitespace generoso — sin buscar reproducción pixel-perfect.

## Boundaries & Constraints

**Always:** Mantener Blazor WASM, MudBlazor, rutas, autorización, eventos y contratos existentes; preservar responsive design, navegación por teclado, foco visible, contraste WCAG AA y estados no dependientes solo del color; reutilizar componentes MudBlazor antes de abstraer; mantener español/inglés intactos.

**Ask First:** Cambiar la estructura funcional de una página, añadir dependencias, modificar datos mostrados, ocultar navegación existente o cambiar una API pública de componente que requiera migración funcional.

**Never:** Cambiar endpoints, modelos, permisos, repositorios o lógica de negocio; introducir otra librería UI; rehacer páginas completas; inventar métricas del mockup; modificar el prototipo Angular; tocar los archivos no rastreados ajenos al frontend.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Desktop autenticado | Ancho ≥ 960 px | Sidebar navy y topbar clara; contenido sobre fondo gris claro | El shell conserva navegación, sesión y roles actuales |
| Viewport estrecho | Ancho < 960 px | Drawer responsivo y contenido sin overflow estructural | Controles mantienen área táctil y foco visibles |
| Estado semántico | Success, warning, error o info | Badge/card usa token, texto o icono además del color | Variante desconocida cae a neutral legible |
| Contenido ausente | Colección vacía | EmptyState accesible con título, descripción y acción opcional | Sin acción no se renderiza control vacío |

</frozen-after-approval>

## Code Map

- `Fronted/src/WebUI/App.razor` — providers y tema inline actual.
- `Fronted/src/WebUI/Shared/Theming/KpgTheme.cs` — nuevo tema MudBlazor centralizado.
- `Fronted/src/WebUI/wwwroot/css/kpg-tokens.css` — tokens primitivos y semánticos.
- `Fronted/src/WebUI/wwwroot/css/app.css` — reset, browser surfaces y compatibilidad global.
- `Fronted/src/WebUI/Layout/MainLayout.razor` — AppShell y Topbar actuales, conservando sesión.
- `Fronted/src/WebUI/Layout/NavMenu.razor` — Sidebar y autorización existentes.
- `Fronted/src/WebUI/Shared/Components/` — primitivas visuales KPG.

## Tasks & Acceptance

**Execution:**
- [x] `Shared/Theming/KpgTheme.cs`, `App.razor` — centralizar paleta, tipografía, radios y defaults compatibles con MudBlazor.
- [x] `wwwroot/css/kpg-tokens.css`, `index.html`, `app.css` — definir e integrar tokens de color, texto, spacing, radios, sombras, estados, backgrounds, borders, foco y movimiento reducido.
- [x] `Layout/MainLayout.razor(.css)`, `Layout/NavMenu.razor(.css)` — convertir el layout existente en AppShell/Topbar/Sidebar moderno sin alterar auth, sesión, rutas ni visibilidad por rol.
- [x] `Shared/Components/KpgPageHeader.razor`, `KpgCard.razor`, `KpgButton.razor`, `KpgInput.razor`, `KpgSelect.razor`, `KpgStatusBadge.razor`, `KpgEmptyState.razor`, `KpgDataTable.razor` — crear wrappers base pequeños, tipados y accesibles cuando aporten un patrón estable.
- [x] `Shared/Components/KpgStatCard.razor`, `KpgEstadoChip.razor`, `KpgTableSkeleton.razor` — refactorizar los componentes base existentes para consumir tokens sin romper sus parámetros actuales.
- [x] `Docs/design-system.md` — documentar tokens, componentes, variantes y uso recomendado.

**Acceptance Criteria:**
- Given la aplicación existente, when compila el frontend, then no cambia ningún contrato de datos, ruta, permiso ni flujo funcional.
- Given cualquier pantalla existente, when usa controles MudBlazor no migrados, then hereda una base visual coherente sin requerir una segunda librería.
- Given teclado o preferencia de movimiento reducido, when el usuario navega, then foco, contraste y motion respetan accesibilidad.
- Given los componentes nuevos, when se consumen con sus valores por defecto, then producen una UI coherente sin estilos inline específicos de página.

## Spec Change Log

## Design Notes

Los tokens se organizan en dos niveles: primitivos (`--kpg-navy-700`, `--kpg-space-4`) y semánticos (`--kpg-color-primary`, `--kpg-surface-page`). Los wrappers no deben ocultar capacidades de MudBlazor ni replicar toda su API; su función es codificar patrones KPG repetibles. La migración de páginas se realizará después y no forma parte de este cambio.

## Verification

**Commands:**
- `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — compilación sin errores.
- `impeccable detect --json <changed targets>` — sin infracciones mecánicas relevantes.
- `git diff --check` — sin errores de whitespace.

**Manual checks:**
- Inspeccionar shell en desktop y móvil, foco visible, estados hover/disabled y ausencia de regresiones en enlaces por rol.

## Suggested Review Order

**Fundamentos visuales**

- Centraliza la paleta, tipografía y geometría consumidas por MudBlazor.
  [`KpgTheme.cs:9`](../../../Fronted/src/WebUI/Shared/Theming/KpgTheme.cs#L9)

- Define tokens primitivos y semánticos independientes de componentes concretos.
  [`kpg-tokens.css:1`](../../../Fronted/src/WebUI/wwwroot/css/kpg-tokens.css#L1)

**Shell y navegación**

- Conserva sesión y autorización mientras moderniza topbar, drawer y contenido.
  [`MainLayout.razor:9`](../../../Fronted/src/WebUI/Layout/MainLayout.razor#L9)

- Mantiene las reglas de visibilidad por rol dentro del menú existente.
  [`NavMenu.razor:5`](../../../Fronted/src/WebUI/Layout/NavMenu.razor#L5)

**Primitivas compartidas**

- Establece encabezados de página con título semántico y acciones responsivas.
  [`KpgPageHeader.razor:1`](../../../Fronted/src/WebUI/Shared/Components/KpgPageHeader.razor#L1)

- Integra tablas, carga y estados vacíos sin imponer datos de servidor.
  [`KpgDataTable.razor:1`](../../../Fronted/src/WebUI/Shared/Components/KpgDataTable.razor#L1)

- Unifica estados visuales con fallback neutral y texto visible.
  [`KpgStatusBadge.razor:1`](../../../Fronted/src/WebUI/Shared/Components/KpgStatusBadge.razor#L1)

**Guía de adopción**

- Documenta cuándo usar wrappers KPG y cuándo conservar MudBlazor directo.
  [`design-system.md:1`](../../../Docs/design-system.md#L1)
