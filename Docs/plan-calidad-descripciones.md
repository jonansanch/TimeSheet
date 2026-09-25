# Plan: Calidad de descripciones del timesheet

Fecha: 2026-09-23 · Estado: propuesto · Épica tentativa: 10

## Contexto

Los consultores escriben descripciones genéricas o incompletas ("soporte", "reunión",
"lo mismo que ayer"). La guía de IT (*Guía comentarios timesheet*, Ramces Just) pide:

- Describir la tarea, no la acción genérica; ser específico; tono neutro y ejecutivo.
- Formato recomendado: `[App/Módulo] – [Acción técnica] – [Elemento afectado] – [Resultado esperado]`
- Ejemplos:
  - `CIMA WebClient – Ajuste en módulo de evidencias para solicitudes de cancelación.`
  - `ACUDEN Digital – Optimización aplicada al someter solicitud.`
  - `API – Validación de SPS en aprobar y rechazar documentos.`

### Estado actual del código

| Punto | Hoy | Archivo |
|---|---|---|
| Validación | Solo `NotEmpty` + `MaximumLength(1000)` | `CreateRegistroHorasCommandValidator.cs`, `CreateRegistrosRangoCommand.cs`, `UpdateDescripcionRegistroHorasCommandValidator.cs`, `RegistroHoras.UpdateDescripcion` |
| Formulario | `MudTextField` libre, sin ayuda | `Fronted/.../Registro/Components/KpgShiftForm.razor` |
| Edición | Diálogo libre (usuario y aprobador) | `KpgEditDescripcionDialog.razor` |
| Sugerencias recientes | Copian la descripción tal cual (propagan las malas) | `KpgShiftForm.HandleSuggestionSelected` |
| Otras entradas | Dictado por voz (Claude), importación Excel | `ClaudeInterpreteVoz.cs`, `Aprobaciones/ImportarTimesheet.cs` |

## Alcance por fases

| Fase | Contenido |
|---|---|
| **0** | Análisis de las descripciones existentes para armar la lista inicial |
| **1** | Catálogo de términos + reglas base + validación backend + asistente en el formulario (solo advertir) |
| **2** | Marca de calidad para el aprobador + reporte · Botón "Mejorar redacción" con IA · Chips de verbos |

Principio rector: **la Fase 1 arranca en modo Advertir**. Nada bloquea el guardado hasta que
los datos confirmen qué términos conviene bloquear; el cambio se hace desde el catálogo, sin
desplegar código.

---

## Fase 0 — Línea base (solo lectura)

Consultas sobre `RegistrosHoras` (SQL Server) para decidir la lista inicial y los umbrales:

```sql
-- Descripciones más repetidas (candidatas a lista negra)
SELECT TOP 100 LOWER(LTRIM(RTRIM(Descripcion))) AS Descripcion, COUNT(*) AS Veces
FROM RegistrosHoras GROUP BY LOWER(LTRIM(RTRIM(Descripcion))) ORDER BY Veces DESC;

-- Distribución por cantidad de palabras (para fijar MinPalabras)
SELECT Palabras, COUNT(*) AS Registros FROM (
  SELECT LEN(Descripcion) - LEN(REPLACE(Descripcion, ' ', '')) + 1 AS Palabras
  FROM RegistrosHoras) t
GROUP BY Palabras ORDER BY Palabras;

-- Las más cortas, para ver ejemplos reales
SELECT TOP 100 Descripcion FROM RegistrosHoras ORDER BY LEN(Descripcion);
```

**Entregable:** lista semilla de 20–40 términos, cada uno con regla, sugerencia y motivo, más
los umbrales `MinPalabras` y `MinPalabrasContexto` definidos con datos reales.

**✅ Ejecutada el 2026-09-23** contra la base Azure (única con credenciales activas; ver
decisión del usuario). Resultado completo, incluida la advertencia sobre el volumen de datos
(146 registros, mayormente seed/QA) y la lista semilla lista para la migración de 10.1:
[Docs/fase0-linea-base-descripciones.md](fase0-linea-base-descripciones.md).

---

## Fase 1 — Implementación base

### 1.1 Dominio y datos · Tamaño M ✅ implementado (2026-09-23)

> **Nota sobre el mecanismo real de despliegue del esquema:** este proyecto no aplica
> `dotnet ef database update`. El snapshot commiteado en
> `Backend/src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs` está
> desactualizado respecto al esquema real (le faltan cambios de migraciones anteriores) — un
> `dotnet ef migrations add` normal arrastra ese drift y casi corrompe el snapshot al
> intentar revertirlo. El esquema se aplica con `EnsureCreatedAsync()` en bases nuevas y se
> parcha con bloques SQL idempotentes (`IF OBJECT_ID(...) IS NULL BEGIN CREATE TABLE...
> END`) en `EnsureTimesheetTablesAsync()`. La tabla `TerminosDescripcion` se agregó
> siguiendo esa misma convención, no como migración EF.
>
> El `CREATE TABLE` ya se probó (dos veces, confirmando que es idempotente) contra la base
> de Azure `free-sql-db-9398472` — la única con credenciales activas en el repo. La semilla
> de 17 términos y 4 parámetros queda en código, lista para poblarse sola la próxima vez que
> la API arranque contra esa base (o cualquier otra).
>
> **✅ Snapshot reparado (2026-09-25).** Se confirmó primero que nada en el proyecto llama a
> `Database.Migrate()` (ni `MigrateAsync`) — el historial de migraciones de EF nunca se
> aplica a una base real, así que generar una migración que capture el drift no tiene efecto
> en producción, solo repara la herramienta. Se generó
> `20260925002721_SincronizarSnapshotConModeloActual` (squash de 6 tablas que faltaban en
> el snapshot: `AprobacionesRegistro`, `ParametrosRestriccionDia`,
> `ReglasVentanaRetroactividad`, `ReportesUsuario`, `SupervisoresPuesto` y
> `TerminosDescripcion`, más las columnas de horarios/proyecto ya renombradas en
> `RegistrosHoras` y `SolicitudesExcepcion`) y se verificó generando una migración de prueba
> aparte: salió con `Up`/`Down` completamente vacíos, confirmando que el snapshot ya
> coincide con el modelo actual. `dotnet ef migrations add` vuelve a funcionar normal para
> este repo desde ahora — solo hay que recordar que ese archivo no se aplica solo a ninguna
> base; sigue haciendo falta el bloque SQL idempotente en `EnsureTimesheetTablesAsync()`
> para bases ya existentes, igual que antes.


**Entidad nueva `TerminoDescripcion : BaseAuditableEntity`** (mismo patrón que `Modalidad`):

| Propiedad | Tipo | Notas |
|---|---|---|
| `Termino` | string ≤ 100 | Texto tal como lo escribe el admin |
| `TerminoNormalizado` | string ≤ 100 | Minúsculas, sin tildes, espacios colapsados. **Índice único** |
| `Tipo` | enum `Palabra` / `Frase` | Sin regex editable por el admin (evita ReDoS y errores de patrón) |
| `Regla` | enum `ProhibidoSiempre` / `GenericoSiVaSolo` | Ver lógica abajo |
| `Severidad` | enum `Advertir` / `Bloquear` | Arranca todo en `Advertir` |
| `Sugerencia` | string ≤ 300, opcional | Puede llevar marcadores: `[App] – Reunión de levantamiento de [elemento] con [área]` |
| `Motivo` | string ≤ 200 | Se muestra al usuario |
| `Activo` | bool | Activar / desactivar como los demás catálogos |

- Configuración EF + migración en `Backend/src/Infrastructure/Migrations`.
- Semilla en `ApplicationDbContextInitialiser` con la lista de Fase 0.
- **Parámetros nuevos en `ParametroSistema`** (constantes en `Domain.Constants.ParametrosSistema`):
  - `Descripcion.ValidacionActiva` (bool, default `true`): interruptor general.
  - `Descripcion.MinPalabras` (int, default según Fase 0, ~5).
  - `Descripcion.MinPalabrasContexto` (int, default 3): palabras "con contenido" necesarias para
    que un término `GenericoSiVaSolo` no se marque.
  - `Descripcion.SeveridadReglasBase` (`Advertir` / `Bloquear`, default `Advertir`).

### 1.2 Evaluador (núcleo, sin dependencias) · Tamaño M ✅ implementado (2026-09-23)

> Implementado en `Application/Common/Services/EvaluadorDescripcion.cs`, tal como se
> describe abajo, con 22 pruebas unitarias en
> `Application.UnitTests/Common/Services/EvaluadorDescripcionTests.cs` (todas en verde).
> Durante las pruebas se encontró y corrigió un caso real: el conteo de "palabras de
> contexto" tokenizaba por espacios y un `-` suelto (el separador del formato
> `[Proyecto] – [Acción]`) se contaba como si fuera una palabra de contexto, lo que podía
> dejar pasar descripciones genéricas por error. Se corrigió tokenizando solo con
> letras/números.


Servicio puro en `Application/Common/Services/EvaluadorDescripcion.cs`, fácil de probar:

```
Evaluar(texto, terminosActivos, parametros, nombreProyecto?) -> EvaluacionDescripcion
EvaluacionDescripcion { IReadOnlyList<Hallazgo> Hallazgos; bool Bloquea; }
Hallazgo { Codigo, Severidad, Mensaje, Fragmento, Sugerencia? }
```

**Normalización:** minúsculas, quitar tildes (`NormalizationForm.FormD` y descartar
`NonSpacingMark`), colapsar espacios. Las coincidencias respetan límites de palabra, así que
"soporte" no coincide dentro de "soportes".

**Reglas del catálogo:**
- `ProhibidoSiempre`: se marca si el término aparece en cualquier parte.
- `GenericoSiVaSolo`: se marca solo si, al quitar el término, las stopwords (de, la, el, en,
  con, para…) y el nombre del proyecto, quedan menos de `MinPalabrasContexto` palabras.
  - `"Reunión"` → se marca.
  - `"CIMA – Reunión de levantamiento de requerimientos del módulo de evidencias"` → no se marca.

**Reglas base (en código, severidad = `SeveridadReglasBase`):**

| Código | Condición |
|---|---|
| `MUY_CORTA` | Menos de `MinPalabras` palabras, sin contar el prefijo del proyecto |
| `MAYUSCULAS` | Más de 10 letras y todas en mayúscula |
| `REPETIDOS` | El mismo carácter 4 o más veces seguidas (`aaaa`, `....`) |
| `SIN_LETRAS` | No contiene letras |
| `MARCADOR_SIN_LLENAR` | Quedaron marcadores `[…]` de una sugerencia aplicada |

`Bloquea = true` si algún hallazgo tiene severidad `Bloquear` y `ValidacionActiva` es true.

### 1.3 Integración backend · Tamaño M ✅ implementado (2026-09-23), salvo CRUD del catálogo (10.4)

> Implementado: `IValidadorDescripcion` + `ValidadorDescripcionService` (carga catalogo y
> parametros desde la base, resuelve el nombre del proyecto por `ProyectoId`), la regla
> compartida `DescripcionValida()` de FluentValidation, y un validador dedicado (con acceso
> a datos, separado del validador de forma) para cada uno de los 3 comandos. El endpoint
> `POST /api/descripciones/evaluar` ya existe para el aviso en vivo. La importación de Excel
> ya no puede rechazar una fila por su descripción: agrega `Advertencias` al resultado sin
> tocar el resto del comportamiento.
>
> 47 pruebas nuevas (22 del evaluador + 9 del validador/servicio + 2 de importación, más las
> de cableado de los 3 validadores), toda la suite en verde: 53/53 Application,
> 125/125 Domain, 319/319 Infrastructure.IntegrationTests.
>
> Los eventos de bitácora para crear/editar/activar términos del catálogo se agregan en la
> story 10.4, junto con los comandos de administración que los disparan — no hay nada que
> loggear todavía porque el catálogo aún no tiene CRUD.
>
> **✅ Frontend actualizado (2026-09-25).** `AprobacionModels.cs` ganó
> `FilaAdvertenciaResponse` y el campo `Advertencias` en `ImportacionResultadoResponse`.
> `ImportarTimesheetPage.razor` muestra un chip "con observaciones de descripción" en el
> resumen y, más abajo, una sección con cada fila y sus avisos — separada de "Omitidas" y
> "Errores", porque estas filas **sí se importaron**; es solo un aviso de calidad, no un
> rechazo.



- **`IValidadorDescripcion`** (Application): carga los términos activos y los parámetros y llama
  al evaluador. La tabla es pequeña, así que alcanza con una consulta por guardado; se agrega
  caché solo si hace falta.
- **Regla FluentValidation compartida**, por ejemplo `.DescripcionValida(validador)` con
  `MustAsync`. Solo falla con hallazgos `Bloquear` y el mensaje incluye la sugerencia. Se
  aplica en:
  - `CreateRegistroHorasCommandValidator`
  - `CreateRegistrosRangoCommandValidator`
  - `UpdateDescripcionRegistroHorasCommandValidator`
- La entidad `RegistroHoras` **no cambia**: el dominio sigue validando solo vacío y longitud,
  porque las reglas de calidad dependen del catálogo.
- **Importación Excel:** no rechaza filas. Se agrega `Advertencias` a `ImportacionResultadoDto`
  con las filas cuya descripción tiene hallazgos (es carga histórica y la hace un admin).
- **Endpoints nuevos** (patrón `IEndpointGroup`, como `Modalidades.cs`):

| Método | Ruta | Rol | Uso |
|---|---|---|---|
| POST | `/api/descripciones/evaluar` | Cualquiera autenticado | `{ texto, proyectoId? }` → `EvaluacionDescripcionDto` (aviso en vivo) |
| GET | `/api/terminos-descripcion` | Admin | Listado completo |
| POST | `/api/terminos-descripcion` | Admin | Crear |
| PUT | `/api/terminos-descripcion/{id}` | Admin | Editar |
| PUT | `/api/terminos-descripcion/{id}/toggle` | Admin | Activar / desactivar |

- **Bitácora:** nuevos `TipoEventoBitacora` para crear, editar y activar términos y para el
  cambio de parámetros (se cambian reglas que afectan a todos).

> **Por qué un endpoint para evaluar y no la lógica en el cliente:** el frontend (Blazor WASM)
> y el backend son soluciones separadas. Evaluar en el servidor deja una sola implementación y
> garantiza que el aviso en vivo y la validación al guardar den siempre el mismo resultado.

### 1.4 Administración · Tamaño S–M ✅ implementado (2026-09-23)

> Backend: CRUD completo de `TerminoDescripcion` (`Application/Features/Catalogos/TerminosDescripcion`)
> con los mismos endpoints propuestos arriba, más `GetParametrosDescripcionQuery` /
> `UpdateParametrosDescripcionCommand` bajo `/api/descripciones/parametros` para los 4
> parámetros. Los 4 eventos de bitácora (crear/editar/activar término, cambio de
> parámetros) que quedaron pendientes de la 10.3 ya están y se disparan desde estos
> handlers. 9 pruebas nuevas, toda la suite en verde: 53/53 Application, 125/125 Domain,
> **328/328** Infrastructure.IntegrationTests.
>
> Frontend: página `DescripcionesAdminPage.razor` en `/admin/parametros/descripciones`
> (enlace nuevo en el menú, bajo Parámetros), con tabla CRUD + toggle activo/inactivo
> (mismo patrón que `ModalidadesAdminPage`), sección de los 4 parámetros generales, y un
> **probador** en vivo que llama a `/api/descripciones/evaluar` con debounce de 500ms.
> Repositorio `TerminoDescripcionRepository` + modelos con los 3 enums espejados
> (`TipoTermino`, `ReglaTermino`, `SeveridadTermino`, mismo orden que el backend para que
> el JSON por posición numérica coincida). Claves de recursos agregadas en español e inglés.
>
> **✅ Selector de proyecto agregado al probador (2026-09-25).** Cliente + Proyecto
> (reusando `IClienteRepository.GetCatalogoAsync()`, mismo catálogo que usa `KpgShiftForm`),
> opcionales. Con proyecto elegido, `ProbarAsync` pasa su `ProyectoId` a
> `/api/descripciones/evaluar`, así el admin puede ejercer el prefijo `[Proyecto] – ` y el
> descuento de su nombre como contexto, no solo términos y reglas base sueltos.

- Página nueva `Features/Admin/Pages/DescripcionesAdminPage.razor` ("Calidad de descripciones"),
  con el mismo patrón que `ModalidadesAdminPage`:
  - Tabla de términos (término, regla, severidad, sugerencia, activo) con crear, editar y
    activar/desactivar.
  - Sección de parámetros: validación activa, mínimo de palabras, palabras de contexto y
    severidad de las reglas base.
  - **Probador:** un cuadro de texto que llama a `/evaluar`, para que el admin vea el efecto de
    un término antes de activarlo.
- Entrada en el menú de administración y claves de texto en `SharedResource.resx` y
  `SharedResource.en.resx` (según la política de idioma, `Docs/decisions/2026-05-14-...`).

### 1.5 Asistente en el formulario · Tamaño M–L ✅ implementado (2026-09-23)

> `Shared/Components/KpgDescripcionField.razor` reemplaza el `MudTextField` suelto en
> **`KpgShiftForm`** (formulario de registro) y en **`KpgEditDescripcionDialog`** (edición,
> usada desde `HistorialPage` y `RevisionPage`). Incluye:
> - Texto guía con el formato de la guía + enlace "Ver ejemplos" con los 3 ejemplos del PDF.
> - Prefijo automático `"{Proyecto} – "` cuando se elige proyecto con la descripción vacía.
> - Aviso en vivo contra `/api/descripciones/evaluar`, con debounce de 600ms — mismo
>   endpoint y misma lógica que usa el backend al guardar, así nunca se contradicen.
> - Botón **"Aplicar sugerencia"** por hallazgo, que reemplaza el fragmento marcado (o
>   agrega la sugerencia al final si el usuario ya lo había borrado).
> - Región `aria-live="polite"` para los hallazgos, foco automático al abrir el diálogo.
>
> **Bug encontrado y corregido en el camino:** al reusar una "sugerencia reciente" o al
> dictar por voz, el texto se actualizaba desde afuera del campo (no a través de
> `ValueChanged`), así que el aviso en vivo no se disparaba hasta que el usuario tocara el
> campo a mano. Se corrigió detectando también los cambios de `Value` que llegan por
> parámetro, no solo los que escribe el usuario.
>
> El diálogo de edición ahora recibe `ProyectoId` desde ambas pantallas que lo abren
> (`RevisionPage.ProyectoId` de `RegistroPendienteResponse`, `HistorialPage.ProyectoId` de
> `HistorialRegistroResponse`), para que el evaluador descuente el nombre del proyecto del
> conteo de contexto también al editar, no solo al crear.
>
> Backend y frontend compilan sin errores.


Componente nuevo `Shared/Components/KpgDescripcionField.razor` que reemplaza el `MudTextField`
en **`KpgShiftForm`** y en **`KpgEditDescripcionDialog`** (así el aprobador también lo usa):

- **Texto guía** con el formato de la guía y un enlace **"Ver ejemplos"** (popover con los 3
  ejemplos del PDF).
- **Prefijo automático:** al elegir proyecto con la descripción vacía, se precarga
  `"{Proyecto} – "`. Es editable, y el evaluador no lo cuenta como palabras.
- **Aviso en vivo:** llama a `/evaluar` con un debounce de ~600 ms y muestra los hallazgos
  debajo del campo:
  - `Advertir` → texto ámbar: *"'Soporte' es muy genérico. Sugerencia: …"*
  - `Bloquear` → error rojo y botón Guardar deshabilitado.
  - Botón **"Aplicar sugerencia"**: reemplaza el fragmento por la sugerencia y posiciona el
    cursor en el primer marcador `[…]`.
- **Sugerencias recientes:** no hace falta lógica extra, porque al copiar una descripción el
  aviso en vivo la evalúa igual.
- **Voz:** el texto dictado cae en el mismo campo y pasa por el mismo aviso.
- Accesibilidad: hallazgos en una región `aria-live="polite"` y el botón de sugerencia con
  etiqueta descriptiva (en línea con la story 2.7).

### 1.6 Pruebas · Tamaño M

- **Unitarias del evaluador** (`Application.UnitTests`), la parte más importante:
  - Tildes y mayúsculas (`Reunion` = `reunión`), límites de palabra (`soporte` vs `soportes`).
  - `GenericoSiVaSolo`: solo vs con contexto; el prefijo del proyecto no cuenta.
  - Reglas base: corta, mayúsculas, repetidos, sin letras, marcadores sin llenar.
  - `ValidacionActiva = false` → nunca bloquea.
- **Validadores:** `Bloquear` falla y `Advertir` pasa, en los 3 comandos. Se actualiza
  `CreateRegistroHorasCommandValidatorTests`.
- **Integración:** endpoints de catálogo (roles 403/404), `/evaluar`, importación con
  advertencias, y los tests existentes de `UpdateDescripcion` en verde.
- **QA visual** del formulario y del diálogo en escritorio y móvil.

### 1.7 Salida a producción

1. Desplegar con todo en `Advertir` y comunicar la guía al equipo (enlace "Ver ejemplos").
2. Medir 2–4 semanas: porcentaje de registros con hallazgos por término y por regla base
   (con la consulta de Fase 0 o con el reporte de Fase 2A).
3. Pasar a `Bloquear` los términos con más incidencia y menos falsos positivos, desde el
   catálogo y sin desplegar.

---

## Fase 2 — Después de medir

### 2A. Marca para el aprobador + reporte · Tamaño M

**Chip para el aprobador ✅ implementado (2026-09-24).** `RegistroPendienteDto` incluye
`TieneObservacionesDescripcion` (bool), evaluado al leer, no guardado — así siempre refleja
el catálogo vigente, incluso sobre registros históricos. Para no golpear la base fila por
fila, `IValidadorDescripcion` ganó `EvaluarVariasAsync(items)`, que carga el catálogo y los
parámetros **una sola vez** por página de resultados y resuelve los nombres de todos los
proyectos distintos en una sola consulta. `AprobacionesRepository.GetPendientesAsync` la usa
sobre el lote completo. En `RevisionPage.razor` aparece un chip **"Revisar descripción"**
junto al texto, con tooltip, cuando hay hallazgos. 5 pruebas nuevas (todas en verde):
332/332 Infrastructure.IntegrationTests.

**Reporte ✅ implementado (2026-09-24).** En vez de una página nueva, se sumó a
**"Reporte de Horas"** (`/reportes/horas`), que ya tenía filtros por empleado, cliente,
proyecto y recurso, exportación a Excel/PDF y paginación — reusar esa pantalla es más simple
y consistente que duplicar un reporte paralelo.

- `GetReporteHorasQuery` y `ExportarReporteHorasQuery` ganaron `SoloConObservaciones` (bool).
- `ReporteHorasItemDto` ganó `TieneObservacionesDescripcion`, calculado al leer con
  `IValidadorDescripcion.EvaluarVariasPorNombreProyectoAsync` (variante de
  `EvaluarVariasAsync` para consultas Dapper que ya traen el nombre del proyecto en la fila,
  sin resolverlo de vuelta a un Id).
- **Camino normal** (sin el filtro activo): igual de rápido que antes — solo evalúa la
  página que se muestra (máx. 100 filas), la paginación sigue resuelta en SQL.
- **Con "Solo con observaciones" activo**: filtrar por calidad no se puede hacer en SQL (la
  calidad depende del catálogo, no de una columna), así que se trae todo lo que entra en los
  demás filtros (tope de 3000 filas), se evalúa una sola vez, se filtra y se pagina en
  memoria sobre el subconjunto ya filtrado. Es un modo más costoso, pero opt-in.
- En pantalla: switch **"Solo con observaciones de calidad"** + columna con ícono de aviso.
- En Excel: columna **"Observaciones"** (Sí / vacío, resaltada). En PDF: columna **"Obs."**.
- 12 pruebas nuevas para `EvaluarVariasAsync`/`EvaluarVariasPorNombreProyectoAsync`. No hay
  pruebas de integración para `ReportesRepository`/`ExportarReporteHorasQueryHandler` en sí
  (usan Dapper contra SQL Server real) — coincide con que **ningún** reporte de este proyecto
  tiene esa cobertura hoy, no es una brecha nueva.

Evaluar al leer (en vez de guardar una columna) funciona también hacia atrás y siempre refleja
el catálogo vigente. Si el volumen lo pide, se guarda una columna más adelante.

### 2B. "Mejorar redacción" con IA · Tamaño M ✅ implementado (2026-09-24)

> `POST /api/descripciones/mejorar` `{ texto, proyectoId }` →
> `{ disponible, propuesta, cambios[], propuestaBloquea, observacionesPropuesta[] }`, más
> `GET /api/descripciones/mejorar/disponible` (mismo patrón que `/api/voz/disponible`, para
> que el frontend oculte el botón sin tener que llamar a "mejorar" solo para saberlo).
>
> **Backend:** `ClaudeRedactorDescripcion` (Infrastructure/Descripciones), reutilizando la
> `ApiKey` de `AnthropicSettings` (misma cuenta que el dictado de voz) y el patrón de salida
> estructurada de `ClaudeInterpreteVoz`, pero con su **propio modelo** —
> `claude-haiku-4-5-20251001` por defecto, configurable en `AnthropicRedactorDescripcion` en
> appsettings— porque reescribir una frase corta con formato fijo no necesita el modelo más
> grande del dictado. El prompt incluye el formato, los 3 ejemplos de la guía, el
> cliente/proyecto y los hallazgos ya detectados, con una regla explícita de **no inventar
> hechos** que el usuario no haya mencionado (deja marcadores `[así]` en su lugar).
> `MejorarDescripcionCommandHandler` reevalúa la propuesta con el mismo
> `IValidadorDescripcion` antes de devolverla, y aplica el **límite diario por usuario**
> (`LimiteDiarioPorUsuario`, default 20) contando eventos `MejoraDescripcionSolicitada` en
> `BitacoraAuditoria` — reutiliza la bitácora existente en vez de agregar infraestructura
> nueva de rate-limiting.
>
> **Bug real encontrado y corregido en el camino:** el corte de "hoy" para el límite diario
> se calculaba con `new DateTimeOffset(clock.Today.ToDateTime(...))`, que interpreta un
> `DateTime` sin zona horaria como **hora local del servidor**, mientras que
> `BitacoraAuditoria.Timestamp` siempre se guarda en UTC — en un servidor con zona horaria
> negativa (detrás de UTC) esto podía descartar usos de "hoy" o contar usos de "ayer" de
> más, según la hora del día. Corregido usando `clock.UtcNow.UtcDateTime.Date` de forma
> consistente. Lo detectó un test.
>
> **Frontend:** botón "Mejorar con IA" en `KpgDescripcionField` (oculto si `!_iaDisponible`
> o mientras se escribe), tarjeta de propuesta con el resumen de cambios y **Usar esta
> versión / Descartar** — nunca reemplaza el texto sola. Si la propuesta igual queda con
> hallazgos, se avisa antes de aceptarla.
>
> 7 pruebas nuevas para el handler (disponible=false, propuesta exitosa, propuesta que
> sigue bloqueada, sin propuesta del modelo, bitácora, límite diario alcanzado, usos de
> ayer no cuentan). Suite completa en verde: 342/342 Infrastructure.IntegrationTests,
> 53/53 Application, 125/125 Domain. Backend y frontend compilan sin errores.

### 2C. Chips de verbos técnicos · Tamaño S ✅ implementado (2026-09-24)

> `KpgDescripcionField` (Shared/Components) suma una fila de chips con los 8 verbos:
> Ajuste, Corrección, Implementación, Optimización, Validación, Análisis, Pruebas,
> Documentación — lista fija en código, no vale la pena un catálogo editable para 8
> palabras que casi no cambian.
>
> **Decisión de diseño (desviación del plan original):** en vez de insertar en la posición
> del cursor, el chip **agrega el verbo al final** del texto actual (con el separador
> `– ` del formato de la guía). Insertar en la posición del cursor requeriría interop de
> JS para leer la selección del `<textarea>` de MudBlazor *antes* de que el clic del chip
> le quite el foco — complejidad no justificada para una story de tamaño S, dado que al
> hacer clic en un chip el usuario casi siempre está escribiendo al final de todas formas.
> Backend y frontend compilan sin errores.

---

## Orden de trabajo sugerido (stories)

| Story | Contenido | Depende de |
|---|---|---|
| 10.0 | ✅ Análisis de la línea base y lista semilla | — |
| 10.1 | ✅ Entidad, migración, semilla, parámetros | 10.0 |
| 10.2 | ✅ Evaluador + pruebas unitarias | 10.1 |
| 10.3 | ✅ Validación en comandos, endpoint `/evaluar`, advertencias de importación (bitácora movida a 10.4) | 10.2 |
| 10.4 | ✅ CRUD admin, página de administración y probador | 10.3 |
| 10.5 | ✅ `KpgDescripcionField` en el formulario y el diálogo | 10.3 |
| 10.6 | Salida a producción en modo Advertir y medición | 10.4, 10.5 |
| 10.7 | ✅ (Fase 2A) Marca en revisión + reporte | 10.6 |
| 10.8 | ✅ (Fase 2B) Mejorar redacción con IA | 10.6 |
| 10.9 | ✅ (Fase 2C) Chips de verbos | 10.5 |

Las stories 10.4 y 10.5 pueden avanzar en paralelo.

## Decisiones abiertas

1. ¿Al guardar con advertencias se pide confirmación ("¿Guardar de todos modos?") o basta con el
   aviso en línea? Recomendación: solo el aviso en línea en la Fase 1.
2. ¿Quién mantiene el catálogo: solo el Admin o también Gerente o líder de proyecto?
   Recomendación: solo Admin.
3. ¿La edición del aprobador aplica las mismas reglas de bloqueo? Recomendación: sí, es el
   mismo validador.
4. ¿El prefijo automático usa el nombre del proyecto o se crea un catálogo de App/Módulo por
   proyecto? Recomendación: el nombre del proyecto en la Fase 1 y evaluarlo después.

## Riesgos

| Riesgo | Mitigación |
|---|---|
| Falsos positivos que frustran al usuario | Arrancar en Advertir, regla `GenericoSiVaSolo` y el probador del admin |
| La gente "rellena" palabras para pasar el mínimo | La calidad real la cuida el aprobador (2A); las reglas solo filtran lo evidente |
| Latencia del aviso en vivo | Debounce y un endpoint liviano (el evaluador es en memoria) |
| Descripciones históricas quedan marcadas | Esperado: la evaluación al leer permite medir la línea base, y no se tocan registros existentes |
