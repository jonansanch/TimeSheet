---
title: 'Ajuste completo del menú y AppShell de KPG Timesheet'
type: 'refactor'
created: '2026-09-23'
status: 'done'
baseline_commit: 'b1e6ff6733771b749824f77de2a3db323a6d7498'
context:
  - '{project-root}/PRODUCT.md'
  - '{project-root}/Docs/Redesign/UI_REDESIGN.md'
  - '{project-root}/Docs/Redesign/UI_VISUAL_GAP_ANALYSIS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** El shell ya fue modernizado, pero el sidebar, sus grupos/submenús y la topbar aún no reproducen con suficiente fidelidad la densidad, jerarquía, alineación y comportamiento de los mockups de `Docs/Redesign/design-reference`.

**Approach:** Refinar exclusivamente el AppShell existente con MudBlazor, manteniendo toda la navegación, autorización, datos autenticados e internacionalización, y usando los PNG como autoridad visual para desktop y responsive.

## Boundaries & Constraints

**Always:** Conservar rutas, `AuthorizeView`, roles, logout, selector de idioma, notificaciones, sesión y datos reales del usuario; usar la librería Material Icons ya instalada; mantener foco visible, controles semánticos, `aria-label` y `aria-expanded`; tomar los cambios no confirmados actuales como línea base.

**Ask First:** Agregar dependencias, cambiar contratos públicos compartidos o alterar la lógica de permisos/autenticación.

**Never:** Rediseñar páginas internas; inventar rutas, usuarios, roles o contadores; eliminar opciones existentes; introducir otra librería UI; revertir cambios ajenos; copiar datos ficticios del mockup.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|---------------------------|----------------|
| Ruta principal | Ruta autorizada de primer nivel | Item correcto marcado como activo con señal visual no dependiente solo del color | La navegación sigue usando `MudNavLink` |
| Ruta hija | Ruta de Reportes, Administración o Parámetros | Grupo padre permanece expandido y el subitem activo es visible | Cambio de URL vuelve a sincronizar el grupo |
| Menú largo | Viewport con poca altura | Marca permanece arriba y solo la navegación hace scroll con scrollbar discreto | El contenido principal no se desplaza con el sidebar |
| Tablet/móvil | Ancho menor a 960 px | Drawer ocultable por hamburger, topbar compacta y shell sin overflow horizontal | Acciones esenciales permanecen accesibles |
| Texto largo | Nombre, rol o traducción extensa | Truncado legible sin desplazar acciones ni romper el ancho del shell | Tooltip/menú conserva el valor completo disponible |

</frozen-after-approval>

## Code Map

- `Fronted/src/WebUI/Layout/MainLayout.razor` — composición de AppShell, topbar, drawer, usuario y contenido.
- `Fronted/src/WebUI/Layout/NavMenu.razor` — menú por roles, rutas, grupos y estado expandido.
- `Fronted/src/WebUI/wwwroot/css/kpg-shell.css` — proporciones, estados, scroll y responsive del shell.
- `Fronted/src/WebUI/wwwroot/css/kpg-tokens.css` — tokens de ancho, altura, color, spacing y transición.
- `Fronted/src/WebUI/Resources/SharedResource*.resx` — etiquetas localizadas existentes; solo se amplían si hace falta texto real del shell.

## Tasks & Acceptance

**Execution:**
- [x] `MainLayout.razor` — compactar marca/topbar y reforzar semántica sin alterar sesión, usuario ni acciones.
- [x] `NavMenu.razor` — sincronizar grupos con la ruta activa y conservar todas las opciones/RBAC.
- [x] `kpg-shell.css` — reproducir ancho, alturas, spacing, active/hover/focus, indentación, scroll y breakpoints del mockup.
- [x] `kpg-tokens.css` — reutilizar o ajustar solo los tokens globales necesarios para evitar valores dispersos.
- [x] Ejecutar build y detector Impeccable; QA visual intentado, bloqueado por firewall de Azure SQL y ausencia de navegador automatizable en la sesión.

**Acceptance Criteria:**
- Given cualquier rol vigente, when abre el menú, then solo ve sus opciones autorizadas y todas las rutas existentes siguen navegando igual.
- Given una ruta hija activa, when el shell renderiza o cambia de URL, then el grupo correspondiente permanece expandido y padre/hijo expresan la ubicación actual.
- Given viewport desktop de 1280–1920 px, when se compara con los mockups, then sidebar de ~232 px, topbar de ~60 px, marca compacta, densidad y offsets coinciden sustancialmente.
- Given viewport estrecho, when se abre/cierra el hamburger, then el drawer funciona sin overflow y las acciones esenciales de la topbar permanecen accesibles.

## Spec Change Log

## Design Notes

Modo `Operate`. El target visual es el shell común visible en los ocho PNG: sidebar navy compacto, logo horizontal integrado, labels discretos, filas de navegación densas y topbar blanca de una sola línea. El estado activo usa fondo azul medio y una señal estructural lateral; los subitems son más pequeños, indentados y sin iconos superfluos.

## Verification

**Commands:**
- `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — cero errores nuevos.
- `git diff --check` — cero errores de whitespace.
- `impeccable.cmd detect --json <changed targets>` — sin hallazgos mecánicos materiales.

**Manual checks:**
- Capturas válidas a 1440 px y 390 px; comparación de marca, scroll, active state, submenús, topbar, offsets y overflow contra `00`–`07`.

**Limitación de entorno:** El frontend inicia en `http://localhost:5200`, pero el backend no inicia porque Azure SQL rechaza la IP pública actual. La superficie de Computer Use tampoco expone un navegador, por lo que no fue posible producir capturas autenticadas válidas en esta sesión.

## Suggested Review Order

**Composición y preservación funcional**

- Entrada del shell, usuario real, drawer y contenedores anchos por ruta.
  [`MainLayout.razor:12`](../../../Fronted/src/WebUI/Layout/MainLayout.razor#L12)

- Destino accesible del salto al contenido y límite global del área principal.
  [`MainLayout.razor:93`](../../../Fronted/src/WebUI/Layout/MainLayout.razor#L93)

**Navegación y submenús**

- Grupos RBAC existentes conservan rutas y enlazan expansión controlada.
  [`NavMenu.razor:66`](../../../Fronted/src/WebUI/Layout/NavMenu.razor#L66)

- La ruta activa sincroniza y mantiene visible su grupo padre.
  [`NavMenu.razor:158`](../../../Fronted/src/WebUI/Layout/NavMenu.razor#L158)

**Fidelidad visual y responsive**

- Sidebar fijo y navegación desplazable aíslan el scroll del contenido.
  [`kpg-shell.css:42`](../../../Fronted/src/WebUI/wwwroot/css/kpg-shell.css#L42)

- Jerarquía compacta, contraste y estados activos reproducen el target.
  [`kpg-shell.css:69`](../../../Fronted/src/WebUI/wwwroot/css/kpg-shell.css#L69)

- Tokens centrales fijan proporciones del shell sin valores dispersos.
  [`kpg-tokens.css:83`](../../../Fronted/src/WebUI/wwwroot/css/kpg-tokens.css#L83)
