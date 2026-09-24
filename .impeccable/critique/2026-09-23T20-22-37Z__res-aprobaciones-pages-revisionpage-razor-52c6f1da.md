---
target: Aprobaciones (RevisionPage) de KPG Timesheet
total_score: 25
max_score: 40
na_heuristics: 
p0_count: 1
p1_count: 2
target_identity: "file:C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Aprobaciones\\Pages\\RevisionPage.razor"
target_fingerprint: "sha256:d2258d1450bbc67267fe53e60f44ef328a10b359a36a7bfbcf41d31cc0db051c"
target_path: "C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Aprobaciones\\Pages\\RevisionPage.razor"
timestamp: 2026-09-23T20-22-37Z
slug: res-aprobaciones-pages-revisionpage-razor-52c6f1da
---
# Aprobaciones — crítica de diseño

## Veredicto
Diverge de la IA especificada: el mockup pide tres tabs de estado con conteo, filtros, selección múltiple y una tabla operativa. Lo implementado es un toggle de dos vías (no tres tabs), aprobación masiva de un solo sentido (sin rechazo masivo), y NO hay tabla — es un stack de "cards por día" con filas MudStack simulando filas de tabla, y falta por completo la columna "horario" (hora de entrada/salida).

## Heurísticas
| Heurística | Puntuación |
|---|---:|
| Visibilidad del estado | 3/4 |
| Correspondencia con el mundo real | 3/4 |
| Control y libertad | 3/4 |
| Consistencia y estándares | 2/4 |
| Prevención de errores | 2/4 |
| Reconocimiento sobre recuerdo | 3/4 |
| Flexibilidad y eficiencia | 2/4 |
| Diseño estético y minimalista | 3/4 |
| Recuperación de errores | 2/4 |
| Ayuda y documentación | 2/4 |
| **Total** | **25/40** |

## Fortalezas
- Semántica de aprobación multinivel bien modelada: EsAccionable/PuedeRevertir restringen correctamente acciones al nivel del revisor actual, con comentario explicando por qué.
- Revertir es una acción de primera clase reverificada contra backend — raro y valioso en un flujo de aprobación donde los errores cuestan.
- Lista de empleados filtrados a "quién puede revisar este revisor" en vez de exponer el roster completo — mejora real de privilegio mínimo.

## Problemas prioritarios
1. **P0:** No existe tabla operativa ni columna "horario" — las filas (RevisionPage.razor:162-218) muestran Cliente/Proyecto, Recurso/Modalidad y horas totales, pero ningún campo de hora de entrada/salida. Los revisores aprueban sin ver los horarios reales, el campo más relevante para detectar fraude o error.
2. **P1:** El tab "Pendientes" no tiene OnClick y aria-selected="true" está hardcodeado sin alternar nunca cuando el usuario cambia a "Aprobados/rechazados" — bug funcional y violación WCAG 4.1.2 (el estado comunicado a tecnología asistiva es falso).
3. **P1:** No hay rechazo masivo (solo aprobación masiva vía _seleccionados); y el rechazo por día aplica el MISMO comentario a todos los registros accionables del día aunque abarquen distintos clientes/proyectos.
4. **P2:** KpgEstadoAprobacionChip.razor hardcodea colores hex (#2E7D32/#C62828/#F9A825/#0D3B5E) que ni siquiera coinciden con los tokens --kpg-status-success/-warning/-error existentes — texto blanco sobre #F9A825 (ámbar) es ~2:1 de contraste, bajo el 4.5:1 de WCAG AA.
5. **P2:** Errores de operaciones masivas parciales se reducen a un solo mensaje (`errores[0]`) — si 2 de 5 aprobaciones fallan, el revisor solo se entera de una.

## Personas
- **Alex:** sin seleccionar-todo, aprobar 40 pendientes en 10 días requiere 40 clics individuales antes de que "Aprobar seleccionados" haga algo; sin rechazo masivo.
- **Riley:** comentario de rechazo compartido en un día con registros de múltiples clientes — el mismo texto llega a proyectos no relacionados; selección obsoleta tras refresh no se depura hasta un Clear() exitoso.

## Observaciones
- El checkbox "Seleccionar {NombreEmpleado}" no distingue qué registro específico selecciona cuando un empleado tiene múltiples líneas el mismo día.
- Edición de descripción durante revisión no muestra marca visible de "editado por supervisor" junto al texto corregido.

## Detector mecánico
`impeccable detect --json` sobre RevisionPage.razor + Components/ devolvió `[]` (confirmado con sanity-check: el detector SÍ detecta problemas reales en HTML sintético, pero no tiene cobertura de regex para sintaxis .razor — el `[]` es hueco de cobertura, no limpieza real). Evidencia complementaria por grep: 4 hardcodeos de Style= con px, y el hallazgo más significativo — KpgEstadoAprobacionChip.razor hardcodea 4 colores de estado que bypasean tokens existentes y ni siquiera coinciden con sus valores hex. Ningún botón icon-only interactivo carece de aria-label (solo íconos decorativos junto a texto visible, correcto). Sin navegador esta sesión.
