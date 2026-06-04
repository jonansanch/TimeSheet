# Story 8.3: Integración UX — Botón de Micrófono en KpgShiftForm

Status: complete

## Story

As a empleado,
I want ver un botón de micrófono en el formulario de registro y ver los campos llenarse automáticamente tras dictar,
So that el registro sea más rápido y cómodo sin perder el control de revisar antes de guardar.

## Acceptance Criteria

1. **Given** el formulario de registro cargado, **When** el usuario lo ve, **Then** aparece un `MudIconButton` con ícono de micrófono en el header del formulario **And** muestra tooltip "Dictar registro" en estado idle.

2. **Given** estado idle, **When** el usuario hace clic en el botón, **Then** el botón cambia a estado "escuchando" con ícono rojo y animación de pulso **And** el micrófono del navegador se activa vía `VoiceInputService`.

3. **Given** estado escuchando, **When** el reconocimiento finaliza (usuario dejó de hablar), **Then** el botón cambia a estado "procesando" con `MudProgressCircular` **And** se invoca `VoiceParser.Parse` con el transcript y el catálogo cargado en el formulario.

4. **Given** resultado del `VoiceParser` con campos detectados, **When** el procesamiento termina, **Then** los campos detectados se actualizan en el formulario automáticamente **And** el botón vuelve a estado idle.

5. **Given** campos no detectados en `VoiceParseResult.CamposNoDetectados`, **When** el formulario se actualiza, **Then** aparece un `MudAlert` Severity.Warning listando los campos pendientes de completar **And** el alert se oculta al comenzar a editar cualquier campo manualmente.

6. **Given** campos ya llenados por dictado previo, **When** el usuario dicta nuevamente, **Then** los campos se sobreescriben con el nuevo resultado del parser (solo los detectados; los no detectados se limpian).

7. **Given** el botón en estado escuchando, **When** el usuario hace clic nuevamente, **Then** el reconocimiento se cancela vía `VoiceInputService.StopAsync` **And** el botón vuelve a idle sin modificar campos.

8. **Given** navegador no compatible o error de micrófono, **When** ocurre la condición, **Then** el `MudAlert` describe el problema **And** el botón vuelve a idle.

## Tasks / Subtasks

- [ ] **T1 — Localizar y leer `KpgShiftForm.razor`** (prerequisito)
  - [ ] Leer el archivo para entender la estructura actual del header y los bindings de campos
  - [ ] Identificar los campos: `_fecha`, `_horaEntradaAM`, `_horaSalidaAM`, `_horaEntradaPM`, `_horaSalidaPM`, `_cliente`, `_proyecto`, `_modalidad`, `_recurso`, `_descripcion`, `_lugar` (o equivalentes)
  - [ ] Identificar cómo está cargado el catálogo (listas de clientes, proyectos, modalidades, recursos, lugares)

- [ ] **T2 — Agregar estado de voz al componente** (AC: 1, 2, 3)
  - [ ] Definir enum privado `VoiceState { Idle, Listening, Processing }`
  - [ ] Agregar campo `private VoiceState _voiceState = VoiceState.Idle`
  - [ ] Agregar campo `private string? _voiceAlertMessage`
  - [ ] Agregar campo `private List<string> _camposNoDectectados = []`
  - [ ] Inyectar `VoiceInputService` y `VoiceParser` en el componente (`[Inject]`)
  - [ ] Crear `DotNetObjectReference` para el callback JS: `_dotNetRef = DotNetObjectReference.Create(this)`
  - [ ] Implementar `[JSInvokable] OnTranscriptReceived(string transcript)` → cambia estado a Processing, llama al parser
  - [ ] Implementar `[JSInvokable] OnVoiceError(string error)` → muestra alert descriptivo, vuelve a Idle
  - [ ] Implementar `[JSInvokable] OnRecognitionEnded()` → si estado sigue Listening (sin transcript), vuelve a Idle
  - [ ] Disponer `_dotNetRef` en `IAsyncDisposable.DisposeAsync`

- [ ] **T3 — Agregar botón de micrófono en el header del formulario** (AC: 1, 2, 7)
  - [ ] En la sección de header de `KpgShiftForm.razor`, agregar `MudTooltip` + `MudIconButton`:
    - Idle: `Icons.Material.Filled.Mic`, Color.Default
    - Listening: `Icons.Material.Filled.MicOff` o `Icons.Material.Filled.GraphicEq`, Color.Error, con clase CSS `kpg-voice-pulse`
    - Processing: reemplazar ícono por `MudProgressCircular` Size.Small
  - [ ] `OnClick` → si Idle: llamar `StartListeningAsync()`; si Listening: llamar `StopListeningAsync()`
  - [ ] Deshabilitar botón si navegador no compatible (verificar en `OnInitializedAsync`)

- [ ] **T4 — Implementar lógica de inicio/parada** (AC: 2, 7)
  - [ ] `StartListeningAsync()`:
    - Limpiar `_camposNoDetectados` y `_voiceAlertMessage`
    - Cambiar estado a Listening
    - Llamar `VoiceInputService.StartAsync(_dotNetRef)`
  - [ ] `StopListeningAsync()`:
    - Llamar `VoiceInputService.StopAsync()`
    - Cambiar estado a Idle

- [ ] **T5 — Implementar llenado de campos desde VoiceParseResult** (AC: 4, 5, 6)
  - [ ] En `OnTranscriptReceived`: construir `VoiceCatalog` con el catálogo cargado en el formulario
  - [ ] Llamar `VoiceParser.Parse(transcript, catalogo)` → obtener `VoiceParseResult`
  - [ ] Asignar cada campo no-null del resultado al binding correspondiente del formulario
  - [ ] Asignar `_camposNoDetectados = result.CamposNoDetectados`
  - [ ] Si `_camposNoDetectados.Any()`, mostrar `MudAlert` con la lista
  - [ ] Cambiar estado a Idle y llamar `StateHasChanged()`
  - [ ] Al editar cualquier campo manualmente: ocultar el alert (`_camposNoDetectados = []`)

- [ ] **T6 — Agregar CSS de animación de pulso** (AC: 2)
  - [ ] En `Fronted/src/WebUI/wwwroot/css/app.css` (o archivo CSS del componente), agregar:
    ```css
    .kpg-voice-pulse {
        animation: kpg-pulse 1s ease-in-out infinite;
    }
    @keyframes kpg-pulse {
        0%, 100% { opacity: 1; }
        50% { opacity: 0.4; }
    }
    ```

- [ ] **T7 — Verificar en navegador** (AC: 1–8)
  - [ ] Arrancar frontend local (`dotnet run --project Fronted/src/WebUI/...`)
  - [ ] Verificar en Chrome: dictado completo → campos llenados → alert de campos faltantes
  - [ ] Verificar en Edge: mismo flujo
  - [ ] Verificar en Firefox: botón deshabilitado y mensaje de advertencia visible
  - [ ] Verificar dictado doble (sobreescritura de campos)
  - [ ] Verificar cancelación del dictado con segundo clic

## Dev Notes

- Dependencias de esta story: Story 8.1 (VoiceInputService) y Story 8.2 (VoiceParser) deben estar completas.
- El catálogo ya está cargado en el formulario desde la API al inicializar el componente; no hay llamada adicional.
- `DotNetObjectReference` debe disponerse en `DisposeAsync` para evitar memory leaks en el GC de .NET WASM.
- Usar `await InvokeAsync(StateHasChanged)` en los callbacks [JSInvokable] ya que vienen de un hilo de JS.
- No hay cambios al backend en esta story.

## Files To Create / Modify

| Acción | Archivo |
|--------|---------|
| Modificar | `Fronted/src/WebUI/Pages/RegistroPage.razor` o `Components/KpgShiftForm.razor` |
| Modificar | `Fronted/src/WebUI/wwwroot/css/app.css` |

## Phrase Examples for Testing

El usuario puede decir algo como:

> "Hoy, Banco Nacional, Core Bancario, de 8 a 12 en la mañana, de 1 a 5 en la tarde, presencial, desarrollador, oficina, descripción: análisis del módulo de pagos internacionales"

Resultado esperado:
- Fecha = hoy
- AM: 08:00 – 12:00
- PM: 13:00 – 17:00
- Cliente = Banco Nacional
- Proyecto = Core Bancario
- Modalidad = Presencial
- Recurso = Desarrollador
- Lugar = Presencial Oficina
- Descripción = análisis del módulo de pagos internacionales
