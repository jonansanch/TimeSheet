# Arquitectura del Frontend — KPG Timesheet

**Versión:** 1.0  
**Fecha:** 2026-05-23  
**Stack:** Blazor WebAssembly · .NET 10 · MudBlazor 8.5 · ApexCharts · Microsoft.Extensions.Localization

---

## Tabla de Contenidos

1. [Visión General](#1-visión-general)
2. [Estructura de Carpetas](#2-estructura-de-carpetas)
3. [Punto de Entrada](#3-punto-de-entrada)
4. [Features — Módulos de Negocio](#4-features--módulos-de-negocio)
5. [Infrastructure — Repositorios HTTP](#5-infrastructure--repositorios-http)
6. [Shared — Componentes y Servicios Transversales](#6-shared--componentes-y-servicios-transversales)
7. [Layout y Navegación](#7-layout-y-navegación)
8. [Autenticación en el Frontend](#8-autenticación-en-el-frontend)
9. [Localización ES/EN](#9-localización-esen)
10. [Flujo de una Acción](#10-flujo-de-una-acción)
11. [Patrones y Convenciones](#11-patrones-y-convenciones)

---

## 1. Visión General

El frontend es una aplicación **Blazor WebAssembly (WASM)**. Esto significa que toda la aplicación — incluyendo el runtime de .NET — se descarga al navegador del usuario y corre completamente del lado del cliente, sin server-side rendering.

```
Navegador del usuario
┌──────────────────────────────────────────────┐
│  Blazor WASM (.NET 10 en WebAssembly)        │
│                                              │
│  ┌──────────┐   ┌────────────┐              │
│  │  Pages/  │   │ Components │              │
│  │ Razors   │──▶│  MudBlazor │              │
│  └──────────┘   └────────────┘              │
│        │                                     │
│  ┌─────▼──────────────┐                     │
│  │  Repositories      │ ← HTTP calls        │
│  │  (interfaces)      │─────────────────────┼──▶ Backend API
│  └────────────────────┘   JWT Bearer        │    (ASP.NET Core)
└──────────────────────────────────────────────┘
```

**Ventajas de esta arquitectura:**
- El servidor solo sirve archivos estáticos (HTML, JS, DLLs compiladas a WASM).
- Toda la navegación es instantánea (SPA — Single Page Application).
- La API solo necesita exponer endpoints REST; no renderiza HTML.

---

## 2. Estructura de Carpetas

```
Fronted/src/WebUI/
│
├── App.razor                  ← Router raíz + auth guards
├── Program.cs                 ← Startup: DI, cultura, HttpClient
├── _Imports.razor             ← Using globales para todos los .razor
├── KPG.Timesheet.WebUI.csproj ← Paquetes NuGet
│
├── Features/                  ← Módulos organizados por funcionalidad
│   ├── Auth/                  ← Login
│   ├── Registro/              ← Registro de jornada + historial
│   ├── Admin/                 ← Gestión de usuarios, catálogos, parámetros
│   ├── Dashboard/             ← Métricas y estado del equipo
│   ├── Reportes/              ← Reportes de horas
│   ├── Bitacora/              ← Bitácora de auditoría
│   └── Notificaciones/        ← Historial de notificaciones
│
├── Infrastructure/
│   └── Repositories/          ← Clientes HTTP hacia la API (interfaces + implementaciones)
│
├── Shared/
│   ├── Components/            ← Componentes reutilizables entre features
│   ├── Services/              ← Servicios transversales (auth, usuario actual)
│   ├── Models/                ← Modelos compartidos
│   ├── Constants/             ← Constantes (Roles)
│   └── Utils/                 ← Utilidades (cálculo días hábiles)
│
├── Layout/                    ← Layouts de la aplicación
│   ├── MainLayout.razor       ← Layout principal con drawer + AppBar
│   └── NavMenu.razor          ← Menú lateral de navegación
│
├── Pages/                     ← Páginas raíz (Home, NotFound)
│
├── Resources/                 ← Archivos de localización
│   ├── SharedResource.resx    ← Cadenas en español (idioma base)
│   └── SharedResource.en.resx ← Traducciones al inglés
│
└── wwwroot/                   ← Archivos estáticos
    ├── index.html             ← Página HTML host de la app Blazor
    ├── css/app.css            ← Estilos globales y personalización del nav
    └── images/                ← Imágenes (logo KPG)
```

---

## 3. Punto de Entrada

### 3.1 `index.html`

Página HTML que el servidor devuelve para cualquier ruta (el web.config de IIS redirige todo a este archivo). Dentro se carga el runtime de Blazor WASM:

```html
<script src="_framework/blazor.webassembly.js"></script>
```

También contiene el **helper de localización**:

```js
window.blazorCulture = {
    get: () => localStorage["blazor-culture"],
    set: (value) => localStorage["blazor-culture"] = value
};
```

Y la **pantalla de carga** (`#kpg-loading`) que se oculta cuando Blazor termina de inicializarse.

### 3.2 `Program.cs`

El startup de la aplicación. Se ejecuta una sola vez al cargar:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// 1. HttpClient apuntando a la API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]) });

// 2. MudBlazor
builder.Services.AddMudServices();

// 3. Auth personalizado
builder.Services.AddScoped<KpgAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<KpgAuthStateProvider>());
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<CurrentUserService>();

// 4. Repositorios (interfaces → implementaciones HTTP)
builder.Services.AddScoped<IRegistroHorasRepository, RegistroHorasRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
// ... todos los repositorios

// 5. Localización
builder.Services.AddLocalization();

var app = builder.Build();

// 6. Leer cultura guardada en localStorage antes de renderizar
var js = app.Services.GetRequiredService<IJSRuntime>();
var culture = await js.InvokeAsync<string?>("blazorCulture.get");
if (!string.IsNullOrEmpty(culture))
{
    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
}

await app.RunAsync();
```

### 3.3 `App.razor`

Raíz de la aplicación. Configura el router y los guards de autenticación:

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @{
                        if (context.User.Identity?.IsAuthenticated == true)
                            Navigation.NavigateTo("/");      // Rol sin permiso → home
                        else
                            Navigation.NavigateTo("/login"); // No autenticado → login
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

Aquí vive la lógica de **guard de rutas**: si un usuario no autenticado intenta acceder a cualquier página protegida con `[Authorize]`, es redirigido automáticamente a `/login`.

### 3.4 `_Imports.razor`

Define los `@using` globales que están disponibles en **todos** los archivos `.razor` sin necesidad de declararlos:

```razor
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using MudBlazor
@using KPG.Timesheet.WebUI.Infrastructure.Repositories.Interfaces
@using KPG.Timesheet.WebUI.Shared.Components
@using KPG.Timesheet.WebUI.Shared.Constants
@using Microsoft.Extensions.Localization
@using KPG.Timesheet.WebUI.Resources
```

---

## 4. Features — Módulos de Negocio

Cada feature agrupa sus **Páginas** y **Componentes** específicos. Una Página (`@page "/ruta"`) es un componente raíz enrutable. Un Componente es una pieza reutilizable dentro de una página.

### 4.1 Auth

```
Features/Auth/
├── Layout/LoginLayout.razor   ← Layout sin menú lateral (solo AppBar con selector de idioma)
└── Pages/LoginPage.razor      ← Formulario email + contraseña
```

`LoginPage` llama a `IAuthRepository.LoginAsync()`. Al tener éxito, el token JWT se guarda en `localStorage` y `KpgAuthStateProvider` notifica a Blazor el cambio de estado de autenticación, lo que desencadena re-render de toda la app.

### 4.2 Registro de Jornada

```
Features/Registro/
├── Pages/
│   ├── RegistroPage.razor          ← @page "/registro"
│   └── HistorialPage.razor         ← @page "/mis-registros"
└── Components/
    ├── KpgShiftForm.razor          ← Formulario completo de turno AM/PM + campos comunes
    ├── KpgRecentSuggestions.razor  ← Tarjetas de sugerencias recientes (autocompletar)
    ├── KpgExceptionDialog.razor    ← Diálogo para solicitar excepción de retroactividad
    └── KpgEditDescripcionDialog.razor ← Diálogo para editar descripción de un registro
```

**`KpgShiftForm`** es el componente central del sistema. Maneja:
- Selector de fecha con verificación de ventana de retroactividad
- Campos de hora para turnos AM y PM
- Selectores de Cliente/Proyecto (cascading: primero cliente, luego proyectos de ese cliente)
- Selector de Modalidad, Recurso, Lugar
- Campo de Descripción
- Lógica de upsert: llama al repositorio que hace POST a la API (la API hace upsert internamente)

### 4.3 Administración

```
Features/Admin/
├── Pages/
│   ├── UsuariosAdminPage.razor           ← @page "/admin/usuarios"
│   ├── EmpleadosAdminPage.razor          ← @page "/admin/empleados"
│   ├── ClientesAdminPage.razor           ← @page "/admin/clientes"
│   ├── ModalidadesAdminPage.razor        ← @page "/admin/parametros/modalidades"
│   ├── LugaresTrabajoAdminPage.razor     ← @page "/admin/parametros/lugares"
│   ├── ParametrosAdminPage.razor         ← @page "/admin/parametros/configuracion"
│   ├── SolicitudesExcepcionAdminPage.razor ← @page "/solicitudes-excepcion"
│   └── BitacoraAdminPage.razor           ← @page "/admin/bitacora"
└── Components/                           ← Diálogos de creación/edición (uno por catálogo)
    ├── KpgUserDialog.razor
    ├── KpgUserRoleDialog.razor
    ├── KpgClienteDialog.razor
    ├── KpgEmpleadoDialog.razor
    ├── KpgProyectoDialog.razor
    ├── KpgProyectosClienteDialog.razor
    ├── KpgLugarTrabajoDialog.razor
    └── KpgModalidadDialog.razor
```

Todas las páginas de administración siguen el mismo patrón:
1. Al inicializar (`OnInitializedAsync`), cargan la lista desde la API vía repositorio.
2. La lista se muestra en un `MudDataGrid` con búsqueda rápida y paginación.
3. Las acciones (crear, editar, activar/desactivar) abren un `MudDialog` con el componente correspondiente.
4. Al confirmar en el diálogo, se hace el POST/PUT/PATCH a la API y se actualiza la lista localmente (sin recargar todo).

### 4.4 Dashboard

```
Features/Dashboard/
├── Pages/DashboardPage.razor         ← @page "/dashboard"
└── Components/KpgTeamStatusCard.razor ← Tarjeta de estado del equipo con gráfico
```

El Dashboard adapta su contenido según el rol del usuario:
- **Supervisor:** estado diario del equipo (Completo/Parcial/Pendiente por colaborador)
- **Gerente:** lo anterior + horas por cliente y proyecto con selector de período
- **Admin:** lo anterior + métricas globales + lista de pendientes críticos

Usa **ApexCharts** para gráficos de barras y líneas.

### 4.5 Reportes

```
Features/Reportes/
├── Pages/ReportesPage.razor     ← @page "/reportes/horas"
└── Pages/TimesheetPage.razor    ← @page "/reportes/timesheet"
```

`ReportesPage` tiene filtros de período, empleado, cliente y proyecto. Al pulsar "Generar Reporte" llama al backend que devuelve los datos; los botones Exportar llaman a endpoints que devuelven un archivo binario (stream de bytes → descarga automática en el navegador).

### 4.6 Bitácora y Notificaciones

```
Features/Bitacora/Pages/BitacoraPage.razor          ← @page "/bitacora"
Features/Notificaciones/Pages/NotificacionesPage.razor ← @page "/notificaciones"
```

Páginas de solo lectura con filtros y exportación. La bitácora muestra todos los eventos de auditoría; las notificaciones muestran el historial de correos enviados.

---

## 5. Infrastructure — Repositorios HTTP

**Carpeta:** `Infrastructure/Repositories/`

Esta es la capa que hace las llamadas HTTP reales a la API del backend. Todo el resto de la aplicación solo conoce las **interfaces**.

### 5.1 Patrón Repository

```
Interfaces/
  IRegistroHorasRepository.cs    ← Contrato: qué operaciones están disponibles
  IClienteRepository.cs
  IAuthRepository.cs
  ... (23 interfaces en total)

Implementations/
  RegistroHorasRepository.cs     ← Implementación HTTP concreta
  ClienteRepository.cs
  AuthRepository.cs
  ... (15 implementaciones)
```

**¿Por qué este patrón?**
- Las páginas y componentes dependen de la interfaz, no de la implementación HTTP concreta.
- En tests se puede inyectar una implementación mock sin tocar el código de las páginas.
- Si cambia la URL de un endpoint, solo se modifica el repositorio, no las páginas.

### 5.2 Cómo funciona un repositorio

```csharp
public class RegistroHorasRepository : IRegistroHorasRepository
{
    private readonly HttpClient _http;

    public async Task<RegistroHorasDto?> CreateAsync(CreateRegistroRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/registros", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RegistroHorasDto>();
    }
}
```

El `HttpClient` tiene el JWT configurado automáticamente: `AuthStateService` inyecta el token en cada request via un `DelegatingHandler`.

### 5.3 Modelos (`Models/`)

DTOs que mapean 1:1 con los contratos de la API:

| Modelo | Uso |
|--------|-----|
| `AuthModels.cs` | LoginRequest, LoginResponse, RefreshTokenRequest |
| `RegistroHorasModels.cs` | CreateRegistroRequest, HistorialRegistroResponse |
| `ClienteModels.cs` | ClienteResponse, CreateClienteRequest |
| `UserAdminModels.cs` | UserAdminResponse, CreateUserRequest, ChangeUserRoleRequest |
| `DashboardModels.cs` | EstadoDiarioResponse, MetricasGlobalesResponse |
| `ReportesModels.cs` | ReporteHorasResponse, FiltrosReporteRequest |

---

## 6. Shared — Componentes y Servicios Transversales

### 6.1 Componentes Reutilizables (`Shared/Components/`)

| Componente | Para qué sirve |
|------------|---------------|
| `KpgLanguageSelector.razor` | Botón con bandera SVG inline que alterna ES/EN. Lee `CultureInfo.CurrentUICulture` para saber el idioma activo. Al cambiar guarda en `localStorage` y fuerza recarga. |
| `KpgStatCard.razor` | Tarjeta de métrica con ícono, valor numérico y etiqueta. Se usa en el dashboard para mostrar totales. |
| `KpgTableSkeleton.razor` | Placeholder animado que se muestra mientras carga una tabla. Evita saltos de layout. |
| `KpgSaveConfirmationBanner.razor` | Banner de confirmación de éxito que se auto-oculta. |

### 6.2 Servicios de Autenticación (`Shared/Services/`)

| Servicio | Responsabilidad |
|---------|----------------|
| `AuthStateService` | Persiste y recupera el JWT de `localStorage`. También inyecta el token en el `HttpClient` para que todas las llamadas salgan autenticadas. |
| `KpgAuthStateProvider` | Implementa `AuthenticationStateProvider` de Blazor. Blazor llama a este provider para saber si hay un usuario autenticado y cuáles son sus claims (email, roles). |
| `JwtParser` | Decodifica el payload del JWT (Base64) y extrae los claims sin necesidad de llamar al servidor. |
| `CurrentUserService` | Helper que expone `IsInRoleAsync(role)` y `GetUserIdAsync()` para consultas rápidas en componentes. |

### 6.3 Utilidades

**`BusinessDayCalculator.cs`** — calcula si una fecha está dentro de la ventana de retroactividad, excluyendo domingos del conteo de días hábiles.

**`Roles.cs`** — constantes de roles (`Admin`, `Gerente`, `Supervisor`, `Empleado`) compartidas con el backend (mismo namespace, mismos valores).

---

## 7. Layout y Navegación

### 7.1 MainLayout

```razor
<MudLayout>
    <MudAppBar>              ← Barra superior azul navy
        KPG Timesheet
        [email usuario]
        [KpgLanguageSelector]
        [Botón logout]
    </MudAppBar>

    <MudDrawer>              ← Menú lateral azul navy con logo KPG
        [Logo KPG]
        <NavMenu />
    </MudDrawer>

    <MudMainContent>
        @Body                ← Aquí se renderiza la página activa
    </MudMainContent>
</MudLayout>
```

### 7.2 NavMenu

El menú lateral adapta sus ítems según el rol del usuario gracias a `<AuthorizeView Roles="...">`:

```
Todos los roles:
  🏠 Inicio
  📅 Registro
  📋 Mis Registros

Supervisor / Gerente / Admin:
  📊 Dashboard
  📈 Reportes
      └─ Reporte de Horas
      └─ Timesheet
  🔔 Notificaciones

Supervisor / Gerente:
  🔍 Bitácora

Admin:
  ⚙️ Administración
      └─ Usuarios
      └─ Recursos
      └─ Clientes y Proyectos
      └─ Solicitudes Excepción
      └─ Bitácora Admin
  🛠️ Parámetros
      └─ Configuración General
      └─ Modalidades
      └─ Lugares de Trabajo
```

### 7.3 LoginLayout

Usado solo por `LoginPage.razor`. Es un layout mínimo sin menú lateral, con una `MudAppBar` transparente que solo contiene el `KpgLanguageSelector`.

---

## 8. Autenticación en el Frontend

### Flujo completo

```
1. Usuario ingresa email + contraseña en LoginPage
   → AuthRepository.LoginAsync() → POST /api/auth/login
   → Respuesta: { accessToken, refreshToken, expiresIn }

2. AuthStateService guarda en localStorage:
   localStorage["kpg_token"]         = accessToken
   localStorage["kpg_refresh_token"] = refreshToken

3. KpgAuthStateProvider notifica a Blazor:
   → JwtParser decodifica el accessToken
   → Extrae claims: userId, email, roles
   → Crea ClaimsPrincipal con esos claims
   → Blazor re-renderiza: el usuario está autenticado

4. Cada llamada HTTP subsiguiente:
   → AuthStateService inyecta header: "Authorization: Bearer <accessToken>"

5. Cuando el accessToken expira (60 min):
   → La API devuelve 401
   → AuthStateService llama a /api/auth/refresh con refreshToken
   → Obtiene nuevos tokens, los guarda, reintenta la petición original

6. Logout:
   → AuthRepository.LogoutAsync() → POST /api/auth/logout (invalida refresh en BD)
   → AuthStateService limpia localStorage
   → KpgAuthStateProvider notifica: usuario anónimo
   → App.razor redirige a /login
```

### Guard de rutas

Cualquier página con `@attribute [Authorize]` (o `@attribute [Authorize(Roles = "Admin")]`) es protegida automáticamente por el sistema de routing de Blazor. Si el usuario no tiene el rol requerido, `App.razor` lo redirige.

---

## 9. Localización ES/EN

El sistema soporta español e inglés con cambio en tiempo de ejecución.

### Archivos de recursos

```
Resources/
  SharedResource.cs         ← Clase marcadora (para que DI encuentre los recursos)
  SharedResource.resx       ← Strings en español (idioma base)
  SharedResource.en.resx    ← Strings en inglés
```

Las claves siguen la convención `Seccion_NombreDescriptivo`:

| Prefijo | Sección |
|---------|---------|
| `Nav_` | Navegación |
| `Btn_` | Botones |
| `Label_` | Etiquetas de campos |
| `Val_` | Mensajes de validación |
| `Reg_` | Registro de jornada |
| `Hist_` | Historial |
| `Dash_` | Dashboard |
| `Rep_` | Reportes |
| `Users_` | Gestión de usuarios |
| `Error_` | Mensajes de error |

### Uso en componentes

```razor
@inject IStringLocalizer<SharedResource> L

<MudButton>@L["Btn_Save"]</MudButton>
<MudTextField Label="@L["Label_Client"]" />

@* Para atributos que mezclan texto: *@
<MudTextField Placeholder="@($"{L["Label_Client"].Value}...")" />
```

### Cambio de idioma

`KpgLanguageSelector.razor`:
1. Lee el idioma actual de `CultureInfo.CurrentUICulture` (disponible sin JS).
2. Al hacer clic, guarda el nuevo idioma en `localStorage` via JSInterop.
3. Llama a `NavigationManager.NavigateTo(uri, forceLoad: true)` para recargar la app.
4. `Program.cs` lee `localStorage["blazor-culture"]` antes del primer render y establece la cultura.

---

## 10. Flujo de una Acción

Ejemplo: el supervisor aprueba una solicitud de excepción.

```
[SolicitudesExcepcionAdminPage.razor]
  Usuario hace clic en "Aprobar"
  → ConfirmarAprobarAsync(item) se ejecuta
  → Muestra MudMessageBox de confirmación
  → Usuario confirma
  → Llama ISolicitudExcepcionAdminRepository.AprobarAsync(item.Id)
        ↓
[SolicitudExcepcionAdminRepository.cs]
  → PATCH /api/solicitudes-excepcion/{id}/aprobar
  → Header: Authorization: Bearer <jwt>
        ↓
[Backend API] SolicitudesExcepcion.cs endpoint
  → Crea AprobarSolicitudExcepcionCommand { Id }
  → mediator.Send(command)
        ↓
[Backend Application] AprobarSolicitudExcepcionCommandHandler
  → Busca SolicitudExcepcion en BD
  → Cambia EstadoSolicitud → Aprobada
  → IBitacoraService.RegistrarEvento("ExcepcionAprobada", ...)
  → SaveChangesAsync()
        ↓
[Backend] Devuelve SolicitudExcepcionDto actualizada
        ↓
[Frontend] Repository devuelve el DTO
[SolicitudesExcepcionAdminPage.razor]
  → Actualiza el ítem en la lista local (sin recargar toda la página)
  → Muestra _successMessage = L["Exc_ApprovedMsg"]
```

---

## 11. Patrones y Convenciones

### Estructura de una Página

Todas las páginas siguen la misma estructura básica:

```razor
@page "/ruta"
@attribute [Authorize(Roles = Roles.Admin)]
@inject IAlgunRepositorio Repo
@inject IStringLocalizer<SharedResource> L

<PageTitle>@L["Clave_Titulo"] - KPG Timesheet</PageTitle>

<MudStack Spacing="3">
    <!-- Encabezado -->
    <MudText Typo="Typo.h4">@L["Clave_Titulo"]</MudText>

    <!-- Estado de carga -->
    @if (_isLoading) { <KpgTableSkeleton /> }
    else if (_errorMessage != null) { <MudAlert Severity="Severity.Error">@_errorMessage</MudAlert> }
    else { /* Contenido */ }
</MudStack>

@code {
    private bool _isLoading = true;
    private string? _errorMessage;
    private List<XxxDto> _items = [];

    protected override async Task OnInitializedAsync()
    {
        try { _items = await Repo.GetXxxAsync(); }
        catch { _errorMessage = L["Error_Load"]; }
        finally { _isLoading = false; }
    }
}
```

### Actualización Optimista de la UI

En lugar de recargar toda la lista desde la API después de una operación, las páginas actualizan la lista local directamente:

```csharp
// Tras crear un nuevo ítem:
_items.Add(nuevoItem);
_items = _items.OrderBy(x => x.Nombre).ToList();

// Tras modificar un ítem:
var idx = _items.IndexOf(itemAnterior);
if (idx >= 0) _items[idx] = itemActualizado;

// Tras eliminar un ítem:
_items.Remove(item);
```

Esto hace la UI más rápida al usuario (no hay espera de red para ver el resultado).

### Diálogos con MudDialog

Los formularios de creación/edición viven en diálogos separados (`KpgXxxDialog.razor`). La comunicación funciona así:

```csharp
// La página abre el diálogo:
var dialog = await DialogService.ShowAsync<KpgClienteDialog>("Nuevo Cliente", options);
var result = await dialog.Result;

// El diálogo cierra con el dato creado:
MudDialog.Close(DialogResult.Ok(nuevoCliente));

// La página recibe el resultado:
if (result?.Canceled != false || result.Data is not ClienteResponse cliente)
    return; // Usuario canceló
// Usar 'cliente' para actualizar la lista
```

### Roles y Autorización en Componentes

Para mostrar/ocultar elementos según el rol se usa `<AuthorizeView>`:

```razor
<AuthorizeView Roles="@Roles.Admin">
    <Authorized>
        <MudIconButton Icon="@Icons.Material.Filled.Delete" />
    </Authorized>
</AuthorizeView>
```

Para verificar el rol en código C#:

```csharp
_puedeEditar = await CurrentUserService.IsInRoleAsync(Roles.Supervisor)
            || await CurrentUserService.IsInRoleAsync(Roles.Admin);
```
