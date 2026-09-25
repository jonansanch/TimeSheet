# Fase 0 — Línea base de descripciones (resultado)

Fecha: 2026-09-23 · Fuente: base Azure `free-sql-db-9398472` (única con credenciales activas),
consultas de solo lectura, sin cambios.

## Advertencia sobre los datos

Solo hay **146 registros** en `RegistrosHoras`, y buena parte es data de seed/QA (commit
`feat(seed): add 40-user QA organization`), no uso real de consultores: `test 2`,
`Prueba gerente`, `pruebas internas cccc`, `test limpieza correcto`. Términos que motivaron este
plan (soporte, varios, pendientes, lo mismo) **no aparecen** — no porque no vayan a pasar, sino
porque todavía no hay volumen real. La lista de abajo combina lo que sí muestran estos datos con
los patrones típicos de ese tipo de descripción floja, para no salir en blanco.

**Recomendación:** repetir esta consulta a los 30–60 días de operación real (story 10.6) y ajustar
la lista con datos de verdad, antes de pasar nada a `Bloquear`.

## Hallazgos

**Descripciones repetidas** (candidatas a quedar como "referencia de buen ejemplo", no a
bloquear — son legítimas, solo repetidas porque es el mismo trabajo día a día):
`análisis y desarrollo de módulo de pagos` (43), `validación de reglas de facturación
electrónica` (43), `configuración de centros de costo proyecto offshore` (30).

**Distribución de palabras:**

| Palabras | Registros |
|---|---|
| 2 | 3 |
| 3 | 3 |
| 4 | 1 |
| 5 | 7 |
| 6 | 46 |
| 7 | 84 |
| 8 | 2 |

Las descripciones de calidad (las repetidas de arriba) tienen 6–7 palabras. Las de 2 palabras son
todas placeholders (`test 2`, `Prueba 1`, `Prueba gerente`). → **`MinPalabras = 4`** deja pasar
todo lo legítimo y marca los placeholders más cortos.

**Palabras sueltas encontradas:** `prueba/pruebas` (7), `sistema` (7), `test` (3),
`reunión` (2), `ajuste` (1), `trabajo` (1). No aparecieron `soporte`, `varios`, `pendientes`,
`lo mismo`, `n/a` — se agregan igual a la lista semilla por ser los que reportó el equipo.

## Lista semilla para la migración (story 10.1)

| Término | Tipo | Regla | Severidad inicial | Sugerencia | Motivo |
|---|---|---|---|---|---|
| prueba | Palabra | GenericoSiVaSolo | Advertir | `[App/Módulo] – [Qué se probó] – [Resultado de la prueba]` | No dice qué se probó ni el resultado |
| pruebas | Palabra | GenericoSiVaSolo | Advertir | `[App/Módulo] – Pruebas de [funcionalidad] – [resultado]` | Igual que "prueba" |
| test | Palabra | GenericoSiVaSolo | Advertir | `[App/Módulo] – Pruebas de [funcionalidad] – [resultado]` | Anglicismo genérico, mismo caso que "prueba" |
| soporte | Palabra | GenericoSiVaSolo | Advertir | `[App/Módulo] – Soporte a [usuario/área] sobre [incidencia] – [resolución]` | No dice a quién ni sobre qué |
| reunión | Palabra | GenericoSiVaSolo | Advertir | `[App/Módulo] – Reunión de [tema] con [área/cliente] – [acuerdo o resultado]` | No dice el tema ni el resultado |
| sistema | Palabra | GenericoSiVaSolo | Advertir | Reemplazar "sistema" por el nombre real de la app o módulo | Nombre genérico en vez del sistema real |
| varios | Palabra | ProhibidoSiempre | Advertir | Detallar cada tarea por separado o la principal del día | No es trazable, agrupa tareas distintas |
| ajustes | Palabra | GenericoSiVaSolo | Advertir | `[App/Módulo] – Ajuste en [módulo/elemento] – [resultado]` | No dice qué se ajustó |
| pendientes | Palabra | GenericoSiVaSolo | Advertir | `[App/Módulo] – [Tarea] pendiente de [motivo]` | No dice qué tarea ni por qué |
| trabajo | Palabra | GenericoSiVaSolo | Advertir | Reemplazar por la acción técnica concreta | No dice qué se hizo |
| lo mismo | Frase | ProhibidoSiempre | Advertir | Repetir la descripción completa de la tarea, aunque sea igual al día anterior | No es trazable sin ver el registro anterior |
| lo mismo de ayer | Frase | ProhibidoSiempre | Advertir | Igual que "lo mismo" | Igual que "lo mismo" |
| igual que ayer | Frase | ProhibidoSiempre | Advertir | Igual que "lo mismo" | Igual que "lo mismo" |
| n/a | Palabra | ProhibidoSiempre | Advertir | Completar con la tarea real del día | Vacío de contenido |
| ninguna | Palabra | ProhibidoSiempre | Advertir | Completar con la tarea real del día | Vacío de contenido |
| ok | Palabra | ProhibidoSiempre | Advertir | Completar con la tarea real del día | No describe ninguna tarea |
| listo | Palabra | ProhibidoSiempre | Advertir | `[App/Módulo] – [Qué se completó]` | No dice qué se completó |

**Parámetros iniciales (`ParametroSistema`):**

| Clave | Valor inicial |
|---|---|
| `Descripcion.ValidacionActiva` | `true` |
| `Descripcion.MinPalabras` | `4` |
| `Descripcion.MinPalabrasContexto` | `3` |
| `Descripcion.SeveridadReglasBase` | `Advertir` |

Todo en `Advertir`: con datos mayormente sintéticos, bloquear ahora generaría falsos positivos
sobre data de QA. Se revisa este umbral en la story 10.6, con datos de uso real.
