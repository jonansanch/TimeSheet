---
target: Mis Registros (HistorialPage) de KPG Timesheet
total_score: 25
max_score: 40
na_heuristics: 
p0_count: 2
p1_count: 1
target_identity: "file:C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Registro\\Pages\\HistorialPage.razor"
target_fingerprint: "sha256:6751c943f08119bbdc95a9141d846ef64bd96aa14b0b3c96d701c23974712a89"
target_path: "C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Registro\\Pages\\HistorialPage.razor"
timestamp: 2026-09-23T20-22-00Z
slug: atures-registro-pages-historialpage-razor-2afe2288
---
# Mis Registros (Historial) — crítica de diseño

## Veredicto
Implementación parcial del mockup, con una sustitución silenciosa de alcance: el strip de KPIs y la tabla existen, pero el KPI "Rechazadas" fue reemplazado por "Horas" y faltan 2 de los 4 filtros prometidos (cliente, estado). No es pulido pendiente, es una desviación de spec que cambia para qué sirve la pantalla.

## Heurísticas
| Heurística | Puntuación |
|---|---:|
| Visibilidad del estado | 2/4 |
| Correspondencia con el mundo real | 3/4 |
| Control y libertad | 2/4 |
| Consistencia y estándares | 3/4 |
| Prevención de errores | 3/4 |
| Reconocimiento sobre recuerdo | 3/4 |
| Flexibilidad y eficiencia | 2/4 |
| Diseño estético y minimalista | 3/4 |
| Recuperación de errores | 2/4 |
| Ayuda y documentación | 2/4 |
| **Total** | **25/40** |

## Fortalezas
- UI consciente de reglas de negocio: registros cerrados/firmados no editables, reenvío de un clic para rechazados es la única salida clara.
- Trazabilidad de aprobación vía tooltip de firmante por nivel, respondiendo a una queja documentada de usuarios.
- Colapso responsive real de KPI/filtros con breakpoints concretos, no solo overflow.

## Problemas prioritarios
1. **P0:** Faltan 2 de 4 filtros del mockup (cliente, estado) — solo existen mes/año; un usuario no puede aislar "solo rechazados" ni "solo cliente X" sin escanear manualmente hasta 100 filas/página.
2. **P0:** El KPI "Rechazadas" (el estado más accionable, dispara el flujo de reenvío) no existe como tile — fue reemplazado por "Horas" en la fila de KPIs.
3. **P1:** CargarDatosAsync hace un segundo fetch del mes completo sin paginar solo para calcular los KPIs, cada vez que cambia página, tamaño de página o filtro — costo de servidor oculto detrás de una UI de stat-cards.
4. **P2:** Todas las columnas son Sortable="false" y SortMode="SortMode.None" — sin poder ordenar por fecha/estado en una pantalla cuyo único trabajo es revisar una lista.
5. **P2:** La celda de estado (chip + icono de firmante + comentario de rechazo) no tiene overflow-wrap/line-clamp con max-width:260px — un comentario largo puede romper la altura uniforme de fila en un grid "Dense".
6. **P3:** El fondo de header (--kpg-surface-subtle) y el de hover de fila (--kpg-slate-50) difieren solo unas pocas unidades RGB — feedback de hover casi imperceptible.

## Personas
- **Alex:** sin ordenamiento ni filtro de cliente/estado, su flujo real ("mis rechazados de marzo del cliente X") requiere paginar manualmente todo el mes.
- **Sam:** columna de Acciones tiene Title="" (sin nombre accesible de columna); los KPIs y la tabla se actualizan sin región aria-live al cambiar filtro.

## Observaciones
- El chip de conteo total bajo la tabla duplica el KPI "Registros" de arriba sin contexto adicional.
- El campo de año permite escritura libre 2020-2099 sin debounce visible — riesgo de ráfaga de requests por dígito.

## Detector mecánico
`impeccable detect --json` sobre HistorialPage.razor y su CSS devolvió `[]` en ambos (cero hallazgos, modo regex limitado para .razor). Evidencia complementaria por grep: cero estilos inline, cero hex hardcodeados, los 2 "px" encontrados son media queries de breakpoint (legítimos), todo el spacing usa tokens. 3 botones icon-only en acciones de tabla tienen aria-label + tooltip correctamente. Un aria-label hardcodeado en español ("Detalle de aprobadores") no pasa por el sistema de localización usado en el resto del archivo. Sin navegador esta sesión.
