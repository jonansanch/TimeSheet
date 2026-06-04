# Story 8.1: Web Speech API — Captura de Voz vía JS Interop

Status: complete

## Story

As a empleado,
I want que la aplicación pueda escuchar mi voz mediante el micrófono del navegador,
So that el sistema reciba el texto transcrito para procesarlo sin depender de servicios externos.

## Acceptance Criteria

1. **Given** Chrome o Edge con micrófono disponible, **When** el usuario activa el botón de voz, **Then** el navegador solicita permiso de micrófono si aún no fue otorgado **And** comienza a escuchar en español colombiano (es-CO).

2. **Given** el usuario termina de hablar, **When** el reconocimiento de voz finaliza, **Then** el transcript en texto llega al componente Blazor vía callback `[JSInvokable]` **And** el texto se entrega completo (no parcial) para su procesamiento.

3. **Given** Firefox u otro navegador sin soporte de Web Speech API, **When** el usuario intenta activar el micrófono, **Then** se muestra un MudAlert de advertencia indicando que se requiere Chrome o Edge **And** el botón queda deshabilitado.

4. **Given** permiso de micrófono denegado por el usuario, **When** ocurre el error del navegador, **Then** se muestra un mensaje descriptivo orientando al usuario a revisar los permisos del navegador **And** el sistema vuelve al estado idle sin lanzar excepción no controlada.

## Tasks / Subtasks

- [ ] **T1 — Crear `voiceInput.js`** (AC: 1, 2, 3, 4)
  - [ ] Crear `Fronted/src/WebUI/wwwroot/js/voiceInput.js`
  - [ ] Implementar `window.voiceInput.isSupported()` → retorna `true` si `SpeechRecognition` o `webkitSpeechRecognition` está disponible
  - [ ] Implementar `window.voiceInput.start(dotNetRef, lang)`:
    - Crear instancia `SpeechRecognition` con `lang`, `continuous: false`, `interimResults: false`
    - Al evento `onresult`: llamar `dotNetRef.invokeMethodAsync('OnTranscriptReceived', transcript)`
    - Al evento `onerror`: llamar `dotNetRef.invokeMethodAsync('OnVoiceError', error.error)`
    - Al evento `onend`: llamar `dotNetRef.invokeMethodAsync('OnRecognitionEnded')`
  - [ ] Implementar `window.voiceInput.stop()` → llama `recognition.stop()` si existe instancia activa
  - [ ] Limpiar instancia al parar para evitar memory leaks

- [ ] **T2 — Registrar script en index.html** (AC: 1)
  - [ ] Agregar `<script src="js/voiceInput.js"></script>` antes del cierre de `</body>` en `Fronted/src/WebUI/wwwroot/index.html`

- [ ] **T3 — Crear `VoiceInputService.cs`** (AC: 1, 2, 3, 4)
  - [ ] Crear `Fronted/src/WebUI/Services/VoiceInputService.cs`
  - [ ] Inyectar `IJSRuntime` en el constructor
  - [ ] Método `Task<bool> IsSupportedAsync()` → llama `window.voiceInput.isSupported()`
  - [ ] Método `Task StartAsync(DotNetObjectReference<T> dotNetRef, string lang = "es-CO")` → llama `window.voiceInput.start`
  - [ ] Método `Task StopAsync()` → llama `window.voiceInput.stop`
  - [ ] Registrar como `Scoped` en `Program.cs`

## Dev Notes

- Web Speech API disponible en Chrome 33+ y Edge 79+. Firefox no la soporta (ver MDN).
- Usar `webkitSpeechRecognition` como fallback para compatibilidad con algunos builds de Chrome.
- `continuous: false` hace que el reconocimiento se detenga solo al detectar pausa, ideal para dictado de un formulario completo.
- El `DotNetObjectReference` debe ser dispuesto por el componente que lo crea, no por el servicio.
- No hay cambios al backend en esta story.

## Files To Create / Modify

| Acción | Archivo |
|--------|---------|
| Crear | `Fronted/src/WebUI/wwwroot/js/voiceInput.js` |
| Modificar | `Fronted/src/WebUI/wwwroot/index.html` |
| Crear | `Fronted/src/WebUI/Services/VoiceInputService.cs` |
| Modificar | `Fronted/src/WebUI/Program.cs` (registro DI) |
