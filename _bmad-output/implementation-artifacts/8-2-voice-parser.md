# Story 8.2: VoiceParser — Motor de Reglas de Extracción

Status: complete

## Story

As a empleado,
I want que el sistema entienda lo que dije y extraiga automáticamente los campos del formulario,
So that no tenga que tipear cada campo manualmente después de dictar.

## Acceptance Criteria

1. **Given** transcript con "hoy", **When** se ejecuta el parser, **Then** Fecha = fecha actual. "ayer" → fecha actual menos 1 día.

2. **Given** transcript con "de 8 a 12" (primer par), **When** se ejecuta el parser, **Then** HoraEntradaAM = 08:00, HoraSalidaAM = 12:00.

3. **Given** transcript con "de 1 a 5 de la tarde" o segundo par de horas menor a 8, **When** se ejecuta el parser, **Then** HoraEntradaPM = 13:00, HoraSalidaPM = 17:00. Las horas < 8 sin contexto explícito de mañana se tratan como PM (se suma 12).

4. **Given** transcript con nombre de cliente normalizado (sin tildes, minúsculas) que coincide con un cliente del catálogo, **When** se ejecuta el parser, **Then** Cliente queda asignado con el valor exacto del catálogo.

5. **Given** cliente detectado y nombre de proyecto del cliente en transcript, **When** se ejecuta el parser, **Then** Proyecto queda asignado con el valor exacto del catálogo del cliente. Solo se busca en proyectos del cliente detectado.

6. **Given** keyword "presencial", "remoto" o "híbrido"/"hibrido" en transcript, **When** se ejecuta el parser, **Then** Modalidad queda asignada al valor correspondiente del catálogo.

7. **Given** keyword de recurso en transcript ("consultor sap" tiene prioridad sobre "consultor"; buscar en orden de especificidad descendente), **When** se ejecuta el parser, **Then** Recurso queda asignado al valor del catálogo.

8. **Given** keyword de lugar ("oficina" → "Presencial Oficina", "viaje" → "Presencial Viaje", "cliente" + no es nombre de cliente → "Presencial Cliente", "remoto" → "Remoto"), **When** se ejecuta el parser, **Then** Lugar queda asignado al valor del catálogo.

9. **Given** texto que sigue a "descripción:" o "descripcion:" en transcript, **When** se ejecuta el parser, **Then** Descripcion = ese texto (trimmed).

10. **Given** uno o más campos no detectados, **When** el parser termina, **Then** `VoiceParseResult.CamposNoDetectados` lista los nombres de los campos faltantes en español.

## Tasks / Subtasks

- [ ] **T1 — Crear `VoiceParseResult.cs`** (AC: todos)
  - [ ] Crear `Fronted/src/WebUI/Models/VoiceParseResult.cs`
  - [ ] Propiedades: `DateOnly? Fecha`, `TimeOnly? HoraEntradaAM`, `TimeOnly? HoraSalidaAM`, `TimeOnly? HoraEntradaPM`, `TimeOnly? HoraSalidaPM`, `string? Cliente`, `string? Proyecto`, `string? Modalidad`, `string? Recurso`, `string? Descripcion`, `string? Lugar`, `List<string> CamposNoDetectados`

- [ ] **T2 — Crear `VoiceCatalog.cs`** (AC: 4, 5, 6, 7, 8)
  - [ ] Crear `Fronted/src/WebUI/Models/VoiceCatalog.cs`
  - [ ] Propiedades: `List<string> Clientes`, `Dictionary<string, List<string>> ProyectosPorCliente`, `List<string> Modalidades`, `List<string> Recursos`, `List<string> Lugares`
  - [ ] Este DTO lo construye el componente a partir del catálogo ya cargado en el formulario

- [ ] **T3 — Crear `VoiceParser.cs`** (AC: todos)
  - [ ] Crear `Fronted/src/WebUI/Services/VoiceParser.cs`
  - [ ] Método público `VoiceParseResult Parse(string transcript, VoiceCatalog catalogo)`
  - [ ] Helper privado `Normalize(string s)` → minúsculas, reemplazar á→a, é→e, í→i, ó→o, ú→u, ü→u, ñ→n
  - [ ] Implementar extracción de fecha (AC: 1):
    - Buscar "hoy" → DateOnly.FromDateTime(DateTime.Today)
    - Buscar "ayer" → DateOnly.FromDateTime(DateTime.Today).AddDays(-1)
  - [ ] Implementar extracción de horas (AC: 2, 3):
    - Regex: `de\s+(\d{1,2})(?::(\d{2}))?\s+a\s+(\d{1,2})(?::(\d{2}))?` (global, encontrar todos los pares)
    - Primer par → AM. Si la hora de entrada AM ≥ 8, asignar como AM. Si < 8, tratar como PM.
    - Segundo par → PM. Si hora < 8 sumar 12 para convertir a formato 24h.
    - Verificar keywords "mañana"/"manana" (fuerza AM) y "tarde" (fuerza PM) adyacentes al par
  - [ ] Implementar matching de cliente (AC: 4):
    - Para cada cliente en `catalogo.Clientes`, verificar si `Normalize(transcript)` contiene `Normalize(cliente)`
    - Usar el cliente con el nombre más largo que matchee (evitar falsos positivos con nombres cortos)
  - [ ] Implementar matching de proyecto (AC: 5):
    - Solo si cliente fue detectado
    - Buscar en `catalogo.ProyectosPorCliente[cliente]`, misma lógica de substring normalizado
  - [ ] Implementar matching de modalidad (AC: 6):
    - Map fijo: "presencial"→"Presencial", "remoto"→"Remoto", "hibrido"→"Híbrido"
  - [ ] Implementar matching de recurso (AC: 7):
    - Normalizar todos los recursos del catálogo y buscar en transcript
    - Ordenar por longitud descendente para que "consultor sap" gane sobre "consultor"
  - [ ] Implementar matching de lugar (AC: 8):
    - Map fijo contra transcript normalizado: "oficina"→primer lugar con "Oficina", "viaje"→primer lugar con "Viaje", "remoto" (si no es modalidad ya detectada)→primer lugar "Remoto"
    - "cliente" solo matchea Lugar si no es nombre de cliente ya detectado
  - [ ] Implementar extracción de descripción (AC: 9):
    - Buscar índice de "descripción:" o "descripcion:" en transcript (case-insensitive)
    - Tomar todo el texto desde ese índice + 12 chars en adelante, trimmed
  - [ ] Calcular CamposNoDetectados (AC: 10):
    - Lista de nombres en español de campos que quedaron null

- [ ] **T4 — Registrar `VoiceParser` en `Program.cs`**
  - [ ] `builder.Services.AddSingleton<VoiceParser>()` (es stateless, singleton es seguro)

## Dev Notes

- `VoiceParser` es puro (sin efectos secundarios, sin async), fácil de unit-testear.
- El catálogo de clientes/proyectos se carga en el formulario desde la API antes del dictado — no hay llamada adicional al backend.
- Normalizar también el transcript completo una sola vez al inicio del método `Parse` para eficiencia.
- Los nombres del catálogo en producción pueden tener tildes; normalizar ambos lados antes de comparar.
- No hay cambios al backend en esta story.

## Files To Create / Modify

| Acción | Archivo |
|--------|---------|
| Crear | `Fronted/src/WebUI/Models/VoiceParseResult.cs` |
| Crear | `Fronted/src/WebUI/Models/VoiceCatalog.cs` |
| Crear | `Fronted/src/WebUI/Services/VoiceParser.cs` |
| Modificar | `Fronted/src/WebUI/Program.cs` (registro DI) |
