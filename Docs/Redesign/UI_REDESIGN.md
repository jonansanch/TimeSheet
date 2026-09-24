# KPG Timesheet --- UI Redesign

## Objetivo

Modernizar KPG Timesheet hacia una interfaz **Corporate SaaS** limpia,
compacta y profesional, conservando identidad KPG y toda la lógica
funcional.

## Fuentes de verdad

1.  El código actual es la fuente de verdad funcional: reglas, permisos,
    APIs, validaciones y datos.
2.  El mockup específico de cada ruta es la fuente de verdad visual.
3.  `00-overview.png` define el lenguaje visual global.
4.  Nunca hardcodear usuarios, horas, clientes, proyectos o métricas del
    mockup.

## Dirección visual

-   Brand dark `#073B5C`
-   Brand primary `#0B466A`
-   Background `#F6F8FA`
-   Surface `#FFFFFF`
-   Text primary `#172B3A`
-   Text secondary `#64748B`
-   Border `#E5EAF0`
-   Sidebar 220--240 px; topbar 60--64 px.
-   Content max-width 1280--1440 px salvo tablas justificadamente
    anchas.
-   Controls 38--42 px; cards radius 10--12 px; padding 16--20 px.
-   Spacing: 4 / 8 / 12 / 16 / 24 / 32 / 48 px.
-   Sombras discretas; sin glassmorphism, neumorphism ni decoración
    excesiva.

## Componentes

Reutilizar `AppShell`, `Sidebar`, `Topbar`, `PageHeader`, `Breadcrumbs`,
`Card`, `KPICard`, `Button`, `IconButton`, `Input`, `Textarea`,
`Select`, `DatePicker`, `TimeInput`, `SegmentedControl`, `StatusBadge`,
`Avatar`, `DataTable`, `Pagination`, `FilterBar`, `EmptyState`, `Alert`,
`Drawer`, `Modal`, `ConfirmDialog`, `Tooltip`, `Toast` y estados de
carga.

## Referencias por vista

-   `01-home.png`: bienvenida, CTA, cuatro accesos, semana, horas del
    mes y actividad/accesos rápidos.
-   `02-time-entry.png`: fecha compacta, horarios en filas, total,
    recientes y grid de información. No conservar el calendario grande
    como columna principal.
-   `03-my-records.png`: KPIs + FilterBar + tabla compacta. El workflow
    N1/N2/N3 debe ir en detalle secundario.
-   `04-dashboard.png`: KPIs + lista compacta del equipo + gráfico +
    pendientes críticos. Evitar una card grande por empleado.
-   `05-approvals.png`: bandeja con tabs, filtros, selección y acciones.
-   `06-org-chart.png`: nodos jerárquicos conectados; no lista
    indentada.
-   `07-admin-users.png`: patrón maestro para módulos administrativos.

## Responsive y accesibilidad

Validar desktop, laptop, tablet y móvil. Focus visible, teclado, labels,
contraste, targets adecuados y estados que no dependan solo del color.

## Definition of Done

Una vista está terminada cuando su composición, densidad, jerarquía y
spacing se aproximan a su mockup, conserva funcionalidad y se percibe
como parte del mismo producto. Cambiar colores solamente no constituye
un rediseño terminado.
