# Story 1.3: Sesión Segura con Refresh Token y Logout

Status: review

## Story

As a usuario autenticado,
I want que mi sesión se renueve de forma segura y pueda cerrarla explícitamente,
so that no pierda trabajo durante el uso normal y pueda terminar mi sesión cuando corresponda.

## Acceptance Criteria

1. **Given** un usuario autenticado con refresh token vigente, **When** el JWT está por expirar o la app recarga, **Then** el sistema solicita renovación al endpoint `POST /api/auth/refresh`, **And** recibe un nuevo JWT si el refresh token no está expirado ni revocado.

2. **Given** un usuario autenticado, **When** cierra sesión, **Then** el refresh token activo queda revocado en servidor, **And** el JWT se elimina de memoria del cliente.

3. **Given** la política de seguridad aprobada, **When** el cliente almacena tokens, **Then** el JWT vive exclusivamente en memoria (campo `_accessToken` en `KpgAuthStateProvider`), **And** el refresh token se almacena en `sessionStorage` (se pierde al cerrar la pestaña), **And** ningún token se persiste en `localStorage`.

4. **Given** un usuario con sesión expirada o sin sesión, **When** navega a cualquier ruta protegida, **Then** es redirigido al login, **And** después de autenticarse regresa a la ruta original.

## Tasks / Subtasks

- [x] **T1 — Backend: RefreshToken command y endpoint** (AC: 1)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenCommand.cs` (record con `RefreshToken` string)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenCommandValidator.cs`
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenResponseDto.cs` (AccessToken, RefreshToken, ExpiresAt)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenCommandHandler.cs`:
    - Hashear el token recibido con SHA-256
    - Buscar en `RefreshTokens` por hash → si no existe o `!IsActive` → `UnauthorizedAccessException`
    - Obtener usuario y sus roles
    - Generar nuevo JWT via `IJwtTokenService`
    - Revocar el refresh token anterior (`token.Revoke()`)
    - Crear nuevo refresh token (rotación), guardar en BD
    - Retornar `RefreshTokenResponseDto`
  - [x] Agregar `MapPost(Refresh, "refresh").AllowAnonymous()` en `Auth.cs` endpoint
  - [x] El endpoint retorna 200 con nuevo JWT + nuevo refresh token

- [ ] **T2 — Backend: Logout command y endpoint** (AC: 2)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Logout/LogoutCommand.cs` (record con `RefreshToken` string)
  - [x] Crear `Backend/src/Application/Features/Auth/Commands/Logout/LogoutCommandHandler.cs`:
    - Hashear el token recibido
    - Buscar en `RefreshTokens` → si existe y no está revocado → `Revoke()` + `SaveChangesAsync`
    - Si no existe → simplemente retornar (idempotente)
  - [x] Agregar `MapPost(Logout, "logout").RequireAuthorization()` en `Auth.cs`
  - [x] El endpoint retorna 204 No Content

- [ ] **T3 — Backend: IApplicationDbContext soporte para queries de RefreshToken** (AC: 1, 2)
  - [x] Verificar que `IApplicationDbContext.RefreshTokens` permite búsqueda por hash (ya existe el DbSet; confirmar que el índice en `TokenHash` facilita la búsqueda)
  - [x] El handler de Refresh necesita llamar `FirstOrDefaultAsync` sobre `RefreshTokens` — confirmar que `Microsoft.EntityFrameworkCore` está disponible en Application

- [ ] **T4 — Frontend: KpgAuthStateProvider** (AC: 1, 3, 4)
  - [x] Crear `Fronted/src/WebUI/Shared/Services/KpgAuthStateProvider.cs`:
    - Hereda `AuthenticationStateProvider`
    - Inyecta `IAuthRepository`, `AuthStateService`, `IJSRuntime`
    - `_anonymousState` = `AuthenticationState` con `ClaimsPrincipal` vacío
    - `GetAuthenticationStateAsync()`: lee refresh token de `sessionStorage` → llama `/api/auth/refresh` → si OK, parsea JWT y crea `ClaimsPrincipal` con claims → retorna estado autenticado; si falla, retorna estado anónimo
    - Expone `LoginAsync(email, password)`: llama `IAuthRepository.LoginAsync` → si OK, guarda access token en `AuthStateService` (memoria) y refresh token en `sessionStorage` → notifica estado
    - Expone `LogoutAsync()`: llama `IAuthRepository.LogoutAsync` → limpia `AuthStateService` y `sessionStorage` → notifica estado anónimo
  - [x] Crear helper `JwtParser.cs` en `Shared/Services/` para parsear claims del JWT sin librería extra (Base64 decode del payload)

- [ ] **T5 — Frontend: Actualizar AuthRepository con Refresh y Logout** (AC: 1, 2)
  - [x] Agregar `RefreshAsync(refreshToken, CancellationToken)` a `IAuthRepository`
  - [x] Agregar `LogoutAsync(refreshToken, CancellationToken)` a `IAuthRepository`
  - [x] Implementar en `AuthRepository.cs`:
    - `RefreshAsync` → `POST /api/auth/refresh`
    - `LogoutAsync` → `POST /api/auth/logout` con header `Authorization: Bearer {token}` o body con refresh token

- [ ] **T6 — Frontend: Integrar KpgAuthStateProvider en DI y App** (AC: 3, 4)
  - [x] Registrar `KpgAuthStateProvider` en `Program.cs`:
    - `builder.Services.AddScoped<KpgAuthStateProvider>()`
    - `builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<KpgAuthStateProvider>())`
  - [x] Agregar `builder.Services.AddAuthorizationCore()` en `Program.cs`
  - [x] Actualizar `App.razor`: reemplazar `<RouteView>` por `<AuthorizeRouteView>` con `NotAuthorized` que redirige a `/login`
  - [x] Reemplazar el uso de `AuthStateService` en `LoginPage.razor` por llamada a `KpgAuthStateProvider.LoginAsync()`
  - [x] Actualizar `LoginPage.razor` para inyectar `KpgAuthStateProvider` en lugar de `AuthStateService` + `AuthRepository` directamente

- [ ] **T7 — Tests y verificación final** (AC: 1, 2, 3)
  - [x] Crear `Backend/tests/Application.UnitTests/Features/Auth/RefreshTokenCommandValidatorTests.cs`
  - [x] `dotnet build Backend/KPG.Timesheet.sln` — 0 errores
  - [x] `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — 0 errores
  - [x] `dotnet test Backend/KPG.Timesheet.sln` — todos los tests pasan

## Dev Notes

### Estado inicial (post Story 1.2)

- `RefreshToken` entity existe con `Revoke()` method, `IsActive` computed, índice en `TokenHash`
- `POST /api/auth/login` funciona y retorna `{ accessToken, refreshToken, expiresAt, userId, email, roles }`
- `AuthStateService` singleton almacena JWT en memoria (no sessionStorage)
- `LoginPage.razor` usa `AuthRepository.LoginAsync` directamente y guarda en `AuthStateService`
- `App.razor` usa `<RouteView>` (sin protección de rutas aún)

### Rotación de refresh tokens

Cada llamada a `/api/auth/refresh` revoca el token anterior y emite uno nuevo. Esto previene reutilización si un token es interceptado.

```
Cliente                          Servidor
  |-- POST /api/auth/refresh -->  |
  |   body: { refreshToken }      |
  |                               |-- Hash SHA-256 del token recibido
  |                               |-- Busca en RefreshTokens por hash
  |                               |-- Verifica IsActive
  |                               |-- Revoca el token actual
  |                               |-- Crea nuevo RefreshToken
  |                               |-- Genera nuevo JWT
  |<-- 200 { accessToken,        |
  |     refreshToken, expiresAt } |
```

### sessionStorage vs localStorage

| Criterio | sessionStorage | localStorage |
|----------|---------------|--------------|
| Alcance | Pestaña activa | Permanente |
| Revocado al cerrar tab | ✅ | ❌ |
| NFR7 cumplimiento | ✅ (solo refresh) | ❌ |

El **JWT** nunca toca sessionStorage — vive solo en `_accessToken` del `KpgAuthStateProvider`. El **refresh token** va en sessionStorage porque necesita sobrevivir recarga de página pero no cierre de pestaña.

### KpgAuthStateProvider — flujo al recargar

```
App inicia
  └─ GetAuthenticationStateAsync()
       └─ Leer refreshToken de sessionStorage
            ├─ No existe → retornar estado anónimo → usuario ve /login
            └─ Existe → POST /api/auth/refresh
                 ├─ 200 OK → parsear JWT → crear ClaimsPrincipal → estado autenticado
                 └─ Error → limpiar sessionStorage → estado anónimo
```

### JwtParser — parseo sin librería

```csharp
public static ClaimsPrincipal ParseJwt(string token)
{
    var parts = token.Split('.');
    if (parts.Length != 3) return new ClaimsPrincipal();
    
    var payload = parts[1];
    // Ajustar padding Base64
    payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
    var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
    
    var claims = new List<Claim>();
    var doc = JsonDocument.Parse(json);
    foreach (var prop in doc.RootElement.EnumerateObject())
    {
        if (prop.Name == "role" || prop.Name == ClaimTypes.Role)
        {
            if (prop.Value.ValueKind == JsonValueKind.Array)
                foreach (var item in prop.Value.EnumerateArray())
                    claims.Add(new Claim(ClaimTypes.Role, item.GetString()!));
            else
                claims.Add(new Claim(ClaimTypes.Role, prop.Value.GetString()!));
        }
        else
        {
            claims.Add(new Claim(prop.Name, prop.Value.ToString()));
        }
    }
    
    return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
}
```

### AuthorizeRouteView — redirección al login

```razor
<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
    <NotAuthorized>
        @{
            Navigation.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(Navigation.Uri)}", replace: true);
        }
    </NotAuthorized>
    <Authorizing>
        <MudProgressLinear Indeterminate="true" Color="Color.Primary" />
    </Authorizing>
</AuthorizeRouteView>
```

### Logout en AuthRepository — enviar refresh token en body

```csharp
// POST /api/auth/logout requiere Authorization header con JWT
// pero el propósito es revocar el refresh token → enviar en body
public async Task LogoutAsync(string refreshToken, string accessToken, CancellationToken ct = default)
{
    using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    request.Content = JsonContent.Create(new { refreshToken });
    await _http.SendAsync(request, ct);
}
```

### Alcance estricto de esta historia

**HACER:**
- Endpoints `/api/auth/refresh` y `/api/auth/logout`
- `KpgAuthStateProvider` con `GetAuthenticationStateAsync`, `LoginAsync`, `LogoutAsync`
- `AuthorizeRouteView` en `App.razor`
- JWT parser sin librería externa

**NO HACER (historias futuras):**
- Navegación lateral por rol → 1.4 / 1.5
- Shell MudBlazor (sidebar, top app bar) → 1.5
- Endpoints protegidos con `[Authorize(Roles="...")]` → 1.4

### References

- [Architecture — Ciclo de vida JWT en Blazor WASM](../../_bmad-output/planning-artifacts/architecture.md#seguridad-y-autenticación)
- [Epics — Story 1.3](../../_bmad-output/planning-artifacts/epics.md#story-13-sesión-segura-con-refresh-token-y-logout)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- `Microsoft.AspNetCore.Components.Authorization` no estaba referenciado en el proyecto Blazor WASM — agregado a `Fronted/Directory.Packages.props` y `KPG.Timesheet.WebUI.csproj`
- Las propiedades calculadas `IsRevoked`, `IsExpired`, `IsActive` de `RefreshToken` generarían error en EF — ignoradas explícitamente en `RefreshTokenConfiguration`

### Completion Notes List

- `POST /api/auth/refresh` implementado con rotación de tokens: revoca el anterior y emite uno nuevo
- `POST /api/auth/logout` implementado como idempotente (si el token no existe o ya está revocado, retorna 204 sin error)
- `KpgAuthStateProvider` restaura sesión al recargar desde sessionStorage + endpoint refresh
- `JwtParser` parsea JWT sin librería externa: Base64url decode del payload, extrae claims incluyendo arrays de roles
- `App.razor` usa `CascadingAuthenticationState` + `AuthorizeRouteView` con redirección a `/login?returnUrl=...`
- `LoginPage.razor` actualizado: inyecta `KpgAuthStateProvider` directamente, maneja `returnUrl` para post-login redirect
- Build Backend: 0 errores, 0 advertencias
- Build Frontend: 0 errores, 0 advertencias
- Tests: 9 pasados (3 ValidationException + 4 LoginCommandValidator + 2 RefreshTokenCommandValidator)

### File List

Backend/src/Api/Endpoints/Auth.cs (modificado)
Backend/src/Application/Common/Interfaces/IIdentityService.cs (modificado)
Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenCommand.cs (creado)
Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenCommandHandler.cs (creado)
Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenCommandValidator.cs (creado)
Backend/src/Application/Features/Auth/Commands/Refresh/RefreshTokenResponseDto.cs (creado)
Backend/src/Application/Features/Auth/Commands/Logout/LogoutCommand.cs (creado)
Backend/src/Application/Features/Auth/Commands/Logout/LogoutCommandHandler.cs (creado)
Backend/src/Infrastructure/Data/Configurations/RefreshTokenConfiguration.cs (modificado)
Backend/src/Infrastructure/Identity/IdentityService.cs (modificado)
Backend/tests/Application.UnitTests/Features/Auth/RefreshTokenCommandValidatorTests.cs (creado)
Fronted/Directory.Packages.props (modificado)
Fronted/src/WebUI/KPG.Timesheet.WebUI.csproj (modificado)
Fronted/src/WebUI/_Imports.razor (modificado)
Fronted/src/WebUI/App.razor (modificado)
Fronted/src/WebUI/Program.cs (modificado)
Fronted/src/WebUI/Features/Auth/Pages/LoginPage.razor (modificado)
Fronted/src/WebUI/Infrastructure/Repositories/IAuthRepository.cs (modificado)
Fronted/src/WebUI/Infrastructure/Repositories/AuthRepository.cs (modificado)
Fronted/src/WebUI/Infrastructure/Repositories/Models/AuthModels.cs (modificado)
Fronted/src/WebUI/Shared/Services/JwtParser.cs (creado)
Fronted/src/WebUI/Shared/Services/KpgAuthStateProvider.cs (creado)

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2026-05-13 | 0.1 | Story creada e iniciada | claude-sonnet-4-6 |
| 2026-05-13 | 1.0 | Implementación completa — refresh, logout, KpgAuthStateProvider | claude-sonnet-4-6 |
