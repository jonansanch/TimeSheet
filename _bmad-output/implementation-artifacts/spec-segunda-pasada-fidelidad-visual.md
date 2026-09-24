---
title: 'Segunda pasada de fidelidad visual KPG Timesheet'
type: 'refactor'
created: '2026-09-23'
status: 'in-review'
baseline_commit: 'b1e6ff6733771b749824f77de2a3db323a6d7498'
context:
  - '{project-root}/PRODUCT.md'
  - '{project-root}/docs/UI_VISUAL_GAP_ANALYSIS.md'
  - '{project-root}/Docs/design-system.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La primera iteración modernizó la piel visual, pero conservó composiciones heredadas y produjo un reskin con exceso de espacio, formularios altos y tablas ruidosas. La aplicación todavía no reproduce la densidad, jerarquía ni organización del mockup objetivo.

**Approach:** Reestructurar el AppShell y las vistas prioritarias con patrones compartidos del Design System, tomando el mockup como autoridad visual y la implementación/API como autoridad funcional. La iteración será progresiva, con build por grupos y comparación visual final.

## Boundaries & Constraints

**Always:** Conservar rutas, APIs, endpoints, modelos, roles, permisos, validaciones, estados, voz, retroactividad, aprobación, CRUD e i18n; usar datos reales disponibles; mantener teclado, foco y WCAG AA; reutilizar MudBlazor y primitivas KPG.

**Ask First:** Agregar una dependencia; cambiar una API pública compartida con impacto funcional; ocultar una acción o dato vigente; modificar un contrato backend; sustituir un asset corporativo por uno no provisto.

**Never:** Hardcodear datos del mockup; crear búsqueda o “Recordarme” falsos; eliminar flujos por diferencias del target; introducir otra librería UI; tocar el prototipo Angular o los archivos no rastreados `palace/`, `roster-backups/` y `roster.json`; limitar la solución a cambios cosméticos.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Datos completos | APIs responden y el rol autoriza | Layout compacto con KPIs, listas, tablas y acciones derivadas de datos reales | Mantener feedback y estados actuales |
| Datos parciales o vacíos | Resumen no disponible o colección vacía | Preservar la composición con empty state honesto, sin cifras inventadas | Error de bloque no debe inutilizar el resto de la vista |
| Permisos distintos | Empleado, Supervisor, Gerente o Admin | Solo aparecen rutas, acciones y métricas autorizadas | Nunca exponer controles deshabilitados que revelen permisos ajenos |
| Viewport estrecho | 390–959 px | Drawer y secciones apiladas, sin overflow estructural; tablas con scroll controlado | Acciones y foco siguen siendo accesibles |
| Datos largos/corruptos | Nombres largos, jerarquía huérfana o ciclo | Truncado/expansión legible; organigrama no recurre infinitamente | Alertar datos huérfanos/cíclicos sin bloquear toda la página |

</frozen-after-approval>

## Code Map

- `Fronted/src/WebUI/wwwroot/css/kpg-tokens.css`, `app.css`, `Shared/Theming/KpgTheme.cs` — tokens y defaults visuales.
- `Fronted/src/WebUI/Layout/` — shell, topbar, sidebar y navegación por rol.
- `Fronted/src/WebUI/Features/Auth/` y `Pages/Home.razor` — login e inicio.
- `Fronted/src/WebUI/Features/Registro/` — registro, historial y componentes de horario/fecha/recientes.
- `Fronted/src/WebUI/Features/Dashboard/` y `Features/Aprobaciones/` — vistas operativas y gerenciales.
- `Fronted/src/WebUI/Features/ReportesUsuario/` y `Features/Admin/` — reporte, organigrama y administración.
- `Fronted/src/WebUI/Shared/Components/` — primitivas reutilizables KPG.

## Tasks & Acceptance

**Execution:**
- [x] Fundamentos/AppShell — compactar tokens, contenedor, sidebar, topbar y patrones transversales sin cambiar autorización.
- [x] Login/Home — reproducir proporciones y jerarquía del target; cargar solo resúmenes disponibles y manejar vacíos.
- [x] Registro/Historial — convertir horarios en filas, mover fecha arriba, reorganizar formulario, KPIs, filtros y tabla sin perder reglas.
- [x] Dashboard/Aprobaciones — reemplazar cards de empleado por listas y convertir revisión en bandeja compacta conservando niveles y acciones.
- [x] Reportar/Organigrama/Admin — limitar ancho, construir nodos jerárquicos y aplicar un único patrón de toolbar/card/table.
- [ ] Responsive/QA — ajustar 390/1280/1440/1920, compilar, capturar, comparar, corregir una ronda y ejecutar detector.

**Acceptance Criteria:**
- Given una vista prioritaria, when se renderiza a 1440 × 900, then su composición, densidad, jerarquía, cards y whitespace corresponden sustancialmente al mockup.
- Given cualquier rol vigente, when navega y actúa, then conserva exactamente los permisos, reglas y contratos funcionales previos.
- Given datos vacíos, errores parciales o texto largo, when la vista renderiza, then permanece legible, honesta y operable sin datos ficticios.
- Given teclado o viewport estrecho, when el usuario completa un flujo, then foco, orden, áreas interactivas y overflow permanecen utilizables.

## Spec Change Log

- 2026-09-23: Segunda pasada aplicada a shell, autenticación, home y vistas operativas; build por grupos sin errores. QA visual queda pendiente porque no hay navegador o superficie GUI disponible en la sesión.
- 2026-09-23: Corrección estructural posterior a revisión: Registro usa filas de tramo con duración/acción/total; Home incorpora cuatro acciones y segunda fila; Dashboard reorganiza su primer viewport en tres paneles; Aprobaciones añade selección y aprobación masiva; Historial compacta horarios/horas/firmas; Organigrama permite plegar ramas; Administración adopta card y toolbar comunes.

## Design Notes

Modo `Operate`. El primer viewport debe priorizar tarea y excepción: acciones compactas, KPIs de lectura rápida y listas/tablas de alta señal. El target fija una estética SaaS corporativa sobria: navy KPG, fondo gris azulado, superficie blanca, bordes `#E5EAF0`, radios 10–12 px y sombra mínima. No se añade búsqueda global sin funcionalidad real ni se inventan métricas para rellenar el layout.

Implementación: se unificó el contenedor a 1440 px, se redujo la densidad de controles/tablas, se convirtió el estado del equipo en filas compactas, se agregaron KPIs derivados al historial y se protegieron ciclos del organigrama. El detector Impeccable solo reporta la tipografía Inter ya comprometida por el Design System y hojas generadas de MudBlazor no resolubles desde `obj`.

Revisión estructural: los KPIs de historial se calculan sobre la colección completa del periodo y no solo sobre la página visible. Las acciones masivas de aprobación reutilizan `AprobarAsync` registro por registro y se limitan a elementos accionables para el nivel actual. No se añadieron endpoints, dependencias ni datos ficticios.

## Verification

**Commands:**
- `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — cero errores.
- `git diff --check` — cero errores de whitespace.
- `C:\Users\devel\.agents\skills\impeccable\scripts\impeccable.cmd detect --json <changed-targets>` — sin hallazgos mecánicos materiales.

**Manual checks:**
- Capturas desktop/mobile válidas; roles Empleado/Supervisor/Gerente/Admin; ES/EN; loading/error/empty; foco/teclado; CRUD, registro, historial y aprobación sin regresiones.
