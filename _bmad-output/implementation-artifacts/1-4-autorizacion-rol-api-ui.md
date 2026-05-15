# Story 1.4: Autorización por Rol en API y UI

Status: review

## Story

As a usuario de KPG,
I want ver y ejecutar solo las funcionalidades permitidas por mi rol,
so that la información y las acciones sensibles queden protegidas independientemente de cómo se acceda al sistema.

## Acceptance Criteria

1. **Given** un usuario con rol Empleado, Supervisor, Gerente o Admin, **When** solicita un endpoint protegido, **Then** el API verifica el rol con autorización server-side, **And** rechaza accesos no permitidos con `403 Forbidden` en formato Problem Details RFC 9457 aunque el frontend haya sido manipulado.

2. **Given** un usuario autenticado, **When** se renderiza la navegación principal, **Then** solo aparecen las secciones permitidas para su rol, **And** las secciones no permitidas simplemente no se renderizan (no aparecen deshabilitadas).

3. **Given** endpoints protegidos de la Épica 1, **When** se consulta `GET /api/auth/me`, **Then** retorna el userId, email y roles del usuario autenticado, **And** requiere token JWT válido (401 si no hay token, 403 si no tiene el rol requerido).

## Tasks / Subtasks

- [x] **T1 — Backend: endpoint GET /api/auth/me** (AC: 1, 3)
  - [x] Agregar `MapGet(Me, "me").RequireAuthorization()` en `Auth.cs`
  - [x] Implementar handler `Me` que lee claims del `HttpContext.User` y retorna `{ userId, email, roles }`
  - [x] Crear `MeResponseDto` con `UserId`, `Email`, `Roles`

- [x] **T2 — Backend: políticas de autorización por rol** (AC: 1)
  - [x] En `Backend/src/Api/DependencyInjection.cs` (o `Program.cs`): agregar políticas nombradas usando `Roles.*` constantes
  - [x] Verificar que respuestas 401/403 usan Problem Details (el `ProblemDetailsExceptionHandler` ya existe — confirmar que captura `ForbidResult`)
  - [x] Agregar `[Authorize(Roles = Roles.Admin)]` de ejemplo en `Users.cs` endpoint (vacío) para demostrar el patrón

- [x] **T3 — Frontend: ICurrentUserService** (AC: 2)
  - [x] Crear `Fronted/src/WebUI/Shared/Services/CurrentUserService.cs` que:
    - Inyecta `AuthenticationStateProvider`
    - Expone `Task<bool> IsInRoleAsync(string role)`
    - Expone `Task<string?> GetEmailAsync()`
    - Expone `Task<IReadOnlyList<string>> GetRolesAsync()`

- [x] **T4 — Frontend: navegación básica con AuthorizeView por rol** (AC: 2)
  - [x] Actualizar `Fronted/src/WebUI/Layout/NavMenu.razor`:
    - Usar `<AuthorizeView>` para mostrar ítems condicionalmente por rol
    - Ítem "Registro" → roles Empleado, Supervisor (todos los autenticados)
    - Ítem "Dashboard" → roles Supervisor, Gerente, Admin
    - Ítem "Reportes" → roles Supervisor, Gerente, Admin
    - Ítem "Administración" → solo Admin
  - [x] Marcar las páginas existentes con `@attribute [Authorize]` o dejar solo la ruta raíz protegida por `AuthorizeRouteView`

- [x] **T5 — Frontend: página Home con bienvenida por rol** (AC: 2)
  - [x] Actualizar `Fronted/src/WebUI/Pages/Home.razor`:
    - Mostrar saludo con email del usuario autenticado
    - Usar `<AuthorizeView Roles="@Roles.Admin">` para mostrar sección exclusiva de Admin
    - Usar `<AuthorizeView Roles="@Roles.Supervisor,@Roles.Gerente,@Roles.Admin">` para secciones de supervisión
    - Botón "Cerrar sesión" que llama `KpgAuthStateProvider.LogoutAsync()`

- [x] **T6 — Tests y verificación final** (AC: 1, 2, 3)
  - [x] `dotnet build Backend/KPG.Timesheet.sln` — 0 errores
  - [x] `dotnet build Fronted/KPG.Timesheet.WebUI.sln` — 0 errores
  - [x] `dotnet test Backend/KPG.Timesheet.sln` — todos los tests pasan

## Dev Notes

### Estado inicial (post Story 1.3)

- JWT Bearer configurado, `UseAuthentication()` + `UseAuthorization()` en pipeline
- `KpgAuthStateProvider` construye `ClaimsPrincipal` con claims incluyendo roles via `JwtParser`
- `CascadingAuthenticationState` + `AuthorizeRouteView` en `App.razor` — rutas sin `[Authorize]` son públicas
- `Roles.cs` define: `Admin`, `Gerente`, `Supervisor`, `Empleado`

### Patrón de autorización en endpoints — regla general para Epic 2+

```csharp
// Endpoint de empleado propio
groupBuilder.MapGet(handler, "pattern").RequireAuthorization();

// Endpoint solo Admin
groupBuilder.MapDelete(handler, "{id}").RequireAuthorization(Roles.Admin);

// Endpoint Supervisor o Admin
groupBuilder.MapGet(handler, "equipo").RequireAuthorization(policy: "SupervisorOrAbove");
```

### AuthorizeView en componentes Blazor

```razor
<AuthorizeView>
    <Authorized><!-- visible si autenticado --></Authorized>
    <NotAuthorized><!-- visible si no autenticado --></NotAuthorized>
</AuthorizeView>

<AuthorizeView Roles="Admin">
    <p>Solo Admin ve esto</p>
</AuthorizeView>

<AuthorizeView Roles="Supervisor,Gerente,Admin">
    <p>Roles con acceso a supervisión</p>
</AuthorizeView>
```

### Problem Details para 401/403

El `ProblemDetailsExceptionHandler` ya está configurado. Para que 401/403 de authorization middleware también usen Problem Details, agregar en `Program.cs`:

```csharp
app.UseStatusCodePages(async ctx =>
{
    var response = ctx.HttpContext.Response;
    if (response.StatusCode is 401 or 403)
    {
        response.ContentType = "application/problem+json";
        await response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9457",
            title = response.StatusCode == 401 ? "No autenticado." : "Acceso denegado.",
            status = response.StatusCode
        });
    }
});
```

### Roles compartidos entre Backend y Frontend

Los valores de rol deben ser consistentes. En Backend están en `KPG.Timesheet.Domain.Constants.Roles`. En Frontend, definir las mismas constantes en un archivo compartido `Shared/Constants/Roles.cs`:

```csharp
namespace KPG.Timesheet.WebUI.Shared.Constants;
public static class Roles
{
    public const string Admin = "Admin";
    public const string Gerente = "Gerente";
    public const string Supervisor = "Supervisor";
    public const string Empleado = "Empleado";
}
```

### Alcance estricto de esta historia

**HACER:**
- `GET /api/auth/me`
- Problem Details para 401/403 del authorization middleware
- Constantes de roles en Frontend
- NavMenu básico con `<AuthorizeView>` por rol
- Home page con saludo por rol y botón de logout

**NO HACER (historias futuras):**
- Shell MudBlazor completo (sidebar fijo, top app bar) → 1.5
- Endpoints de dominio con roles específicos → Épicas 2–6
- Pantalla de carga Blazor → 1.5 o 1.6

### References

- [Architecture — RBAC](../../_bmad-output/planning-artifacts/architecture.md#seguridad-y-autenticación)
- [Epics — Story 1.4](../../_bmad-output/planning-artifacts/epics.md#story-14-autorización-por-rol-en-api-y-ui)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- `FindFirstValue` no existe en Blazor WASM — reemplazado con `FindFirst()?.Value` en `CurrentUserService.cs`
- `@using Microsoft.AspNetCore.Authorization` añadido a `_Imports.razor` para que `[Authorize]` compile en páginas Razor

### Completion Notes List

- T1: `GET /api/auth/me` implementado en `Auth.cs`; `MeResponseDto` creado; claims leídos de `HttpContext.User` (Sub/NameIdentifier, email, Role)
- T2: `UseStatusCodePages` en `Program.cs` produce Problem Details RFC 9457 para 401 y 403 del authorization middleware; `Users.cs` limpiado (sin código comentado)
- T3: `CurrentUserService` (scoped) y `Roles.cs` creados; registrados en DI; usando añadido a `_Imports.razor`
- T4: `NavMenu.razor` actualizado con `<AuthorizeView Roles="...">` por rol; logout llama `AuthProvider.LogoutAsync()`
- T5: `Home.razor` actualizado con `@attribute [Authorize]`, email del usuario, cards MudBlazor por rol (Registro / Dashboard+Reportes / Administración)
- T6: Backend 0 errores 0 advertencias; Frontend 0 errores 0 advertencias; 9/9 unit tests pasan

### File List

**Backend:**
- `Backend/src/Api/Endpoints/Auth.cs` — añadido `GET /api/auth/me` + handlers logout/me con Problem Details
- `Backend/src/Api/Endpoints/Users.cs` — limpiado (sin código comentado)
- `Backend/src/Api/Program.cs` — añadido `UseStatusCodePages` para 401/403
- `Backend/src/Application/Features/Auth/Queries/Me/MeResponseDto.cs` — nuevo

**Frontend:**
- `Fronted/src/WebUI/Pages/Home.razor` — actualizado con Authorize + MudBlazor cards por rol
- `Fronted/src/WebUI/Layout/NavMenu.razor` — AuthorizeView por rol + logout
- `Fronted/src/WebUI/Shared/Services/CurrentUserService.cs` — nuevo (corregido FindFirst)
- `Fronted/src/WebUI/Shared/Constants/Roles.cs` — nuevo
- `Fronted/src/WebUI/_Imports.razor` — añadidos usings Authorization + servicios + constantes

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2026-05-13 | 0.1 | Story creada e iniciada | claude-sonnet-4-6 |
| 2026-05-13 | 1.0 | Implementación completa — todos los ACs satisfechos | claude-sonnet-4-6 |
