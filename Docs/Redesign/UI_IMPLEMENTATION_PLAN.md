# KPG Timesheet --- UI Implementation Plan

## Ejecución

Trabajar incrementalmente. Antes de modificar una vista, entender su
comportamiento. Después de cada fase ejecutar build, tests,
lint/typecheck disponibles y validación visual.

## Fases

1.  **Auditoría:** detectar stack, librerías, rutas, layouts, permisos,
    servicios y componentes.
2.  **Design System:** consolidar tokens y componentes reutilizables;
    eliminar duplicación segura.
3.  **AppShell:** ajustar sidebar, topbar, content container y
    responsive sin alterar permisos.
4.  **Inicio:** implementar `01-home.png`.
5.  **Registro:** implementar `02-time-entry.png`; prioridad crítica.
6.  **Mis Registros:** implementar `03-my-records.png`.
7.  **Dashboard:** implementar `04-dashboard.png`.
8.  **Aprobaciones:** implementar `05-approvals.png`.
9.  **Organigrama:** implementar `06-org-chart.png`.
10. **Administración:** aplicar `07-admin-users.png` a Usuarios,
    Recursos, Supervisores y módulos equivalentes.
11. **Resto:** aplicar el mismo sistema a fallas/mejoras, reportes,
    notificaciones y parámetros.
12. **Responsive + accesibilidad.**
13. **Visual QA final.**

## Visual QA obligatorio

Para cada ruta: 1. Abrir a 1440×900 o equivalente. 2. Comparar contra su
referencia. 3. Revisar composición, anchura, altura, spacing,
alineación, tipografía, cards, tablas y whitespace. 4. Corregir
diferencias evidentes. 5. Repetir hasta reducir el gap. 6. Ejecutar
validaciones técnicas.

## Restricciones

No hardcodear datos del mockup. No cambiar APIs, auth, permisos, base de
datos ni reglas de negocio sin autorización. No considerar una fase
terminada solo porque compile.
