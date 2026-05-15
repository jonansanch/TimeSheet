# Story 1.5: Shell Visual KPG y Tema Base

Status: review

## Story

As a usuario autenticado,
I want una interfaz base clara y consistente con mi rol,
So that pueda orientarme rápidamente en la plataforma.

## Acceptance Criteria

1. **Given** un usuario autenticado, **When** entra a la aplicación, **Then** ve un layout desktop-first con sidebar fijo de 240px, top app bar de 64px y contenido centrado con ancho máximo de 1200px, **And** el ítem activo se distingue visualmente con indicador lateral.

2. **Given** el tema KPG aprobado, **When** la app renderiza componentes MudBlazor, **Then** usa tokens de color KPG, Poppins para títulos y Roboto para cuerpo, **And** conserva contraste compatible con WCAG 2.1 AA.

3. **Given** la dirección visual A (sidebar navy), **When** se implementa el shell definitivo, **Then** el sidebar tiene fondo #0D3B5E con texto e iconos blancos.

## Tasks / Subtasks

- [x] **T1 — MainLayout: MudBlazor layout completo** (AC: 1)
  - [x] Reemplazar `Fronted/src/WebUI/Layout/MainLayout.razor` con MudLayout + MudAppBar (64px) + MudDrawer (240px, Variant=Persistent) + MudMainContent
  - [x] Top app bar: título "KPG Timesheet" a la izquierda, email del usuario + botón cerrar sesión a la derecha
  - [x] Contenido principal con ancho máximo de 1200px centrado

- [x] **T2 — NavMenu: MudBlazor navigation** (AC: 1, 3)
  - [x] Reemplazar `Fronted/src/WebUI/Layout/NavMenu.razor` con MudNavMenu + MudNavLink por rol (AuthorizeView)
  - [x] Ítem "Inicio" — todos los autenticados
  - [x] Ítem "Registro" — Empleado, Supervisor, Gerente, Admin
  - [x] Ítems "Dashboard" y "Reportes" — Supervisor, Gerente, Admin
  - [x] Ítem "Administración" — solo Admin
  - [x] Indicador visual lateral en ítem activo (Match="NavLinkMatch.All" para Inicio)

- [x] **T3 — CSS: sidebar navy y limpieza** (AC: 2, 3)
  - [x] Vaciar `MainLayout.razor.css` (MudBlazor maneja el layout)
  - [x] Vaciar `NavMenu.razor.css` (MudBlazor maneja el nav)
  - [x] Aplicar color #0D3B5E al drawer vía MudTheme DrawerBackground + CSS en `app.css`
  - [x] Texto e iconos del drawer en blanco (#FFFFFF, contraste 13.5:1 WCAG AA)

- [x] **T4 — Tipografía UX-DR5** (AC: 2)
  - [x] Verificar que `App.razor` MudTheme ya aplica Poppins a H1-H6 y Roboto al default
  - [x] Agregar sizes en Typography: H4=24px (page title), H5=20px (card title), Body1=16px, Body2=14px, Caption=12px

- [x] **T5 — Compilar y verificar** (AC: 1, 2, 3)
  - [x] `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — 0 errores, 0 advertencias
  - [x] `dotnet build Backend/KPG.Timesheet.sln` — 0 errores, 0 advertencias
  - [x] `dotnet test Backend/KPG.Timesheet.sln` — 9/9 pasan

## Dev Notes

### Dirección visual elegida: A — Sidebar Navy

- Sidebar fondo: `#0D3B5E` (Primary)
- Sidebar texto/iconos: `#FFFFFF`
- Ítem activo: borde lateral izquierdo 3px celeste `#5BB8D4` + fondo semitransparente `rgba(255,255,255,0.12)`
- Top app bar: fondo `#0D3B5E` (Primary), texto blanco

### Estructura MudBlazor para layout (referencia)

```razor
@inherits LayoutComponentBase
<MudLayout>
    <MudAppBar Elevation="0" Color="Color.Primary">
        <!-- logo + título + user info + logout -->
    </MudAppBar>
    <MudDrawer Open="true" Variant="DrawerVariant.Persistent" Elevation="0">
        <MudNavMenu>
            <!-- NavLinks con AuthorizeView por rol -->
        </MudNavMenu>
    </MudDrawer>
    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Large" Class="py-6">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>
```

### MudNavLink activo visual (Dirección A)

```razor
<MudNavLink Href="/" Match="NavLinkMatch.All" Icon="@Icons.Material.Filled.Home">
    Inicio
</MudNavLink>
```

MudBlazor aplica clase `active` automáticamente. Para el indicador lateral,
sobrescribir con CSS en `app.css`:

```css
.mud-nav-link.active {
    border-left: 3px solid #5BB8D4;
    background-color: rgba(255,255,255,0.12) !important;
}
```

### Color drawer en MudTheme

Usar `DrawerBackground = "#0D3B5E"` en la paleta del tema en `App.razor`, o sobrescribir con CSS.
Para texto blanco en el drawer usar `DrawerText = "#FFFFFF"` y `DrawerIcon = "#5BB8D4"`.

### UX-DR5 tamaños tipográficos

Poppins H4 = 24px (page title), H5 = 20px (card title). Las páginas deben usar `Typo.h4`
para el título de página y `Typo.h5` para títulos de cards.

### MaxWidth.Large en MudContainer

`MaxWidth.Large` = 1280px en MudBlazor. Usar `MaxWidth.ExtraLarge` si se quiere 1920px.
UX-DR2 dice 1200px máximo — usar `Style="max-width:1200px"` explícito si hace falta.

### Referencias

- UX-DR2, UX-DR3, UX-DR4, UX-DR5, UX-DR6, UX-DR44
- [Architecture](../../_bmad-output/planning-artifacts/architecture.md)
- [Epics Story 1.5](../../_bmad-output/planning-artifacts/epics.md#story-15-shell-visual-kpg-y-tema-base)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- MUD0002: `Title` (PascalCase) en `MudIconButton` no es atributo Blazor válido — corregido a `title` (minúscula HTML)
- CSS `rgba(255,255,255,0.85)` y `rgba(255,255,255,0.1)` generaron advertencias WCAG S7924 — reemplazados con `#FFFFFF` y `#1B4F73` (colores sólidos con contraste computable)

### Completion Notes List

- T1: `MainLayout.razor` reemplazado con MudLayout + MudAppBar (Primary navy) + MudDrawer (Persistent 240px) + MudMainContent (max 1200px). Email del usuario y botón logout en la app bar.
- T2: `NavMenu.razor` reemplazado con MudNavMenu + MudNavLink por rol usando AuthorizeView. Ítem activo detectado automáticamente por MudBlazor (clase `active`).
- T3: CSS legacy vaciado; estilos sidebar navy en `app.css`: texto blanco #FFFFFF (contraste 13.5:1), hover #1B4F73, activo con borde lateral #5BB8D4.
- T4: MudTheme actualizado con DrawerBackground, DrawerText, DrawerIcon, AppbarBackground, AppbarText y todos los tamaños tipográficos UX-DR5.
- T5: Frontend 0 errores 0 advertencias; Backend 0 errores 0 advertencias; 9/9 unit tests pasan.

### File List

- `Fronted/src/WebUI/Layout/MainLayout.razor` — reemplazado con MudBlazor shell completo
- `Fronted/src/WebUI/Layout/MainLayout.razor.css` — vaciado (MudBlazor maneja layout)
- `Fronted/src/WebUI/Layout/NavMenu.razor` — reemplazado con MudNavMenu por rol
- `Fronted/src/WebUI/Layout/NavMenu.razor.css` — vaciado (MudBlazor maneja nav)
- `Fronted/src/WebUI/App.razor` — tema KPG extendido (DrawerBackground, AppbarBackground, tipografía UX-DR5)
- `Fronted/src/WebUI/wwwroot/css/app.css` — estilos globales + sidebar navy CSS overrides

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2026-05-13 | 0.1 | Story creada, dirección A elegida por Jonathan | claude-sonnet-4-6 |
| 2026-05-13 | 1.0 | Implementación completa — todos los ACs satisfechos | claude-sonnet-4-6 |
