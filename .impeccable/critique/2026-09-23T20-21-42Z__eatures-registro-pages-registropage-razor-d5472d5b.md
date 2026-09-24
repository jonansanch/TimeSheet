---
target: Registrar jornada (RegistroPage) de KPG Timesheet
total_score: 25
max_score: 40
na_heuristics: 
p0_count: 1
p1_count: 2
target_identity: "file:C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Registro\\Pages\\RegistroPage.razor"
target_fingerprint: "sha256:be06c5e2a4e09da160608783b8053b9ad6a1e13c97c9c7b3c9aed53bb22dc54e"
target_path: "C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Registro\\Pages\\RegistroPage.razor"
timestamp: 2026-09-23T20-21-42Z
slug: eatures-registro-pages-registropage-razor-d5472d5b
---
# Registrar jornada — crítica de diseño

## Veredicto
El redisenio entrega ganancias reales de interacción (bloques ocupados, guardas de cuarto de hora, sugerencias recientes) pero NO entrega la IA de cuatro zonas del mockup: es una sola card densa, no fecha compacta + card de tramos + panel secundario + grid compacto. Un calendario mensual completo (KpgMiniCalendario) existe en código pero nunca se monta.

## Heurísticas
| Heurística | Puntuación |
|---|---:|
| Visibilidad del estado | 3/4 |
| Correspondencia con el mundo real | 3/4 |
| Control y libertad | 2/4 |
| Consistencia y estándares | 2/4 |
| Prevención de errores | 3/4 |
| Reconocimiento sobre recuerdo | 3/4 |
| Flexibilidad y eficiencia | 3/4 |
| Diseño estético y minimalista | 1/4 |
| Recuperación de errores | 3/4 |
| Ayuda y documentación | 2/4 |
| **Total** | **25/40** |

## Fortalezas
- Reglas de negocio visibles en UI: banner+chip de bloque ocupado, validación de cuarto de hora y salida-después-de-entrada.
- Reducción real de reingreso: sugerencias recientes, autocomplete de descripción, autofill de Recurso desde el claim del JWT.
- Accesibilidad real: aria-live en errores, aria-label por campo de horario, radiogroup accesible para Modalidad con navegación por flechas.

## Problemas prioritarios
1. **P0:** KpgMiniCalendario.razor (calendario mensual con puntos de estado y leyenda) existe en el código pero cero referencias en la app — RegistroPage.razor:17-19 solo monta KpgShiftForm; la selección de fecha es un KpgDatePicker modal simple, sin panel de mes siempre visible.
2. **P1:** KpgShiftForm.razor:14-282 mete alertas, selector de fecha, aviso, sugerencias, voz, tabla de tramos y grid de día en un solo MudStack sin separación — contradice las cuatro zonas del mockup.
3. **P1:** Dos sistemas de color hardcodeados y no unificados codifican disponibilidad de fecha (app.css:245-259 con rgba, KpgMiniCalendario.razor:152-156 con hex distinto) — ninguno referencia --kpg-status-success/-warning/-error.
4. **P2:** ToggleVoiceAsync (KpgShiftForm.razor:795-817) llama a _model.Reset() al iniciar dictado, borrando silenciosamente cualquier dato ya ingresado sin confirmación.
5. **P2:** Marcador de campo requerido inconsistente — Modalidad usa "*" explícito, los otros 5 campos requeridos (Cliente, Proyecto, Recurso, Lugar, Descripción) solo tienen Required="true" sin asterisco visible.
6. **P3:** "Usados recientemente" (KpgRecentSuggestions.razor) no tiene contenedor visual propio, se lee como metadato suelto en vez del atajo principal que promete el mockup.

## Personas
- **Jordan:** ~14 inputs simultáneos con una sola sección con encabezado; la regla de cuarto de hora solo se descubre al fallar.
- **Sam:** semáforo de disponibilidad de fecha es solo color (WCAG 1.4.1); la fila de encabezado de la tabla de tramos está aria-hidden mientras cada label visible repite el aria-label del campo, generando salida redundante en lector de pantalla.

## Observaciones
- Dos implementaciones de "total de horas" (TotalHorasFormateado vs TotalFormulario) sin etiqueta que distinga cuál es cuál si coincidieran en pantalla.
- Botón de eliminar por fila usa Color.Error incluso en bloques vacíos, implicando una acción destructiva donde no hay nada que destruir.
- El botón de micrófono solo tiene tooltip al pasar el mouse, sin etiqueta de texto persistente, a diferencia del botón Guardar/Enviar.

## Detector mecánico
`impeccable detect --json` sobre RegistroPage.razor + Components/ devolvió `[]` (cero hallazgos) — cobertura limitada del detector en archivos .razor (modo regex, no análisis HTML/CSS completo). Evidencia complementaria por grep: 8 estilos inline (7 en KpgMiniCalendario.razor, código huérfano no montado), 7 colores hex sin token también en KpgMiniCalendario.razor. RegistroPage.razor, KpgDatePicker.razor y KpgRecentSuggestions.razor están limpios de estilos inline/hex. Ningún botón icon-only carece de aria-label. Sin navegador esta sesión.
