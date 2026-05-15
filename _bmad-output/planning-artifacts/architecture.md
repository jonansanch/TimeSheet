---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "_bmad-output/planning-artifacts/ux-design-specification.md"
  - "_bmad-output/planning-artifacts/product-brief-Timesheet.md"
workflowType: 'architecture'
project_name: 'Timesheet'
user_name: 'Jonathan'
date: '2026-05-12'
lastStep: 8
status: 'complete'
completedAt: '2026-05-13'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

---

## Análisis de Contexto del Proyecto

### Resumen de Requerimientos

**Requerimientos Funcionales:**
40 FRs distribuidos en 6 dominios: Registro de Horas (FR1–FR15), Acceso y Seguridad (FR16–FR21), Catálogos (FR22–FR26), Reportes (FR27–FR31), Dashboard Operativo (FR32–FR36), y Auditoría y Control (FR37–FR40). El dominio central es el registro de horas con inmutabilidad parcial (solo descripción editable post-guardado) y ventana de retroactividad configurable de 3 días hábiles con flujo de aprobación de excepciones.

**Requerimientos No Funcionales:**
Los NFRs que más impactan la arquitectura son:
- Rendimiento: carga WASM ≤ 5s, guardado < 2s, reportes < 10s, 30 usuarios concurrentes
- Seguridad: JWT almacenado en memoria del cliente, bcrypt/argon2, CORS interno, validación independiente en cada capa (NFR12)
- Confiabilidad: 99% disponibilidad en horario laboral, backups diarios, recuperación en ≤ 4 horas
- Escalabilidad: arquitectura soporta crecimiento hasta 100 usuarios sin cambios estructurales; índices en columnas de filtro frecuente
- Usabilidad: formulario completable solo por teclado (NFR19); mensajes de error específicos por campo (NFR20)

**Escala y Complejidad:**
- Dominio primario: Full-stack web (Blazor WASM SPA + .NET 10 API REST)
- Nivel de complejidad: Medio
- Usuarios: 30 concurrentes → escalar a 100 sin cambios estructurales
- Sin real-time en V1, sin multi-tenancy, sin integraciones externas

### Restricciones y Dependencias Técnicas

- Stack decidido: .NET 10 + Blazor WebAssembly + Clean Architecture + CQRS (MediatR)
- Persistencia: EF Core (escritura) + Dapper (lectura de reportes y dashboard)
- UI: MudBlazor (componentes) + ApexCharts for Blazor (gráficas)
- Autenticación: JWT stateless + refresh token con revocación
- Red: interna corporativa; sin exposición pública; sin acceso móvil en V1
- Librería de exportación PDF/Excel: **pendiente de decisión**
- Sin migración de históricos desde Excel

### Preocupaciones Transversales Identificadas

1. **RBAC en 4 niveles** — cada endpoint, query y componente UI debe respetar los permisos del rol activo independientemente de las restricciones en otras capas (NFR9)
2. **Inmutabilidad parcial de registros** — el dominio debe hacer cumplir que solo el campo descripción es editable; el resto requiere autorización explícita de Admin
3. **Bitácora de auditoría append-only** — registra inicios de sesión, cambios de rol, modificaciones autorizadas y aprobaciones; debe ser consumida por 3 roles distintos
4. **Ventana de retroactividad como parámetro del sistema** — la regla de negocio depende de un parámetro configurable en runtime; no puede hardcodearse en el dominio
5. **Flujo de excepción de ventana** — requiere persistir un estado pendiente/aprobado/rechazado con ciclo de vida propio; es un mini-workflow de aprobación
6. **JWT en memoria de Blazor WASM** — el ciclo de vida del token en WebAssembly tiene implicaciones sobre la estrategia de refresh y la experiencia al recargar la página
7. **Exportación PDF/Excel de reportes** — requerida por FR29/FR31 pero sin librería decidida; necesita resolución antes de implementar el módulo de reportes

---

## Evaluación de Template de Inicio

### Dominio Tecnológico Primario

Full-stack web — Blazor WebAssembly SPA + .NET 10 REST API con estructura Clean Architecture.

### Opciones Consideradas

Ningún template existente cubre el stack completo (.NET 10 + Blazor WASM + MudBlazor + Dapper). El template más relevante (fullstackhero/blazor-starter-kit) está desactualizado en .NET 6. Se elige scaffolding híbrido: backend desde jasontaylordev/CleanArchitecture (.NET 10 activo) + frontend Blazor WASM estándar.

### Scaffolding Seleccionado: Híbrido Jason Taylor + Blazor WASM Estándar

**Justificación:**
- Jason Taylor CleanArchitecture (.NET 10) cubre el 100% de la estructura backend necesaria
- Blazor WASM nativo cubre el frontend; MudBlazor se agrega como NuGet
- Control total sobre la configuración sin dependencias de templates desactualizados
- Decisiones ya tomadas en PRD se implementan directamente, sin sobreescribir opciones del template

**Comandos de inicialización:**

```bash
# Backend — Clean Architecture
dotnet new install CleanArchitecture.Template::8.0.6
dotnet new ca-sln --output KPG.Timesheet

# Frontend — Blazor WASM
dotnet new blazorwasm --output src/WebUI --name KPG.Timesheet.WebUI
dotnet sln add src/WebUI/KPG.Timesheet.WebUI.csproj

# Paquetes adicionales
dotnet add src/Infrastructure package Dapper
dotnet add src/Infrastructure package MiniExcel
dotnet add src/Infrastructure package QuestPDF
dotnet add src/WebUI package MudBlazor
dotnet add src/WebUI package Blazor.ApexCharts
```

**Decisiones arquitectónicas establecidas por el scaffolding:**

- Lenguaje/Runtime: C# 13 + .NET 10
- Estructura: Domain / Application / Infrastructure / Api / WebUI (5 proyectos)
- Validación: FluentValidation (incluido en template Jason Taylor)
- Testing: xUnit + FluentAssertions + NSubstitute (incluido en template)
- Exportación Excel: MiniExcel (lightweight, streaming)
- Exportación PDF: QuestPDF (open source, Community License — costo $0)
- Gráficas: Blazor.ApexCharts (open source)

**Nota:** La inicialización del proyecto debe ser la primera historia de implementación.

---

## Decisiones Arquitectónicas Centrales

### Análisis de Prioridad de Decisiones

**Decisiones Críticas (Bloquean implementación):**
- Motor de base de datos: SQL Server
- Autenticación: ASP.NET Core Identity + JWT stateless
- Almacenamiento de refresh token: tabla en base de datos
- RBAC: Roles simples ASP.NET Core
- Ciclo de vida JWT en WASM: Custom AuthenticationStateProvider

**Decisiones Importantes (Definen arquitectura):**
- Gestión de estado frontend: Singleton Services con notificación
- Estándar de errores: Problem Details RFC 9457
- Logging: Serilog + sink a archivo
- Hosting: IIS + Kestrel en Windows Server on-premises

**Decisiones Diferidas (Post-MVP):**
- Caché: Sin caché en V1; evaluar IMemoryCache en dashboard si los tiempos de carga superan NFR4 (≤ 3s) en testing

---

### Arquitectura de Datos

| Decisión | Elección | Versión | Justificación |
|----------|----------|---------|---------------|
| Motor de BD | SQL Server | 2019+ | Stack .NET en infraestructura Windows existente; integración nativa con EF Core |
| ORM escritura | Entity Framework Core | 10.x | Code-First migrations; LINQ type-safe; incluido en template |
| Micro-ORM lectura | Dapper | 2.x | SQL de alto rendimiento para reportes con joins complejos; sin overhead de tracking |
| Migraciones | EF Core Code-First | — | `dotnet ef migrations add`; historial versionado en el repositorio |
| Caché | Sin caché en V1 | — | 30 usuarios; índices en columnas de filtro cubren NFR3/NFR4; agregar IMemoryCache si testing lo requiere |
| Modelo de datos clave | Soft-delete en catálogos | — | Empleados/clientes/proyectos desactivados preservan integridad referencial de registros históricos |

**Tablas críticas del dominio:**
- `RegistroHoras` — inmutable post-guardado excepto campo `Descripcion`; FK a `Empleado`, `Cliente`, `Proyecto`
- `SolicitudExcepcion` — estado: `Pendiente` / `Aprobada` / `Rechazada`; FK a `RegistroHoras` y `Admin`
- `RefreshTokens` — token hash, userId, expiración, revocado (bool + timestamp)
- `BitacoraAuditoria` — append-only; tipo de evento, userId, entidad afectada, timestamp, metadata JSON
- `ParametroSistema` — clave-valor para ventana de retroactividad y otros parámetros configurables

---

### Seguridad y Autenticación

| Decisión | Elección | Justificación |
|----------|----------|---------------|
| Gestión de usuarios | ASP.NET Core Identity | Incluye hashing PBKDF2 (600K iteraciones), gestión de roles, lockout; sin librerías extra |
| Token de acceso | JWT stateless, expiración 60 min | NFR8; firmado con clave simétrica almacenada en configuración de servidor |
| Refresh token | Tabla `RefreshTokens` en BD | Persistente entre reinicios; revocable; auditable; expira en 8h (NFR8) |
| RBAC | `[Authorize(Roles = "...")]` ASP.NET Core | 4 roles fijos sin permisos dinámicos; simple y directo |
| Almacenamiento JWT | Memoria del cliente (Blazor WASM) | NFR7; el Custom AuthStateProvider reconstruye sesión desde refresh token al recargar |
| Hashing contraseñas | PBKDF2 vía ASP.NET Core Identity | Moderno, bien testeado, sin dependencia adicional; cumple NFR10 |
| CORS | Política restrictiva a origen interno | NFR11; configurada en `Program.cs` del API |
| Autorización en API | Verificación por endpoint independiente de UI | NFR9; cada controller/endpoint verifica rol antes de procesar |

**Ciclo de vida JWT en Blazor WASM:**
Al iniciar la app, el `KpgAuthStateProvider` (implementación de `AuthenticationStateProvider`) llama a `POST /api/auth/refresh` con el refresh token almacenado en `sessionStorage`. Si el servidor devuelve un nuevo JWT, la sesión se restaura transparentemente. Si no (token expirado/revocado), el usuario ve el login. El JWT se guarda exclusivamente en memoria (`_currentToken` en el provider), nunca en `localStorage`.

---

### API y Comunicación

| Decisión | Elección | Justificación |
|----------|----------|---------------|
| Estilo de API | REST con controllers ASP.NET Core | Incluido en template; convenciones claras para CQRS; sin complejidad de GraphQL |
| Versioning | Sin versioning en V1 | API interna de una sola app; agregar si surge necesidad de integraciones externas |
| Contrato de errores | Problem Details RFC 9457 | Estándar nativo en .NET 10; `IProblemDetailsService` en `Program.cs`; `ValidationProblemDetails` para errores de formulario |
| Documentación | Scalar + OpenAPI nativo .NET 10 | Reemplaza Swagger UI; integrado sin paquetes adicionales en .NET 10 |
| Rate limiting | Sin rate limiting en V1 | Red interna; 30 usuarios; riesgo negligible |
| Comunicación WASM→API | `HttpClient` tipado por dominio (Typed Clients) | Un repositorio por módulo; inyectado vía DI; gestiona JWT en header |

**Contrato de error estándar (Problem Details):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "Validation failed",
  "status": 422,
  "errors": {
    "HoraSalida": ["La hora de salida debe ser posterior a la hora de entrada"]
  }
}
```

---

### Arquitectura Frontend (Blazor WASM)

| Decisión | Elección | Justificación |
|----------|----------|---------------|
| Gestión de estado | Singleton Services + `StateHasChanged` | Suficiente para 4 módulos independientes; sin overhead de Redux/Fluxor |
| Patrón de acceso HTTP | Repository pattern (Typed HttpClient) | Un repositorio por agregado; oculta detalles de HTTP al ViewModel; facilita testing |
| Organización de componentes | Por feature (módulo) | `Features/Registro/`, `Features/Dashboard/`, `Features/Reportes/`, `Features/Admin/` |
| Auth state | `KpgAuthStateProvider` custom | Extiende `AuthenticationStateProvider`; reconstruye sesión desde refresh token al inicio |
| Routing | Blazor Router nativo | Sin librerías adicionales; rutas protegidas con `<AuthorizeRouteView>` |
| Optimización bundle | Brotli/gzip (IIS compression) | NFR1 (carga ≤ 5s); habilitado en `web.config` del sitio IIS |
| Loading inicial | Pantalla custom en `index.html` | Logo KPG + barra de progreso durante descarga del runtime WASM (per UX spec) |

**Estructura de carpetas WebUI:**
```
src/WebUI/
├── Features/
│   ├── Registro/         ← KpgShiftForm, KpgDatePicker
│   ├── Dashboard/        ← KpgTeamStatusCard, KpgStatCard
│   ├── Reportes/         ← filtros, exportación
│   └── Admin/            ← catálogos, bitácora, aprobaciones
├── Shared/
│   ├── Components/       ← componentes custom KPG transversales
│   └── Services/         ← auth state, notification service
└── Infrastructure/
    └── Repositories/     ← HTTP clients por dominio
```

---

### Infraestructura y Despliegue

| Decisión | Elección | Justificación |
|----------|----------|---------------|
| Hosting | IIS + Kestrel en Windows Server | On-premises; IIS como reverse proxy; Kestrel corre el proceso .NET |
| Deploy WASM | Sitio IIS separado (archivos estáticos) | El bundle Blazor WASM son assets estáticos; se publica en wwwroot de IIS |
| Logging | Serilog 4.x | Sinks: Console (desarrollo) + File con rotación diaria (producción) |
| Log format | Structured logging (JSON) | Facilita búsqueda y diagnóstico sin acceso a servidor |
| CI/CD | Manual en V1 | `dotnet publish` + xcopy a servidor; automatizar post-MVP si se adopta |
| Secretos | `appsettings.Production.json` en servidor (fuera del repo) | JWT signing key, connection string; nunca en control de versiones |
| Backups | SQL Server Backup Agent (tarea programada) | NFR14; respaldo diario; recuperación ≤ 4h |

---

### Análisis de Impacto de Decisiones

**Secuencia de implementación sugerida por dependencias:**
1. Scaffolding + SQL Server + Identity (base de todo)
2. JWT + Refresh token + RBAC (desbloquea todos los flujos autenticados)
3. Dominio: entidades `RegistroHoras`, `SolicitudExcepcion`, `BitacoraAuditoria`
4. CQRS: Commands/Queries de registro de horas (el flujo central)
5. Shell Blazor WASM + KpgAuthStateProvider (desbloquea frontend)
6. Módulo de Registro (J1 + J2)
7. Dashboard Supervisor + Módulo de Reportes (J3)
8. Módulo Admin — catálogos + bitácora + aprobaciones (J4)

**Dependencias cruzadas clave:**
- `ParametroSistema` (ventana retroactividad) debe existir antes de cualquier lógica de registro
- `BitacoraAuditoria` es transversal — todos los commands deben escribir en ella
- El `KpgAuthStateProvider` bloquea todo el frontend hasta que esté implementado
- Problem Details debe configurarse antes de implementar validaciones

---

## Patrones de Implementación y Reglas de Consistencia

### Puntos de Conflicto Identificados

8 zonas donde agentes de IA podrían tomar decisiones incompatibles entre sí. Todas resueltas con los patrones a continuación.

---

### Patrones de Nomenclatura

**Base de datos (SQL Server):**
- Tablas: PascalCase plural — `RegistrosHoras`, `SolicitudesExcepcion`, `RefreshTokens`, `BitacoraAuditoria`
- Columnas: PascalCase — `RegistroId`, `FechaRegistro`, `HoraEntrada`, `IsActive`
- Claves foráneas: `{Entidad}Id` — `EmpleadoId`, `ClienteId`, `ProyectoId`
- Índices: `IX_{Tabla}_{Columna(s)}` — `IX_RegistrosHoras_EmpleadoId_FechaRegistro`
- Claves primarias: `PK_{Tabla}` (convención EF Core por defecto)

**Endpoints REST:**
- Plural + kebab-case: `/api/registros-horas`, `/api/solicitudes-excepcion`, `/api/parametros-sistema`
- Parámetros de ruta: `{id}` (formato .NET) — `GET /api/registros-horas/{id}`
- Query params: camelCase — `?empleadoId=1&fechaDesde=2026-05-01`
- Acciones no-CRUD con verbo: `POST /api/solicitudes-excepcion/{id}/aprobar`

**Commands y Queries (CQRS):**
- Patrón: `{Verbo}{Entidad}Command` / `{Verbo}{Entidad}Query`
- Commands: `CreateRegistroHorasCommand`, `ApproveExcepcionCommand`, `DeleteRegistroCommand`
- Queries: `GetRegistrosByUserQuery`, `GetReporteByPeriodoQuery`, `GetBitacoraQuery`
- DTOs de resultado: `{Entidad}Dto` — `RegistroHorasDto`, `EmpleadoDto`, `ReporteLineaDto`

**Código C# (backend):**
- Clases, interfaces, métodos, propiedades: PascalCase (estándar C#)
- Variables locales y parámetros: camelCase
- Interfaces: prefijo `I` — `IRegistroHorasRepository`, `IAuditService`
- Constantes: PascalCase en clases estáticas — `Roles.Admin`, `Roles.Supervisor`

**Componentes Blazor:**
- Archivos `.razor`: PascalCase — `KpgShiftForm.razor`, `KpgDatePicker.razor`
- Parámetros de componente: PascalCase — `[Parameter] public int EmpleadoId { get; set; }`
- Variables de estado en componente: camelCase — `private bool isLoading = false`

**JSON (serialización API↔Frontend):**
- Campos: camelCase — `"registroId"`, `"fechaRegistro"`, `"horaEntrada"`
- Configurado globalmente en `Program.cs`: `JsonNamingPolicy.CamelCase`

---

### Patrones de Estructura

**Organización CQRS — Feature Folders (vertical slices):**
```
src/Application/
└── Features/
    ├── RegistroHoras/
    │   ├── Commands/
    │   │   ├── CreateRegistro/
    │   │   │   ├── CreateRegistroCommand.cs
    │   │   │   ├── CreateRegistroCommandHandler.cs
    │   │   │   └── CreateRegistroCommandValidator.cs
    │   │   └── DeleteRegistro/
    │   └── Queries/
    │       └── GetRegistrosByUser/
    │           ├── GetRegistrosByUserQuery.cs
    │           ├── GetRegistrosByUserQueryHandler.cs
    │           └── RegistroHorasDto.cs
    ├── Excepciones/
    ├── Catálogos/
    ├── Reportes/
    ├── Dashboard/
    └── Auditoria/
```

**Regla:** Command, Handler, Validator y DTO de resultado viven en la misma carpeta. Un agente que implementa `CreateRegistroCommand` crea los 4 archivos en `Features/RegistroHoras/Commands/CreateRegistro/`.

**Organización Frontend — Feature Folders:**
```
src/WebUI/
├── Features/
│   ├── Registro/
│   │   ├── Pages/        ← RegistroPage.razor
│   │   ├── Components/   ← KpgShiftForm.razor, KpgDatePicker.razor
│   │   └── Services/     ← RegistroStateService.cs, IRegistroRepository.cs
│   ├── Dashboard/
│   ├── Reportes/
│   └── Admin/
├── Shared/
│   ├── Components/       ← KpgSaveConfirmationBanner.razor, KpgStatCard.razor
│   └── Services/         ← KpgAuthStateProvider.cs, NotificationService.cs
└── Infrastructure/
    └── Repositories/     ← HttpClient implementations
```

---

### Patrones de Formato

**Respuestas exitosas de API:**
- Sin wrapper — retornar el objeto o colección directamente
- `200 OK` con objeto para GET individual
- `200 OK` con lista para GET colección
- `201 Created` con objeto creado + `Location` header para POST
- `204 No Content` para DELETE exitoso

**Errores — Problem Details RFC 9457:**
```json
// Validación (422)
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "errors": {
    "HoraSalida": ["La hora de salida debe ser posterior a la hora de entrada"],
    "ClienteId": ["El cliente es requerido"]
  }
}

// Not Found (404)
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "Not Found",
  "status": 404,
  "detail": "RegistroHoras with id '99' was not found."
}

// Forbidden (403)
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "Forbidden",
  "status": 403,
  "detail": "No tienes autorización para modificar este registro."
}
```

**Formatos de fecha y hora:**
- API (JSON): ISO 8601 — `"2026-05-12T08:00:00"` (sin timezone; sistema opera en zona horaria local del servidor)
- UI display: `dd/MM/yyyy` para fechas, `HH:mm` para horas (24h)
- Blazor: usar `DateTime` internamente; formatear en la capa de presentación

**Paginación (para listas largas — bitácora, historial):**
```json
{
  "items": [...],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 25
}
```

---

### Patrones de Comunicación

**Estado en Blazor — Singleton Service con evento:**
```csharp
// Patrón estándar para TODOS los módulos
public class RegistroStateService {
    private List<RegistroHorasDto> _registros = new();
    public IReadOnlyList<RegistroHorasDto> Registros => _registros;
    public event Action? OnChange;

    public async Task LoadRegistrosAsync(int empleadoId) {
        _registros = await _repository.GetByUserAsync(empleadoId);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

// En el componente:
protected override void OnInitialized() {
    _registroState.OnChange += StateHasChanged;
}
public void Dispose() {
    _registroState.OnChange -= StateHasChanged;
}
```

**Regla:** Todos los componentes que consumen estado deben implementar `IDisposable` y desuscribirse en `Dispose()`.

---

### Patrones de Proceso

**Excepciones de dominio — jerarquía estándar:**
```csharp
// Excepciones predefinidas en Application layer
NotFoundException          → HTTP 404
ValidationException        → HTTP 422  (lanzada por FluentValidation pipeline)
ForbiddenAccessException   → HTTP 403
ConflictException          → HTTP 409  (ej: registro duplicado en misma fecha/turno)
BusinessRuleException      → HTTP 422  (ej: fuera de ventana sin excepción aprobada)
```
El `GlobalExceptionHandlerMiddleware` mapea cada tipo al Problem Details correspondiente. **Ningún controller debe tener try/catch** — el middleware lo maneja.

**Validación — pipeline automático:**
FluentValidation está registrado como behavior de MediatR. Todo command con un validator asociado es validado automáticamente antes de llegar al handler. **No validar manualmente en handlers.**

**Auditoría — interceptor automático:**
```csharp
// Entidades auditables implementan IAuditableEntity
public interface IAuditableEntity {
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
// El AuditInterceptor llena estos campos automáticamente en SaveChanges
```
Para eventos de bitácora de negocio (login, cambio de rol, aprobación de excepción), se usa `IBitacoraService.LogAsync(tipo, entidad, metadata)` explícitamente en el command handler correspondiente.

**Soft Delete — patrón estándar en catálogos:**
```csharp
public interface ISoftDeletable {
    bool IsActive { get; set; }
    DateTime? DeactivatedAt { get; set; }
}
// EF Core query filter global: .HasQueryFilter(e => e.IsActive)
// Todos los queries excluyen registros inactivos automáticamente
// Para incluir inactivos: .IgnoreQueryFilters()
```

**Estados de carga en Blazor:**
```csharp
// Variables de estado estándar en componentes con carga de datos
private bool _isLoading = true;
private string? _errorMessage = null;

// Patrón estándar
try {
    _isLoading = true;
    await _state.LoadAsync();
} catch (Exception ex) {
    _errorMessage = "Error al cargar los datos. Intenta nuevamente.";
    _logger.LogError(ex, "Error loading {Component}", nameof(MiComponente));
} finally {
    _isLoading = false;
    StateHasChanged();
}
```

---

### Reglas Obligatorias para Todos los Agentes

**Los agentes DEBEN:**
1. Crear Command + Handler + Validator + DTO en la misma carpeta de feature
2. Usar `[Authorize(Roles = "...")]` en cada controller action — nunca confiar solo en el frontend
3. Retornar `IActionResult` con el status code correcto por tipo de respuesta
4. Implementar `IDisposable` en componentes Blazor que se suscriban a eventos de estado
5. Usar `IBitacoraService.LogAsync()` explícitamente en handlers de eventos de negocio críticos
6. Nombrar endpoints en plural + kebab-case
7. Formatear fechas como ISO 8601 en DTOs de API; formatear para display solo en la capa de presentación Blazor
8. Lanzar excepciones de dominio tipadas — nunca retornar errores en el payload de éxito

**Los agentes NO DEBEN:**
- Agregar try/catch en controllers o handlers para errores esperados — usar excepciones de dominio
- Validar manualmente en handlers si existe un Validator de FluentValidation
- Acceder a `HttpClient` directamente en componentes — usar el Repository correspondiente
- Guardar el JWT en `localStorage` o `sessionStorage` directamente — solo `KpgAuthStateProvider` gestiona tokens
- Crear queries Dapper para operaciones de escritura — Dapper es exclusivo para lectura
 

---

## Project Structure & Boundaries

### Complete Project Directory Structure

```text
Timesheet/
├── Backend/
│   ├── KPG.Timesheet.sln
│   ├── Directory.Build.props
│   ├── Directory.Packages.props
│   ├── global.json
│   ├── README.md
│   ├── .editorconfig
│   ├── src/
│   │   ├── Domain/
│   │   │   ├── Common/
│   │   │   ├── Constants/
│   │   │   ├── Entities/
│   │   │   │   ├── RegistroHoras.cs
│   │   │   │   ├── SolicitudExcepcion.cs
│   │   │   │   ├── Empleado.cs
│   │   │   │   ├── Cliente.cs
│   │   │   │   ├── Proyecto.cs
│   │   │   │   ├── ParametroSistema.cs
│   │   │   │   ├── BitacoraAuditoria.cs
│   │   │   │   └── RefreshToken.cs
│   │   │   ├── Enums/
│   │   │   └── Exceptions/
│   │   ├── Application/
│   │   │   ├── Common/
│   │   │   │   ├── Behaviours/
│   │   │   │   ├── Exceptions/
│   │   │   │   ├── Interfaces/
│   │   │   │   ├── Mappings/
│   │   │   │   ├── Models/
│   │   │   │   └── Security/
│   │   │   └── Features/
│   │   │       ├── RegistroHoras/
│   │   │       ├── SolicitudesExcepcion/
│   │   │       ├── Auth/
│   │   │       ├── Usuarios/
│   │   │       ├── Catalogos/
│   │   │       ├── Reportes/
│   │   │       ├── Dashboard/
│   │   │       └── Auditoria/
│   │   ├── Infrastructure/
│   │   │   ├── Data/
│   │   │   │   ├── Configurations/
│   │   │   │   ├── Interceptors/
│   │   │   │   ├── Migrations/
│   │   │   │   └── ApplicationDbContext.cs
│   │   │   ├── Identity/
│   │   │   ├── Persistence/
│   │   │   │   ├── Dapper/
│   │   │   │   └── Repositories/
│   │   │   ├── Reports/
│   │   │   │   ├── Excel/
│   │   │   │   └── Pdf/
│   │   │   ├── Security/
│   │   │   └── Services/
│   │   ├── Api/
│   │   │   ├── Controllers/
│   │   │   │   ├── AuthController.cs
│   │   │   │   ├── RegistrosHorasController.cs
│   │   │   │   ├── SolicitudesExcepcionController.cs
│   │   │   │   ├── CatalogosController.cs
│   │   │   │   ├── ReportesController.cs
│   │   │   │   ├── DashboardController.cs
│   │   │   │   └── AuditoriaController.cs
│   │   │   ├── Middleware/
│   │   │   ├── OpenApi/
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   └── Program.cs
│   │   └── WebUI/
│   │       ├── Features/
│   │       │   ├── Registro/
│   │       │   ├── Dashboard/
│   │       │   ├── Reportes/
│   │       │   └── Admin/
│   │       ├── Infrastructure/
│   │       │   └── Repositories/
│   │       ├── Shared/
│   │       │   ├── Components/
│   │       │   ├── Layout/
│   │       │   └── Services/
│   │       ├── wwwroot/
│   │       ├── App.razor
│   │       ├── Program.cs
│   │       └── Routes.razor
│   └── tests/
│       ├── Domain.UnitTests/
│       ├── Application.UnitTests/
│       ├── Infrastructure.IntegrationTests/
│       ├── Api.IntegrationTests/
│       └── WebUI.ComponentTests/
├── Docs/
├── _bmad-output/
└── Fronted/
    └── legacy-angular-prototype/
```

### Architectural Boundaries

**API Boundaries:**
- `Api/Controllers` es la única entrada HTTP externa.
- Cada controller delega a MediatR; no contiene reglas de negocio.
- Cada action usa `[Authorize(Roles = "...")]`.
- Errores salen como Problem Details; no hay try/catch por action.

**Application Boundaries:**
- `Application/Features` contiene commands, queries, handlers, validators y DTOs.
- Los handlers coordinan casos de uso, validan reglas mediante dominio/servicios y registran bitácora cuando corresponde.
- `Application` no depende de `Infrastructure`, `Api` ni `WebUI`.

**Domain Boundaries:**
- `Domain` contiene entidades, enums, constantes, excepciones y reglas invariantes.
- La inmutabilidad de `RegistroHoras` vive en dominio, no en UI.
- `SolicitudExcepcion` mantiene su propio ciclo de vida: pendiente, aprobada, rechazada.

**Infrastructure Boundaries:**
- EF Core se usa para escritura y operaciones transaccionales.
- Dapper se usa solo para lecturas de reportes y dashboard.
- MiniExcel y QuestPDF viven en `Infrastructure/Reports`.
- Identity, JWT, refresh tokens, Serilog y persistencia concreta viven aquí.

**WebUI Boundaries:**
- Componentes Blazor no llaman `HttpClient` directamente.
- Cada feature usa repositories tipados bajo `WebUI/Infrastructure/Repositories`.
- Estado compartido vive en services singleton con evento `OnChange`.

### Requirements to Structure Mapping

**Registro de Horas, FR1-FR15:**
- Dominio: `Domain/Entities/RegistroHoras.cs`, `SolicitudExcepcion.cs`
- Casos de uso: `Application/Features/RegistroHoras`, `SolicitudesExcepcion`
- API: `RegistrosHorasController`, `SolicitudesExcepcionController`
- UI: `WebUI/Features/Registro`

**Acceso y Seguridad, FR16-FR21:**
- Dominio: roles en `Domain/Constants/Roles.cs`
- Aplicación: `Application/Features/Auth`, `Usuarios`
- Infraestructura: `Infrastructure/Identity`, `Infrastructure/Security`
- API: `AuthController`
- UI: `WebUI/Shared/Services/KpgAuthStateProvider.cs`

**Catálogos, FR22-FR26:**
- Dominio: `Empleado`, `Cliente`, `Proyecto`, `ParametroSistema`
- Aplicación: `Application/Features/Catalogos`
- API: `CatalogosController`
- UI: `WebUI/Features/Admin/Catalogos`

**Reportes, FR27-FR31:**
- Aplicación: `Application/Features/Reportes`
- Infraestructura: `Infrastructure/Reports/Excel`, `Infrastructure/Reports/Pdf`, `Persistence/Dapper`
- API: `ReportesController`
- UI: `WebUI/Features/Reportes`

**Dashboard, FR32-FR36:**
- Aplicación: `Application/Features/Dashboard`
- Infraestructura: queries Dapper optimizadas
- API: `DashboardController`
- UI: `WebUI/Features/Dashboard`

**Auditoría, FR37-FR40:**
- Dominio: `BitacoraAuditoria`
- Aplicación: `Application/Features/Auditoria`
- Infraestructura: `IBitacoraService` implementation
- API: `AuditoriaController`
- UI: `WebUI/Features/Admin/Auditoria`

### Integration Points

**Internal Communication:**
- WebUI -> API: REST JSON con JWT bearer.
- API -> Application: MediatR commands/queries.
- Application -> Infrastructure: interfaces inyectadas.
- Infrastructure -> SQL Server: EF Core para escritura, Dapper para lectura.

**External Integrations:**
- V1 no integra ERP, nómina ni facturación.
- Exportación digital se limita a archivos Excel/PDF generados por la API.

**Data Flow:**
- Registro: Blazor form -> API -> Command -> Domain validation -> EF Core -> Bitácora.
- Reporte: Blazor filtros -> API -> Query -> Dapper -> Excel/PDF service -> descarga.
- Login: Blazor login -> API auth -> Identity -> JWT en memoria + refresh token revocable.

### File Organization Patterns

**Configuration Files:**
- `Api/appsettings.json` contiene configuración no sensible.
- `Api/appsettings.Development.json` contiene valores locales.
- `appsettings.Production.json` queda en servidor y fuera del repo.
- `Directory.Packages.props` centraliza versiones NuGet.

**Source Organization:**
- Backend usa Clean Architecture: `Domain`, `Application`, `Infrastructure`, `Api`.
- Frontend Blazor vive en `src/WebUI`.
- Features se organizan por dominio funcional, no por tipo técnico.

**Test Organization:**
- Unit tests de dominio en `tests/Domain.UnitTests`.
- Unit tests de handlers y validators en `tests/Application.UnitTests`.
- Tests con SQL Server/Testcontainers o base controlada en `Infrastructure.IntegrationTests`.
- Tests de endpoints en `Api.IntegrationTests`.
- Component tests de Blazor en `WebUI.ComponentTests`.

**Asset Organization:**
- Assets Blazor en `WebUI/wwwroot`.
- CSS global y tema MudBlazor en `WebUI/Shared`.
- Documentos BMAD se mantienen en `_bmad-output/planning-artifacts`.

### Development Workflow Integration

**Development Server Structure:**
- API corre desde `Backend/src/Api`.
- WebUI corre desde `Backend/src/WebUI`.
- Ambos usan configuración local con CORS restringido a orígenes de desarrollo permitidos.

**Build Process Structure:**
- `dotnet build Backend/KPG.Timesheet.sln` compila toda la solución.
- `dotnet test Backend/KPG.Timesheet.sln` ejecuta pruebas.
- `dotnet publish` genera artefactos separados para API y WebUI.

**Deployment Structure:**
- API se publica en IIS + Kestrel como aplicación backend.
- WebUI se publica como sitio estático IIS separado.
- SQL Server aloja datos operativos, refresh tokens, auditoría y catálogos.
- Backups diarios se gestionan fuera de la app mediante SQL Server Agent.

---

## Architecture Validation Results

### Coherence Validation

**Decision Compatibility:**
La arquitectura es coherente: .NET 10, Clean Architecture, CQRS/MediatR, EF Core, Dapper, ASP.NET Core Identity, JWT, Blazor WASM, MudBlazor, MiniExcel y QuestPDF trabajan dentro de un ecosistema .NET consistente.

**Pattern Consistency:**
Los patrones definidos en el paso 5 soportan las decisiones centrales: naming consistente, controllers delgados, MediatR como frontera de aplicación, Problem Details para errores, Dapper solo lectura y EF Core para escritura.

**Structure Alignment:**
La estructura del paso 6 soporta las fronteras Domain/Application/Infrastructure/Api/WebUI y mapea cada grupo de FR a módulos concretos.

### Requirements Coverage Validation

**Feature Coverage:**
Las jornadas J1-J4 quedan cubiertas por los módulos Registro, SolicitudesExcepcion, Dashboard, Reportes, Admin, Catalogos y Auditoria.

**Functional Requirements Coverage:**
FR1-FR40 tienen ubicación arquitectónica explícita. No queda ningún grupo funcional sin módulo asignado.

**Non-Functional Requirements Coverage:**
Rendimiento, seguridad, confiabilidad, escalabilidad y usabilidad tienen soporte arquitectónico. La decisión de Blazor WASM exige cuidar compresión Brotli/gzip y pantalla de carga inicial.

### Implementation Readiness Validation

**Decision Completeness:**
Las decisiones críticas están documentadas con stack, versión o librería: SQL Server, EF Core, Dapper, Identity, JWT, refresh tokens, MudBlazor, ApexCharts, MiniExcel, QuestPDF, Serilog e IIS/Kestrel.

**Structure Completeness:**
La estructura es suficientemente concreta para que agentes implementen sin inventar carpetas principales.

**Pattern Completeness:**
Los puntos de conflicto principales están resueltos: naming, endpoints, DTOs, errores, validación, auditoría, soft delete, estado Blazor y boundaries.

### Gap Analysis Results

**Critical Gaps:**
Ninguno bloqueante.

**Important Gaps:**
- Existe un proyecto `Fronted/` Angular en el repo, mientras la arquitectura oficial define Blazor WASM. Debe tratarse como prototipo/legacy o decidir explícitamente cambiar la arquitectura.
- La estrategia exacta de pruebas e2e no está definida; hay component/integration tests, pero no herramienta e2e formal.
- El despliegue manual V1 está definido, pero falta script operativo de publicación.

**Minor Gaps:**
- Podría agregarse una convención más detallada para seeds iniciales de catálogos.
- Podría definirse una política de retención de logs y bitácora.
- Podría documentarse un checklist de seguridad previo a producción.

### Validation Issues Addressed

La validación confirma que no hay gaps críticos. Los gaps importantes se dejan como decisiones operativas de implementación y no bloquean la arquitectura: el estado de `Fronted/` debe resolverse antes de generar historias de frontend; e2e y despliegue pueden formalizarse como historias técnicas.

### Architecture Completeness Checklist

**Requirements Analysis**

- [x] Project context thoroughly analyzed
- [x] Scale and complexity assessed
- [x] Technical constraints identified
- [x] Cross-cutting concerns mapped

**Architectural Decisions**

- [x] Critical decisions documented with versions
- [x] Technology stack fully specified
- [x] Integration patterns defined
- [x] Performance considerations addressed

**Implementation Patterns**

- [x] Naming conventions established
- [x] Structure patterns defined
- [x] Communication patterns specified
- [x] Process patterns documented

**Project Structure**

- [x] Complete directory structure defined
- [x] Component boundaries established
- [x] Integration points mapped
- [x] Requirements to structure mapping complete

### Architecture Readiness Assessment

**Overall Status:** READY FOR IMPLEMENTATION

**Confidence Level:** high

**Key Strengths:**
- Arquitectura consistente con el PRD y UX spec.
- Separación clara entre dominio, aplicación, infraestructura, API y UI.
- Buen soporte para auditoría, RBAC, reportes y reglas de inmutabilidad.
- Patrones suficientemente específicos para coordinar varios agentes de implementación.

**Areas for Future Enhancement:**
- Formalizar e2e tests.
- Automatizar despliegue.
- Definir retención de logs/auditoría.
- Resolver explícitamente el estado del Angular existente.

### Implementation Handoff

**AI Agent Guidelines:**
- Follow all architectural decisions exactly as documented.
- Use implementation patterns consistently across all components.
- Respect project structure and boundaries.
- Refer to this document for all architectural questions.

**First Implementation Priority:**
Inicializar la solución `Backend/KPG.Timesheet.sln` con Clean Architecture + Blazor WASM, agregar paquetes base y dejar compilación/pruebas mínimas funcionando.
