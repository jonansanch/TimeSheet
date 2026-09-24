---
target: Dashboard de equipo (DashboardPage) de KPG Timesheet
total_score: 23
max_score: 40
na_heuristics: 
p0_count: 2
p1_count: 2
target_identity: "file:C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Dashboard\\Pages\\DashboardPage.razor"
target_fingerprint: "sha256:9abff4053b272c41ca8e2a799b872d6ffc0bd8dd90e11ed722283425b43d4e35"
target_path: "C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Dashboard\\Pages\\DashboardPage.razor"
timestamp: 2026-09-23T20-22-18Z
slug: tures-dashboard-pages-dashboardpage-razor-d5cfacf2
---
# Dashboard de equipo — crítica de diseño

## Veredicto
El grid de 3 columnas del primer viewport existe, pero el gráfico de distribución de horas está DUPLICADO (versión compacta arriba + sección completa abajo) con dos controles de fecha desconectados entre sí que gobiernan datos distintos, y el campo "puesto" requerido por el mockup en las filas de equipo no existe en el contrato de datos.

## Heurísticas
| Heurística | Puntuación |
|---|---:|
| Visibilidad del estado | 2/4 |
| Correspondencia con el mundo real | 3/4 |
| Control y libertad | 2/4 |
| Consistencia y estándares | 2/4 |
| Prevención de errores | 3/4 |
| Reconocimiento sobre recuerdo | 2/4 |
| Flexibilidad y eficiencia | 2/4 |
| Diseño estético y minimalista | 2/4 |
| Recuperación de errores | 3/4 |
| Ayuda y documentación | 2/4 |
| **Total** | **23/40** |

## Fortalezas
- Divulgación progresiva por rol vía AuthorizeView para ocultar secciones gerenciales/admin de supervisores.
- KpgTeamStatusCard degrada con gracia en móvil, quitando la columna de horas en vez de desbordar.
- Cada sección async tiene su propio try/catch/finally independiente — un fallo no bloquea toda la página.

## Problemas prioritarios
1. **P0:** El gráfico "Distribución de horas" aparece DOS VECES — compacto en el workspace de 3 columnas y de nuevo en una sección completa abajo con su propio selector Hoy/Semana/Mes/Personalizado. El MudDatePicker del header (un solo día) no afecta a ninguno de los dos gráficos — solo los botones de período inferiores lo hacen — creando una trampa real: cambiar la fecha del header no mueve el gráfico.
2. **P0:** Las filas de equipo no tienen campo "puesto/rol" pese a que el mockup lo requiere (avatar/nombre/puesto/horas/estado) — KpgTeamStatusCard solo expone Nombre/Estado/TotalMinutos/HorariosRegistrados; un supervisor no puede distinguir un consultor de un lead de un vistazo.
3. **P1:** La lista de pendientes críticos trunca silenciosamente a 6 (`_criticos.Pendientes.Take(6)`) sin mostrar el total ni link "ver todos" — justo donde más importa la visibilidad de riesgo.
4. **P1:** El tile KPI "Pendiente" usa Color.Default (el más bajo en jerarquía visual) mientras Completo/Parcial usan Success/Warning — la prioridad de color está invertida respecto a qué número requiere más atención.
5. **P2:** La lista de equipo no tiene empty state (a diferencia de sus hermanas chart/critical) y toda la sección de 3 columnas está bloqueada detrás de la carga más lenta de tres cargas independientes que podrían resolver en paralelo.

## Personas
- **Alex:** sin búsqueda/orden en lista de equipo; dos controles de fecha desconectados le hacen leer datos de gráfico obsoletos pensando que están actualizados; lista crítica limitada a 6 sin total.
- **Sam:** los gráficos ApexChart no tienen aria-label ni tabla de datos alternativa — un usuario de lector de pantalla no tiene acceso equivalente a "horas por consultor".

## Observaciones
- Textos hardcodeados en español ("Estado del equipo", aria-label "Resumen operativo del equipo") rompen la disciplina de localización usada en el resto del archivo.
- Colores de grid del gráfico hardcodeados en #e0e0e0 en vez de --kpg-border-default.
- L["Error_Server"] genérico en todos los catch sin logging visible adjunto.

## Detector mecánico
`impeccable detect --json` sobre DashboardPage.razor + Components/ devolvió `[]` (cero hallazgos, cobertura regex limitada para .razor). Evidencia complementaria por grep: 19 usos de Style= con px hardcodeado en DashboardPage.razor (parámetros MudBlazor, no CSS cruda), 3 BorderColor="#e0e0e0" en config de ApexCharts (fuera del pipeline normal de tokens CSS), KpgTeamStatusCard.razor usa tokens correctamente en color/borde/radio pero hardcodea gap/min-height/font-size en px. Cero botones icon-only en el alcance. Sin navegador esta sesión.
