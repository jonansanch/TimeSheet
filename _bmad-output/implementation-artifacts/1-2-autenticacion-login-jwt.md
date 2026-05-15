# Story 1.2: Autenticación con Login y JWT

Status: review

## Story

As a usuario interno,
I want iniciar sesión con mis credenciales,
so that pueda acceder de forma segura a KPG Timesheet y el sistema valide mi identidad emitiendo un JWT firmado.

## Acceptance Criteria

1. **Given** un usuario activo con credenciales válidas, **When** envía usuario y contraseña a `POST /api/auth/login`, **Then** el API valida credenciales con ASP.NET Core Identity, **And** retorna un JWT firmado (expiración 60 min) y un refresh token válido (expiración 8h).

2. **Given** credenciales inválidas o usuario inactivo, **When** el usuario intenta iniciar sesión, **Then** el sistema rechaza el acceso con `401 Unauthorized` en formato Problem Details RFC 9457, **And** no expone información sensible sobre la causa exacta del rechazo (no indica si el usuario existe, si la contraseña es incorrecta o si la cuenta está inactiva).

3. **Given** la pantalla de login corporativo, **When** se muestra al usuario, **Then** incluye identidad KPG (logo/nombre), campo usuario, campo contraseña, botón "Iniciar sesión" y texto de recuperación orientado a contactar al Admin (no un link de self-service reset).

## Tasks / Subtasks

- [x] **T1 — Configurar JWT en appsettings y DI** (AC: 1)
  - [x] Agregar sección `Jwt` a `Backend/src/Api/appsettings.json`: `Key`, `Issuer`, `Audience`, `ExpirationMinutes` (60), `RefreshExpirationHours` (8)
  - [x] Agregar sección `Jwt` a `Backend/src/Api/appsettings.Development.json` con valores de desarrollo
  - [x] Crear `Backend/src/Application/Common/Models/JwtSettings.cs` (POCO options)
  - [x] En `Backend/src/Infrastructure/DependencyInjection.cs`: reemplazar `AddBearerToken(IdentityConstants.BearerScheme)` por `AddJwtBearer` con validación de `Issuer`, `Audience` y `IssuerSigningKey` (simétrica)
  - [x] Registrar `JwtSettings` via `IOptions<JwtSettings>` en DI
  - [x] Crear `Backend/src/Infrastructure/Identity/JwtTokenService.cs` que genere `JwtSecurityToken` con claims: `sub` (userId), `email`, `roles`
  - [x] Registrar `JwtTokenService` como `IJwtTokenService` en DI

- [x] **T2 — Crear entidad RefreshToken en Domain** (AC: 1)
  - [x] Crear `Backend/src/Domain/Entities/RefreshToken.cs` con propiedades: `Id` (Guid), `UserId` (string), `Token` (string — hash SHA-256), `ExpiresAt` (DateTime), `RevokedAt` (DateTime?), `IsRevoked` (bool computed), `CreatedAt` (DateTime)
  - [x] La entidad NO hereda de `BaseAuditableEntity` (append-only, no se actualiza excepto revocación)

- [x] **T3 — Actualizar ApplicationDbContext y Roles** (AC: 1)
  - [x] Agregar `DbSet<RefreshToken> RefreshTokens` a `ApplicationDbContext`
  - [x] Crear `Backend/src/Infrastructure/Data/Configurations/RefreshTokenConfiguration.cs` con índice en `Token` y `UserId`
  - [x] Actualizar `Backend/src/Domain/Constants/Roles.cs`: reemplazar `Administrator` por las 4 constantes KPG: `Admin`, `Gerente`, `Supervisor`, `Empleado`
  - [x] Actualizar `IApplicationDbContext` para incluir `DbSet<RefreshToken> RefreshTokens`

- [x] **T4 — Implementar LoginCommand en Application** (AC: 1, 2)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Login/LoginCommand.cs` (record con `Email` y `Password`)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Login/LoginCommandValidator.cs` (Email not empty, Password not empty)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Login/LoginResponseDto.cs` con `AccessToken`, `RefreshToken`, `ExpiresAt`, `UserId`, `Email`, `Roles`
  - [x] Crear `Backend/src/Application/Common/Interfaces/IJwtTokenService.cs`
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`:
    - Buscar usuario por email via `UserManager`
    - Si no existe o contraseña inválida → `UnauthorizedAccessException` con mensaje genérico
    - Si existe → generar JWT via `IJwtTokenService`
    - Generar refresh token (GUID → SHA-256 hash), guardar en `RefreshTokens` con expiración 8h
    - Retornar `LoginResponseDto`

- [x] **T5 — Crear Auth endpoint** (AC: 1, 2)
  - [x] Crear `Backend/src/Api/Endpoints/Auth.cs` con `POST /api/auth/login` que delega a `LoginCommand` via MediatR
  - [x] El endpoint es anónimo (`AllowAnonymous`)
  - [x] Respuesta 200 con `LoginResponseDto` en caso de éxito
  - [x] Eliminar o vaciar `MapIdentityApi<ApplicationUser>()` de `Users.cs` (reemplazar con endpoint custom, para evitar conflicto con `/login` de Identity)
  - [x] Registrar `UseAuthentication()` y `UseAuthorization()` en `Program.cs`

- [x] **T6 — Actualizar seeder con 4 roles KPG** (AC: 1)
  - [x] Actualizar `ApplicationDbContextInitialiser.TrySeedAsync()`:
    - Crear los 4 roles: `Admin`, `Gerente`, `Supervisor`, `Empleado`
    - Crear usuario admin `admin@kpg.com` / `Admin1234!` con rol `Admin`
    - Crear usuario empleado de prueba `empleado@kpg.com` / `Empleado1234!` con rol `Empleado`

- [x] **T7 — Configurar HttpClient Blazor → API** (AC: 3)
  - [x] Agregar sección `ApiSettings` a `Fronted/src/WebUI/wwwroot/appsettings.json` con `BaseUrl` apuntando a la API (https://localhost:7035)
  - [x] En `Fronted/src/WebUI/Program.cs`: reemplazar `builder.HostEnvironment.BaseAddress` por la URL de la API leída desde configuración
  - [x] Configurar `HttpClient` con `BaseAddress` de la API

- [x] **T8 — Crear LoginPage.razor** (AC: 3)
  - [x] Crear `Fronted/src/WebUI/Features/Auth/Pages/LoginPage.razor` con:
    - Layout sin sidebar (página pública)
    - Identidad KPG: nombre "KPG Timesheet" como título/encabezado
    - Campo `MudTextField` para usuario (email)
    - Campo `MudTextField` para contraseña (tipo password con toggle visibility)
    - Botón `MudButton` "Iniciar sesión" (se deshabilita durante carga)
    - Texto de recuperación: "¿Problemas para acceder? Contacte al Administrador del sistema."
    - Mensaje de error si las credenciales son inválidas
  - [x] Registrar ruta `/login` en `Routes.razor` o `App.razor`

- [x] **T9 — Crear AuthRepository y AuthStateService** (AC: 1, 3)
  - [x] Crear `Fronted/src/WebUI/Infrastructure/Repositories/IAuthRepository.cs` con método `LoginAsync(LoginRequest, CancellationToken)`
  - [x] Crear `Fronted/src/WebUI/Infrastructure/Repositories/AuthRepository.cs` que llama a `POST /api/auth/login`
  - [x] Crear `Fronted/src/WebUI/Shared/Services/AuthStateService.cs` (singleton) con:
    - `string? AccessToken` (en memoria, nunca en localStorage)
    - `bool IsAuthenticated`
    - `SetToken(token)` y `ClearToken()`
    - `event Action? OnAuthStateChanged`
  - [x] Registrar `IAuthRepository` y `AuthStateService` en DI
  - [x] `LoginPage.razor` usa `AuthRepository` para login y `AuthStateService` para guardar token → redirige a `/`

- [x] **T10 — Tests unitarios y verificación final** (AC: 1, 2)
  - [x] Crear `Backend/tests/Application.UnitTests/Features/Auth/LoginCommandValidatorTests.cs` con xUnit + FluentAssertions: email vacío falla, password vacío falla, datos válidos pasan
  - [x] `dotnet build Backend/KPG.Timesheet.sln` — 0 errores
  - [x] `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — 0 errores
  - [x] `dotnet test Backend/KPG.Timesheet.sln` — todos los tests pasan

## Dev Notes

### Estado inicial relevante de Story 1.1

- `Backend/` tiene solución `KPG.Timesheet.sln` con 4 proyectos fuente (Domain, Application, Infrastructure, Api) + 3 de test
- `Fronted/` tiene solución `KPG.Timesheet.WebUI.sln` con 1 proyecto (KPG.Timesheet.WebUI, Blazor WASM)
- ASP.NET Core Identity ya está configurado (`ApplicationUser : IdentityUser`, `AddIdentityCore`, `AddRoles<IdentityRole>`)
- `ApplicationDbContext : IdentityDbContext<ApplicationUser>` — ya tiene tablas de Identity
- Seeder actual crea solo rol `Administrator` y usuario `administrator@localhost` — debe actualizarse a 4 roles KPG
- `appsettings.json` tiene `DefaultConnection` con conexión a SQL Server (ya corregida en Story 1.1)
- `Users.cs` endpoint usa `MapIdentityApi<ApplicationUser>()` — debe reemplazarse con endpoint custom
- `System.IdentityModel.Tokens.Jwt 8.16.0` ya está en `Directory.Packages.props`

### Arquitectura JWT — detalles de implementación

**Generación del JWT:**
```csharp
// JwtTokenService — claims mínimos
var claims = new List<Claim>
{
    new(JwtRegisteredClaimNames.Sub, user.Id),
    new(JwtRegisteredClaimNames.Email, user.Email!),
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
};
// Agregar claims de roles
foreach (var role in roles)
    claims.Add(new Claim(ClaimTypes.Role, role));
```

**Validación JWT en DI (reemplaza AddBearerToken):**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
```

**Refresh token — generación:**
```csharp
var rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
// Guardar tokenHash en DB; retornar rawToken al cliente
```

### Roles KPG definitivos

| Constante | Valor | Descripción |
|-----------|-------|-------------|
| `Roles.Admin` | `"Admin"` | Administrador total |
| `Roles.Gerente` | `"Gerente"` | Reportes y métricas globales |
| `Roles.Supervisor` | `"Supervisor"` | Gestión de equipo |
| `Roles.Empleado` | `"Empleado"` | Registro de horas propio |

### Decisión sobre MapIdentityApi

`MapIdentityApi<ApplicationUser>()` en `Users.cs` expone endpoints de Identity en `/login`, `/register`, `/refresh`, etc. que usan el esquema Bearer interno (no JWT custom). Dado que Story 1.2 implementa endpoint JWT custom en `/api/auth/login`:

- **Eliminar** `MapIdentityApi<ApplicationUser>()` de `Users.cs`
- Los endpoints de Identity (register, confirm-email, etc.) no son necesarios en V1 (el Admin crea usuarios)
- `Users.cs` puede quedar vacía o eliminarse si no tiene otros endpoints aún

### Estructura de archivos nuevos — backend

```
Backend/src/
├── Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   └── IJwtTokenService.cs
│   │   └── Models/
│   │       └── JwtSettings.cs
│   └── Features/
│       └── Auth/
│           └── Commands/
│               └── Login/
│                   ├── LoginCommand.cs
│                   ├── LoginCommandHandler.cs
│                   ├── LoginCommandValidator.cs
│                   └── LoginResponseDto.cs
├── Domain/
│   ├── Constants/
│   │   └── Roles.cs (actualizar)
│   └── Entities/
│       └── RefreshToken.cs
└── Infrastructure/
    ├── Data/
    │   ├── ApplicationDbContext.cs (actualizar)
    │   ├── ApplicationDbContextInitialiser.cs (actualizar)
    │   └── Configurations/
    │       └── RefreshTokenConfiguration.cs
    └── Identity/
        └── JwtTokenService.cs
```

### Estructura de archivos nuevos — frontend

```
Fronted/src/WebUI/
├── Features/
│   └── Auth/
│       └── Pages/
│           └── LoginPage.razor
├── Infrastructure/
│   └── Repositories/
│       ├── IAuthRepository.cs
│       └── AuthRepository.cs
└── Shared/
    └── Services/
        └── AuthStateService.cs
```

### HttpClient en Blazor WASM

La configuración correcta:
```json
// wwwroot/appsettings.json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7035"
  }
}
```

```csharp
// Program.cs
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]!;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
```

### Program.cs — orden de middleware

```csharp
// Agregar ANTES de app.MapEndpoints:
app.UseAuthentication();
app.UseAuthorization();
```

### appsettings.json — sección Jwt

```json
"Jwt": {
  "Key": "KPG-Timesheet-Dev-Secret-Key-2026-MinLength32",
  "Issuer": "KPG.Timesheet.Api",
  "Audience": "KPG.Timesheet.WebUI",
  "ExpirationMinutes": 60,
  "RefreshExpirationHours": 8
}
```

**IMPORTANTE:** La Key de desarrollo puede estar en appsettings.Development.json. En producción NUNCA va en el repositorio (usar variable de entorno o appsettings.Production.json en servidor).

### Paquetes ya disponibles — no agregar nada nuevo

- `System.IdentityModel.Tokens.Jwt 8.16.0` — ya en `Directory.Packages.props`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` — ya referenciado
- Paquete JWT Bearer: `Microsoft.AspNetCore.Authentication.JwtBearer` — VERIFICAR si está en Directory.Packages.props; si no, agregar

### Alcance estricto de esta historia

**HACER en esta historia:**
- JWT login endpoint
- RefreshToken entity (sin endpoint de refresh — ese es 1.3)
- Login page Blazor
- AuthStateService básico (sin KpgAuthStateProvider completo — ese es 1.3)
- Actualizar Roles.cs a roles KPG

**NO HACER en esta historia (pertenece a historias futuras):**
- Endpoint de refresh token → Historia 1.3
- Endpoint de logout con revocación → Historia 1.3
- KpgAuthStateProvider completo → Historia 1.3
- Autorización por rol en endpoints → Historia 1.4
- Shell visual MudBlazor → Historia 1.5
- Endpoint de registro de usuarios → Historia 4.x (Admin crea usuarios)

### References

- [Architecture — Seguridad y Autenticación](../../_bmad-output/planning-artifacts/architecture.md#seguridad-y-autenticación)
- [Epics — Story 1.2](../../_bmad-output/planning-artifacts/epics.md#story-12-autenticación-con-login-y-jwt)
- [Architecture — Patrones de Nomenclatura](../../_bmad-output/planning-artifacts/architecture.md#patrones-de-nomenclatura)
- [Architecture — Patrones de Estructura CQRS](../../_bmad-output/planning-artifacts/architecture.md#patrones-de-estructura)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- `LoginLayout` no resolvía en `LoginPage.razor` — se agregó `@using KPG.Timesheet.WebUI.Features.Auth.Layout` a `_Imports.razor`
- `Microsoft.AspNetCore.Authentication.JwtBearer` no estaba en `Directory.Packages.props` — se agregó versión `10.0.5`
- `app.Run()` generaba warning CS6966 — reemplazado por `await app.RunAsync()`

### Completion Notes List

- JWT Bearer configurado en `DependencyInjection.cs` reemplazando `AddBearerToken(IdentityConstants.BearerScheme)`
- `JwtTokenService` genera tokens con claims: `sub`, `email`, `jti`, `roles` (como `ClaimTypes.Role`)
- `RefreshToken` entity creada en Domain (append-only, no hereda BaseAuditableEntity)
- `RefreshTokenConfiguration` crea índice único en `TokenHash` e índice en `UserId`
- `Roles.cs` actualizado a 4 roles KPG: `Admin`, `Gerente`, `Supervisor`, `Empleado`
- Seeder crea 4 roles + usuario `admin@kpg.com` + usuario `empleado@kpg.com` de prueba
- `LoginCommandHandler` genera SHA-256 del refresh token raw antes de persistir; retorna el token raw al cliente
- `Auth` endpoint registrado en `/api/auth` con `AllowAnonymous`; `MapIdentityApi` eliminado de `Users.cs`
- `UseAuthentication()` + `UseAuthorization()` agregados a `Program.cs`
- MudBlazor configurado en Blazor WASM (CSS/JS en index.html, `AddMudServices()` en Program.cs, tema KPG en App.razor)
- `LoginPage.razor` en `/login` con layout sin sidebar, KPG branding, toggle de contraseña y mensaje de recuperación
- `AuthStateService` singleton almacena JWT en memoria (nunca localStorage/sessionStorage)
- Build Backend: 0 errores, 0 advertencias
- Build Frontend: 0 errores, 0 advertencias
- Tests: 7 pasados (3 ValidationException + 4 LoginCommandValidator)

### File List

Backend/Directory.Packages.props (modificado)
Backend/src/Api/appsettings.json (modificado)
Backend/src/Api/appsettings.Development.json (creado)
Backend/src/Api/Program.cs (modificado)
Backend/src/Api/Endpoints/Auth.cs (creado)
Backend/src/Api/Endpoints/Users.cs (modificado)
Backend/src/Application/Common/Interfaces/IApplicationDbContext.cs (modificado)
Backend/src/Application/Common/Interfaces/IIdentityService.cs (modificado)
Backend/src/Application/Common/Interfaces/IJwtTokenService.cs (creado)
Backend/src/Application/Common/Models/JwtSettings.cs (creado)
Backend/src/Application/Features/Auth/Commands/Login/LoginCommand.cs (creado)
Backend/src/Application/Features/Auth/Commands/Login/LoginCommandHandler.cs (creado)
Backend/src/Application/Features/Auth/Commands/Login/LoginCommandValidator.cs (creado)
Backend/src/Application/Features/Auth/Commands/Login/LoginResponseDto.cs (creado)
Backend/src/Domain/Constants/Roles.cs (modificado)
Backend/src/Domain/Entities/RefreshToken.cs (creado)
Backend/src/Infrastructure/Data/ApplicationDbContext.cs (modificado)
Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs (modificado)
Backend/src/Infrastructure/Data/Configurations/RefreshTokenConfiguration.cs (creado)
Backend/src/Infrastructure/DependencyInjection.cs (modificado)
Backend/src/Infrastructure/Identity/IdentityService.cs (modificado)
Backend/src/Infrastructure/Identity/JwtTokenService.cs (creado)
Backend/src/Infrastructure/KPG.Timesheet.Infrastructure.csproj (modificado)
Backend/tests/Application.UnitTests/Features/Auth/LoginCommandValidatorTests.cs (creado)
Fronted/src/WebUI/_Imports.razor (modificado)
Fronted/src/WebUI/App.razor (modificado)
Fronted/src/WebUI/Program.cs (modificado)
Fronted/src/WebUI/Features/Auth/Layout/LoginLayout.razor (creado)
Fronted/src/WebUI/Features/Auth/Pages/LoginPage.razor (creado)
Fronted/src/WebUI/Infrastructure/Repositories/IAuthRepository.cs (creado)
Fronted/src/WebUI/Infrastructure/Repositories/AuthRepository.cs (creado)
Fronted/src/WebUI/Infrastructure/Repositories/Models/AuthModels.cs (creado)
Fronted/src/WebUI/Shared/Services/AuthStateService.cs (creado)
Fronted/src/WebUI/wwwroot/appsettings.json (creado)
Fronted/src/WebUI/wwwroot/appsettings.Development.json (creado)
Fronted/src/WebUI/wwwroot/index.html (modificado)

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2026-05-13 | 0.1 | Story creada e iniciada | claude-sonnet-4-6 |
| 2026-05-13 | 1.0 | Implementación completa — JWT, login endpoint, LoginPage Blazor | claude-sonnet-4-6 |
