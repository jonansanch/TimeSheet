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

- [ ] Revisar textos hardcodeados antes de go-live.
- [ ] Confirmar que V1 no muestra selector de idioma.
- [ ] Confirmar que todo texto final visible esta en Espanol.
- [ ] Mantener Story 6.7 en backlog de Epic 6.
- [ ] Reabrir esta decision antes de implementar Ingles.
