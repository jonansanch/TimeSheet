# Rediseño UI de KPG Timesheet

## Objetivo

Evolucionar la aplicación Blazor WebAssembly existente hacia una experiencia Corporate SaaS moderna, clara y consistente, conservando íntegramente lógica de negocio, rutas, endpoints, modelos, permisos, validaciones y datos reales.

## Fuente de verdad

1. La implementación actual define funcionalidad y permisos.
2. El mockup define dirección visual, no comportamiento ni datos.
3. `Docs/design-system.md` define la infraestructura visual reutilizable.
4. `Docs/UI_IMPLEMENTATION_PLAN.md` define orden, alcance y validaciones.

## Arquitectura real

- Frontend: Blazor WebAssembly .NET 10.
- UI: MudBlazor 8.5.0.
- Gráficas: Blazor-ApexCharts 6.1.0.
- Estado y autenticación: servicios Blazor y `KpgAuthStateProvider`.
- Autorización: `AuthorizeRouteView`, atributos `[Authorize]` y `AuthorizeView` por rol.
- Estilos: tema MudBlazor, tokens CSS, CSS global y CSS aislado por componente.
- Organización: páginas y componentes por feature; repositorios HTTP tipados bajo `Infrastructure`.

## Dirección visual

- Navy KPG como identidad y navegación.
- Fondo general gris muy claro y superficies blancas.
- Bordes suaves, radios de 8–12 px y sombras discretas.
- Tipografía legible, títulos compactos y jerarquía inequívoca.
- Alta densidad en tablas; whitespace generoso entre grupos funcionales.
- Estados semánticos acompañados de texto o icono.
- Movimiento mínimo y respetuoso de `prefers-reduced-motion`.

## Principios

- Design tokens → componentes base → componentes de dominio → páginas.
- MudBlazor sigue siendo la biblioteca principal; no se añade otra librería UI.
- Una acción primaria por contexto.
- Los estados loading, empty, error y disabled forman parte del diseño.
- Responsive no equivale a añadir scroll global: cada superficie debe priorizar contenido.
- La seguridad no depende de que un enlace esté oculto.

## Componentes

### Reutilizados y tematizados

`MudLayout`, `MudDrawer`, `MudAppBar`, `MudNavMenu`, `MudDataGrid`, `MudForm`, campos, diálogos, alertas, tooltips, chips, skeletons y ApexCharts.

### Base KPG

`KpgPageHeader`, `KpgCard`, `KpgButton`, `KpgInput`, `KpgSelect`, `KpgStatusBadge`, `KpgStatCard`, `KpgDataTable`, `KpgEmptyState` y `KpgTableSkeleton`.

### Componentes de dominio conservados

Formularios, calendarios, diálogos administrativos, aprobación, registro por voz, repositorios y modelos mantienen su contrato. Su migración es visual y progresiva.

## Protección de regresiones

- No se hardcodean datos del mockup.
- No se cambian API, backend, DTO, roles ni reglas.
- Cada fase exige build sin errores y revisión del diff.
- Cambios visuales se separan en commits lógicos.
- Hallazgos previos no relacionados se documentan y no se mezclan.

