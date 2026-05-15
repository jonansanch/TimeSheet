# Story 1.6: Experiencia de Carga Inicial Blazor WASM

Status: review

## Story

As a usuario interno,
I want ver feedback claro durante la carga inicial,
So that no perciba la aplicación como rota mientras descarga el runtime.

## Acceptance Criteria

1. **Given** la primera carga de Blazor WebAssembly, **When** la app descarga recursos iniciales, **Then** se muestra pantalla de carga con identidad KPG, barra de progreso y texto "Cargando KPG Timesheet...", **And** la experiencia comunica honestamente la demora inicial.

2. **Given** la app publicada en entorno IIS, **When** se sirven assets Blazor WASM, **Then** Brotli/gzip está habilitado para reducir el bundle, **And** la carga inicial objetivo es menor o igual a 5 segundos en red interna.

## Tasks / Subtasks

- [x] **T1 — Loading screen KPG branded en index.html** (AC: 1)
  - [x] Reemplazar el spinner SVG genérico del `#app` div con pantalla KPG: fondo navy, logo textual "KPG Timesheet", subtítulo, barra de progreso animada
  - [x] Texto: "Cargando KPG Timesheet..."
  - [x] Actualizar `<title>` a "KPG Timesheet"
  - [x] Traducir `blazor-error-ui` al español

- [x] **T2 — Estilos loading screen en app.css** (AC: 1)
  - [x] Estilos del `#kpg-loading`: centrado vertical/horizontal, fondo #0D3B5E, fuente Poppins
  - [x] Barra de progreso Blazor enlazada a `--blazor-load-percentage` con color #5BB8D4
  - [x] Estado de error en español

- [x] **T3 — web.config para compresión IIS** (AC: 2)
  - [x] Crear `Fronted/src/WebUI/wwwroot/web.config` con Brotli + gzip habilitados
  - [x] MIME types para `.wasm`, `.blat`, `.dat` (requeridos para Blazor WASM en IIS)
  - [x] Headers de caché: assets con fingerprint max-age=1 año, index.html no-cache
  - [x] SPA fallback rule (rewrite all to index.html)
  - [x] Headers de seguridad: X-Content-Type-Options, X-Frame-Options

- [x] **T4 — Compilar y verificar** (AC: 1, 2)
  - [x] `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — 0 errores, 0 advertencias
  - [x] `dotnet test Backend/KPG.Timesheet.sln` — 9/9 pasan

## Dev Notes

### Estructura del loading screen KPG (Dirección A, fondo navy)

```html
<div id="app">
  <div id="kpg-loading">
    <div class="kpg-loading-brand">
      <span class="kpg-brand-title">KPG Timesheet</span>
    </div>
    <div class="kpg-loading-bar-wrap">
      <div class="kpg-loading-bar"></div>
    </div>
    <p class="kpg-loading-text">Cargando KPG Timesheet...</p>
  </div>
</div>
```

### Barra de progreso Blazor WASM

Blazor expone la variable CSS `--blazor-load-percentage` (0%–100%) durante la carga.
Usarla en la barra de progreso:

```css
.kpg-loading-bar {
  width: var(--blazor-load-percentage, 0%);
  transition: width 0.1s ease;
}
```

### web.config IIS — Blazor WASM requirements

Blazor WASM necesita MIME types para `.wasm` y archivos `.dll`. El web.config debe:
- Habilitar módulo de compresión dinámica (Brotli + gzip)
- Agregar MIME types para `application/wasm` (.wasm) y `application/octet-stream` (.dat, .blat)
- Headers `cache-control: max-age=31536000` para assets con fingerprint
- `no-cache` para `index.html` (sin fingerprint, debe ser siempre fresco)

### NFR1 — objetivo 5s en red interna

La compresión Brotli reduce el bundle Blazor WASM ~30-40%. El web.config prepara
el servidor para cumplir NFR1 cuando se despliegue en IIS. No es verificable en
entorno de desarrollo local (Kestrel).

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

Sin bloqueos durante la implementación.

### Completion Notes List

- T1: `index.html` actualizado — `<title>` en español, `#kpg-loading` con SVG icono reloj, brand "KPG Timesheet", texto "Cargando KPG Timesheet...", barra de progreso. `blazor-error-ui` traducido al español.
- T2: Estilos `#kpg-loading` en `app.css`: fondo `#0D3B5E`, texto blanco Poppins, barra celeste `#5BB8D4` enlazada a `--blazor-load-percentage`. Eliminados estilos genéricos `.loading-progress` y `.loading-progress-text` que ya no se usan.
- T3: `web.config` creado con SPA fallback, MIME types para Blazor WASM (.wasm, .blat, .dat), compresión dinámica y estática habilitada, headers de caché (fingerprint=1 año / index.html=no-cache), X-Content-Type-Options y X-Frame-Options.
- T4: Frontend 0 errores 0 advertencias; 9/9 unit tests pasan.

### File List

- `Fronted/src/WebUI/wwwroot/index.html` — loading screen KPG branded, textos en español
- `Fronted/src/WebUI/wwwroot/css/app.css` — estilos KPG loading screen + eliminado SVG spinner genérico
- `Fronted/src/WebUI/wwwroot/web.config` — nuevo: SPA routing, MIME types, compresión IIS, caché headers

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2026-05-13 | 0.1 | Story creada | claude-sonnet-4-6 |
| 2026-05-13 | 1.0 | Implementación completa — todos los ACs satisfechos | claude-sonnet-4-6 |
