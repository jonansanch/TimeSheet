# UI Implementation Plan

Secuencia obligatoria: **análisis → design system → shell → páginas → responsive → accesibilidad → QA**.

## Estado de fases

| Fase | Alcance | Estado |
|---|---|---|
| 0 | Análisis y documentación | Completada |
| 1 | Design System | Completada |
| 2 | Application Shell | Completada |
| 3 | Inicio | Completada |
| 4 | Registro de jornada | Completada |
| 5 | Mis Registros y Mis Solicitudes | Completada |
| 6 | Dashboard | Pendiente |
| 7 | Aprobaciones | Pendiente |
| 8 | Organigrama | Pendiente |
| 9 | Administración | Pendiente |
| 10 | Reportar falla/mejora | Pendiente |
| 11 | Login y recuperación | Completada |
| 12 | Estados transversales | Pendiente |
| 13 | Responsive | Pendiente |
| 14 | Accesibilidad | Pendiente |
| 15 | QA final | Pendiente |

## Fase 0 — Análisis

- Mantener `UI_REDESIGN.md` como resumen arquitectónico.
- Inventariar rutas, roles, componentes y dependencias antes de migrar cada módulo.
- Criterio de salida: documentos presentes y alcance funcional protegido.

## Fase 1 — Design System

- Tema MudBlazor centralizado, tokens semánticos y primitivas KPG.
- Documentación en `Docs/design-system.md`.
- Criterio de salida: build limpio, detector visual revisado y componentes compatibles.

## Fase 2 — Application Shell

- Completar sidebar, topbar, identidad, usuario, idioma y notificaciones existentes.
- Agrupar navegación sin alterar visibilidad por roles.
- Validar drawer móvil, foco, textos largos y rutas activas.

## Fases 3–11 — Páginas

Migrar en este orden para reutilizar patrones:

1. Inicio y Login: composición y jerarquía.
2. Registro: formulario prioritario y sugerencias recientes.
3. Mis Registros y Mis Solicitudes: filtros, estados y tablas.
4. Dashboard y Aprobaciones: KPIs, tablas y acciones.
5. Administración y Organigrama: patrón CRUD compartido y jerarquía.
6. Reportes, Notificaciones y Reportar: filtros, tablas y estados.

En cada página:

- Sustituir encabezado repetido por `KpgPageHeader`.
- Consumir tokens y componentes base antes de crear CSS local.
- Preservar repositorios, eventos, validaciones y permisos.
- Comprobar loading, empty, error, success y disabled existentes.

## Fase 12 — Estados transversales

- Unificar empty states, skeletons, alertas, badges y feedback.
- No convertir estados estáticos en live regions innecesarias.

## Fase 13 — Responsive

- Verificar 1440+, 1280, 960, 768, 390 px.
- Sidebar responsivo; acciones y filtros apilables.
- Tablas priorizan columnas y limitan overflow a su propio contenedor.
- Targets táctiles de al menos 44 px en controles principales.

## Fase 14 — Accesibilidad

- Contraste WCAG AA y foco visible ≥ 3:1.
- Navegación completa por teclado.
- Labels, nombres accesibles y mensajes localizados.
- Estados con texto/icono, nunca solo color.
- Soporte de zoom y `prefers-reduced-motion`.

## Fase 15 — QA

Después de cada fase y al final:

1. `dotnet build Fronted/KPG.Timesheet.WebUI.sln`
2. `dotnet test Backend/KPG.Timesheet.sln --no-restore`
3. `git diff --check`
4. Detector Impeccable sobre archivos UI modificados.
5. Revisión de rutas, roles, acciones y consola del navegador.

## Reglas de avance

- No iniciar una fase con regresiones conocidas de la anterior.
- No introducir datos ficticios ni APIs decorativas.
- Todo cambio que afecte negocio, permisos, endpoints o backend requiere decisión explícita.
- Actualizar esta tabla al cerrar cada fase.
