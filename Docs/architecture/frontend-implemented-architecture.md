# Arquitectura Frontend Implementada

Fecha de revision: 2026-05-20

## Resumen ejecutivo

El frontend esta implementado como una aplicacion Blazor WebAssembly de un solo proyecto:

```text
Fronted/src/WebUI/KPG.Timesheet.WebUI.csproj
```

La arquitectura actual se organiza por:

- `Features`: paginas y componentes por modulo funcional.
- `Infrastructure/Repositories`: clientes HTTP hacia el backend.
- `Infrastructure/Repositories/Models`: DTOs de request/response usados por los repositorios.
- `Shared`: servicios transversales, componentes comunes, constantes, modelos y utilidades.
- `Layout`: layout principal y menu lateral.
- `wwwroot`: assets, configuracion del cliente, CSS global y archivos estaticos.

El flujo principal es:

```text
Blazor Page/Component
  -> Repository interface
  -> Repository implementation
  -> HttpClient + Bearer token
  -> Backend API
  -> UI state / MudBlazor components
```

La estructura esta funcionando y es consistente para un MVP. El punto mas claro a mejorar es que `Infrastructure/Repositories` concentra todos los clientes HTTP y modelos de todos los modulos en una carpeta plana. A medida que crece, conviene mover esos repositorios/modelos debajo de cada `Feature` o crear una estructura espejo por modulo.

## Proyecto y dependencias

### Proyecto unico

```text
Fronted/src/WebUI/
  KPG.Timesheet.WebUI.csproj
```

Paquetes principales:

- `Microsoft.AspNetCore.Components.WebAssembly`
- `Microsoft.AspNetCore.Components.Authorization`
- `MudBlazor`
- `Blazor-ApexCharts`

Observacion:

Es una SPA Blazor WebAssembly. No hay separacion por proyectos tipo `WebUI.Application` o `WebUI.Infrastructure`; la separacion actual es por carpetas dentro de un solo proyecto.

## Composicion de la app

### `Program.cs`

Responsabilidad:

- Configura Blazor WASM.
- Lee `ApiSettings:BaseUrl`.
- Registra `HttpClient`.
- Registra MudBlazor y ApexCharts.
- Registra autorizacion cliente.
- Registra repositorios HTTP.
- Registra servicios de autenticacion y usuario actual.

Flujo de DI actual:

```text
IAuthRepository -> AuthRepository
IRegistroHorasRepository -> RegistroHorasRepository
IParametroSistemaRepository -> ParametroSistemaRepository
ISolicitudExcepcionRepository -> SolicitudExcepcionRepository
ISolicitudExcepcionAdminRepository -> SolicitudExcepcionAdminRepository
IUserAdminRepository -> UserAdminRepository
IEmpleadoRepository -> EmpleadoRepository
IClienteRepository -> ClienteRepository
IModalidadRepository -> ModalidadRepository
ILugarTrabajoRepository -> LugarTrabajoRepository
IDashboardRepository -> DashboardRepository
IReportesRepository -> ReportesRepository
INotificacionesRepository -> NotificacionesRepository
IBitacoraAdminRepository -> BitacoraAdminRepository
IBitacoraRepository -> BitacoraRepository
```

Servicios de auth:

```text
AuthStateService        -> singleton en memoria
KpgAuthStateProvider   -> scoped
AuthenticationStateProvider -> KpgAuthStateProvider
CurrentUserService     -> scoped
```

### `App.razor`

Responsabilidad:

- Configura tema MudBlazor global.
- Declara providers globales:
  - `MudThemeProvider`
  - `MudPopoverProvider`
  - `MudDialogProvider`
  - `MudSnackbarProvider`
- Configura `CascadingAuthenticationState`.
- Usa `AuthorizeRouteView` para proteger rutas.
- Redirige usuarios no autorizados a `/login?returnUrl=...`.

Tema actual:

- Primary: azul KPG `#0D3B5E`.
- Secondary: celeste `#5BB8D4`.
- Warning: amarillo `#FFD300`.
- Tipografias: `Poppins` para titulos, `Roboto` para texto.

Observacion:

El tema vive inline en `App.razor`. Es practico, pero si crece podria moverse a `Shared/Theming/KpgTheme.cs`.

## Organizacion de carpetas

### Estructura actual

```text
Fronted/src/WebUI/
  Features/
    Admin/
      Components/
      Pages/
    Auth/
      Layout/
      Pages/
    Bitacora/
      Pages/
    Dashboard/
      Components/
      Pages/
    Notificaciones/
      Pages/
    Registro/
      Components/
      Pages/
    Reportes/
      Pages/

  Infrastructure/
    Repositories/
      Models/

  Layout/
  Pages/
  Shared/
    Components/
    Constants/
    Layout/
    Models/
    Services/
    Utils/

  wwwroot/
    css/
    lib/
    sample-data/
```

### `Features`

Responsabilidad:

- Agrupar UI por modulo funcional.
- Contener paginas `.razor` con rutas.
- Contener componentes especificos de cada modulo.

Modulos actuales:

- `Auth`: login y layout de login.
- `Registro`: registro de horas, historial y componentes del formulario.
- `Admin`: usuarios, empleados, clientes, parametros, solicitudes y bitacora admin.
- `Dashboard`: dashboard y cards.
- `Reportes`: pagina de reportes y exportaciones.
- `Notificaciones`: historial/consulta de notificaciones.
- `Bitacora`: bitacora para Supervisor/Gerente.

Patron actual:

```text
Features/{Modulo}/Pages/{Pagina}.razor
Features/{Modulo}/Components/{Componente}.razor
```

Observacion:

Esta parte esta bastante bien. Las paginas y componentes visuales viven cerca del modulo funcional.

### `Infrastructure/Repositories`

Responsabilidad:

- Encapsular llamadas HTTP al backend.
- Adjuntar `Authorization: Bearer`.
- Transformar query strings.
- Leer JSON o bytes de descarga.
- Exponer interfaces consumidas por paginas/componentes.

Ejemplos:

- `AuthRepository`
- `RegistroHorasRepository`
- `ReportesRepository`
- `BitacoraAdminRepository`
- `BitacoraRepository`
- `DashboardRepository`
- `UserAdminRepository`

Patron actual:

```text
Infrastructure/Repositories/
  IRegistroHorasRepository.cs
  RegistroHorasRepository.cs
  IReportesRepository.cs
  ReportesRepository.cs
  Models/
    RegistroHorasModels.cs
    ReportesModels.cs
```

Observacion:

La carpeta es funcional, pero plana. Ya hay muchos repositorios y muchos archivos de modelos mezclados. Esta es la principal candidata a reorganizacion.

### `Shared`

Responsabilidad:

- Componentes comunes reutilizables.
- Servicios transversales.
- Constantes compartidas.
- Modelos/utilidades no ligados a una sola feature.

Subcarpetas actuales:

```text
Shared/
  Components/
  Constants/
  Models/
  Services/
  Utils/
```

Ejemplos:

- `Shared/Components/KpgTableSkeleton.razor`
- `Shared/Components/KpgSaveConfirmationBanner.razor`
- `Shared/Components/KpgStatCard.razor`
- `Shared/Constants/Roles.cs`
- `Shared/Services/KpgAuthStateProvider.cs`
- `Shared/Services/AuthStateService.cs`
- `Shared/Services/CurrentUserService.cs`
- `Shared/Utils/BusinessDayCalculator.cs`

Observacion:

`Shared` esta bien usado para piezas comunes. Hay una carpeta `Shared/Layout` vacia o no usada en el mapa actual, mientras el layout real esta en `Layout/`.

### `Layout`

Responsabilidad:

- Shell principal autenticado.
- AppBar.
- Drawer lateral.
- Menu por roles.

Archivos:

- `Layout/MainLayout.razor`
- `Layout/MainLayout.razor.css`
- `Layout/NavMenu.razor`
- `Layout/NavMenu.razor.css`

Rutas visibles por rol segun `NavMenu.razor`:

| Rol | Rutas |
|---|---|
| Todos autenticados | `/` |
| Empleado/Supervisor/Gerente/Admin | `/registro`, `/mis-registros` |
| Supervisor/Gerente/Admin | `/dashboard`, `/reportes`, `/notificaciones` |
| Supervisor/Gerente | `/bitacora` |
| Admin | `/admin/usuarios`, `/admin/empleados`, `/admin/clientes`, `/solicitudes-excepcion`, `/admin/bitacora`, `/admin/parametros` |

Observacion:

El control visual por rol se hace en el menu con `AuthorizeView`. La proteccion real de pagina tambien se declara con `[Authorize]` en cada pagina sensible.

### `Pages`

Responsabilidad:

- Paginas base o de plantilla.

Archivos actuales:

- `Home.razor`
- `NotFound.razor`
- `Counter.razor`
- `Weather.razor`

Observacion:

`Counter.razor`, `Weather.razor` y `wwwroot/sample-data/weather.json` parecen restos de plantilla Blazor. Si ya no se usan, son candidatos a limpieza.

## Autenticacion y sesion

### Componentes principales

```text
LoginPage.razor
  -> KpgAuthStateProvider.LoginAsync
  -> AuthRepository.LoginAsync
  -> api/auth/login
```

### Estado de auth

`AuthStateService` mantiene el access token en memoria:

```text
AccessToken
IsAuthenticated
OnAuthStateChanged
```

`KpgAuthStateProvider`:

- Construye el `ClaimsPrincipal` desde el JWT.
- Guarda el refresh token en `sessionStorage` con key `kpg_rt`.
- Intenta restaurar sesion usando `api/auth/refresh`.
- Limpia sesion en logout.
- Notifica cambios con `NotifyAuthenticationStateChanged`.

`JwtParser`:

- Decodifica payload JWT.
- Convierte roles en claims `ClaimTypes.Role`.
- Soporta rol como array o valor unico.

Observacion:

El access token no se persiste, solo vive en memoria. El refresh token vive en `sessionStorage`. Es una decision razonable para una SPA interna.

## Capa HTTP / Repositories

### Patron de llamadas autenticadas

La mayoria de repositorios hacen:

```text
1. Validar AuthStateService.AccessToken
2. Crear HttpRequestMessage
3. Agregar Authorization: Bearer {token}
4. Enviar con HttpClient
5. Retornar DTO, lista vacia, bool o null segun caso
```

Ejemplo:

```text
RegistroHorasRepository.CreateAsync
  -> POST api/registros-horas
  -> retorna RegistroHorasResponse? o null
```

### Descargas

`ReportesRepository` y `BitacoraAdminRepository` retornan:

```text
(byte[] Contenido, string ContentType, string FileName)?
```

Las paginas usan `IJSRuntime` para llamar `downloadFile`.

Observacion:

La logica de bearer token esta repetida en todos los repositorios. Podria extraerse a un `AuthorizedHttpClient` o a un helper interno.

## UI y componentes

### MudBlazor como sistema visual

El frontend usa MudBlazor de forma consistente:

- `MudLayout`, `MudAppBar`, `MudDrawer`.
- `MudPaper`, `MudStack`, `MudGrid`.
- `MudForm`, `MudTextField`, `MudSelect`, `MudDatePicker`, `MudTimePicker`.
- `MudDataGrid` para tablas.
- `MudDialog` para modales.
- `MudAlert`, `MudChip`, `MudProgressCircular`.

### Componentes comunes

`Shared/Components`:

- `KpgTableSkeleton`
- `KpgSaveConfirmationBanner`
- `KpgStatCard`

`Features/Registro/Components`:

- `KpgShiftForm`
- `KpgDatePicker`
- `KpgExceptionDialog`
- `KpgRecentSuggestions`
- `KpgEditDescripcionDialog`

`Features/Admin/Components`:

- Dialogs para usuario, rol, empleado, cliente, proyecto, modalidad y lugar.

Observacion:

El patron de componentes especificos por feature esta bien. El formulario de registro (`KpgShiftForm`) concentra bastante logica: carga catalogos, validacion, sugerencias, excepciones, semaforo de fecha y guardado AM/PM. Si crece mas, convendria extraer un pequeño view model/service de UI para reducir codigo en el `.razor`.

## Configuracion y assets

### Configuracion cliente

Archivos:

```text
wwwroot/appsettings.json
wwwroot/appsettings.Development.json
```

`Program.cs` lee:

```text
ApiSettings:BaseUrl
```

### CSS global

Archivo:

```text
wwwroot/css/app.css
```

Responsabilidades actuales:

- Tipografia global.
- Estilos del sidebar/nav.
- Estilos de loading.
- Blazor error UI.
- Ajustes de accesibilidad/foco.
- Ajustes de `MudDatePicker`.
- Semaforo visual para fechas del `KpgDatePicker`.

Observacion:

El CSS global contiene overrides de MudBlazor y estilos de componentes especificos. Esta bien para fixes globales, pero si crece, conviene mover estilos especificos a archivos `.razor.css` junto al componente correspondiente.

## Flujos funcionales principales

### Login

```text
/login
  -> LoginPage
  -> KpgAuthStateProvider
  -> AuthRepository
  -> api/auth/login
  -> AuthStateService + sessionStorage refresh token
  -> NavigateTo(returnUrl o /)
```

### Registro de horas

```text
/registro
  -> RegistroPage
  -> KpgShiftForm
  -> carga catalogos y parametros
  -> RegistroHorasRepository.CreateAsync
  -> api/registros-horas
```

Dependencias de UI:

- `IParametroSistemaRepository`
- `IEmpleadoRepository`
- `IClienteRepository`
- `IModalidadRepository`
- `ILugarTrabajoRepository`
- `ISolicitudExcepcionRepository`
- `IRegistroHorasRepository`

### Historial

```text
/mis-registros
  -> HistorialPage
  -> RegistroHorasRepository.GetHistorialAsync
  -> api/registros-horas
```

### Admin

```text
/admin/usuarios
/admin/empleados
/admin/clientes
/admin/parametros
/admin/bitacora
/solicitudes-excepcion
```

Las paginas admin usan repositorios dedicados:

- `UserAdminRepository`
- `EmpleadoRepository`
- `ClienteRepository`
- `ModalidadRepository`
- `LugarTrabajoRepository`
- `ParametroSistemaRepository`
- `SolicitudExcepcionAdminRepository`
- `BitacoraAdminRepository`

### Reportes

```text
/reportes
  -> ReportesPage
  -> ReportesRepository
  -> api/reportes/horas
  -> api/reportes/horas/excel
  -> api/reportes/horas/pdf
  -> api/reportes/timesheet/excel
```

### Bitacora

Admin:

```text
/admin/bitacora
  -> BitacoraAdminPage
  -> BitacoraAdminRepository
  -> api/bitacora
  -> api/bitacora/export/excel
```

Supervisor/Gerente:

```text
/bitacora
  -> BitacoraPage
  -> BitacoraRepository
  -> api/bitacora/mi-alcance
```

Observacion:

`BitacoraAdminPage` y `BitacoraPage` son estructuralmente muy parecidas. Podrian compartir un componente `KpgBitacoraGrid` y un componente de filtros.

## Puntos donde podria haber cambio arquitectonico

### 1. Repositories y modelos estan demasiado planos

Estado actual:

```text
Infrastructure/Repositories/
  AuthRepository.cs
  RegistroHorasRepository.cs
  ReportesRepository.cs
  ...
  Models/
    AuthModels.cs
    RegistroHorasModels.cs
    ReportesModels.cs
    ...
```

Problema:

- Todos los modulos comparten una sola carpeta.
- Al crecer, buscar modelos/repositorios requiere navegar una lista larga.
- La carpeta `Infrastructure` concentra tanto transporte HTTP como DTOs de features.

Opciones:

#### Opcion A: Infraestructura por modulo

```text
Infrastructure/ApiClients/
  Auth/
    IAuthRepository.cs
    AuthRepository.cs
    AuthModels.cs
  RegistroHoras/
    IRegistroHorasRepository.cs
    RegistroHorasRepository.cs
    RegistroHorasModels.cs
```

#### Opcion B: Todo junto por feature

```text
Features/Registro/
  Pages/
  Components/
  Services/
    IRegistroHorasRepository.cs
    RegistroHorasRepository.cs
  Models/
    RegistroHorasModels.cs
```

Recomendacion:

Para este frontend, elegiria la Opcion B. En Blazor, poner UI, modelos cliente y cliente HTTP cerca de la feature mejora mucho la navegabilidad. `Shared` queda solo para lo verdaderamente transversal.

### 2. Repeticion de Bearer token en repositorios

Estado actual:

Cada repositorio arma `HttpRequestMessage` y agrega:

```csharp
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
```

Problema:

- Codigo repetido.
- Manejo inconsistente de `UnauthorizedAccessException`, `null`, `false` o listas vacias.

Opciones:

- Crear `AuthorizedHttpClient`.
- Crear helper `ApiRequestFactory`.
- Crear extension method `SendAuthorizedAsync`.

Recomendacion:

Crear un helper simple en `Shared/Services` o `Infrastructure/Http`:

```text
AuthorizedApiClient
  GetAsync<T>
  PostAsync<TRequest,TResponse>
  SendForFileAsync
```

No hace falta sobre-ingenieria; solo centralizar token, errores comunes y descargas.

### 3. Paginas con demasiada logica UI

Ejemplos:

- `KpgShiftForm.razor`
- `ReportesPage.razor`
- `BitacoraAdminPage.razor`

Problema:

- Mezclan render, estado, carga de datos, validaciones, transformaciones y manejo de errores.
- Se vuelven dificiles de probar o modificar.

Recomendacion:

Extraer gradualmente:

```text
Features/Registro/
  Services/RegistroFormState.cs
  Models/ShiftFormModel.cs

Features/Reportes/
  Models/ReportesFiltroModel.cs

Features/Bitacora/
  Components/KpgBitacoraFilters.razor
  Components/KpgBitacoraGrid.razor
```

### 4. Duplicacion en Bitacora Admin vs Bitacora Supervisor/Gerente

Estado actual:

- `Features/Admin/Pages/BitacoraAdminPage.razor`
- `Features/Bitacora/Pages/BitacoraPage.razor`

Problema:

- Tabla, filtros, chips, colores y estados son muy parecidos.
- Cambios visuales tendrian que hacerse dos veces.

Recomendacion:

Crear componentes compartidos:

```text
Features/Bitacora/Components/
  KpgBitacoraFilters.razor
  KpgBitacoraGrid.razor
```

Y dejar las paginas como wrappers:

```text
BitacoraAdminPage -> usa repo admin + permite exportar
BitacoraPage      -> usa repo mi-alcance + sin export admin si aplica
```

### 5. `Pages/Counter`, `Pages/Weather` y `sample-data/weather.json`

Estado actual:

Parecen restos de plantilla Blazor.

Recomendacion:

Eliminar si no estan referenciados. Esto reduce ruido y evita que el proyecto parezca tener features que no existen.

### 6. Tema y CSS global

Estado actual:

- Tema KPG en `App.razor`.
- CSS global contiene estilos de shell, loading, MudDatePicker y calendario.

Recomendacion:

- Mover tema a `Shared/Theming/KpgTheme.cs`.
- Mantener en `app.css` solo estilos globales reales.
- Mover estilos especificos de componente a `.razor.css` cuando sea posible.

### 7. Nombre de carpeta raiz `Fronted`

Estado actual:

La carpeta se llama `Fronted`, no `Frontend`.

Observacion:

No afecta compilacion, pero es un typo visible en paths, docs y scripts.

Recomendacion:

Si no hay scripts externos o pipelines dependiendo del nombre, considerar renombrar a `Frontend`. Si ya hay referencias en docs/scripts, hacerlo como cambio controlado aparte.

## Propuesta de orden objetivo

Opcion recomendada para este proyecto:

```text
Fronted/src/WebUI/
  App.razor
  Program.cs
  _Imports.razor

  Layout/
    MainLayout.razor
    NavMenu.razor

  Features/
    Auth/
      Layout/
      Pages/
      Services/
      Models/

    Registro/
      Pages/
      Components/
      Services/
      Models/

    Admin/
      Pages/
      Components/
      Services/
      Models/

    Dashboard/
      Pages/
      Components/
      Services/
      Models/

    Reportes/
      Pages/
      Services/
      Models/

    Bitacora/
      Pages/
      Components/
      Services/
      Models/

    Notificaciones/
      Pages/
      Services/
      Models/

  Shared/
    Components/
    Constants/
    Services/
    Theming/
    Utils/

  Infrastructure/
    Http/
      AuthorizedApiClient.cs
```

En esta propuesta:

- Las paginas y componentes quedan con sus modelos/repositorios cerca.
- `Infrastructure` deja de ser una bodega de features y se queda con infraestructura HTTP transversal.
- `Shared` mantiene piezas comunes reales.

## Reglas arquitectonicas recomendadas

1. Cada feature debe contener sus paginas, componentes especificos, modelos y servicios/repositories propios.
2. `Shared` solo debe contener elementos reutilizados por dos o mas features.
3. `Infrastructure` debe contener mecanismos tecnicos transversales, no modelos de negocio de cada pantalla.
4. Las paginas `.razor` deben coordinar UI, pero no concentrar transformaciones grandes ni construccion repetida de requests.
5. Los repositories no deberian repetir manualmente la configuracion del bearer token.
6. Las rutas deben protegerse con `[Authorize]`, no solo ocultarse en `NavMenu`.
7. `AuthorizeView` en menu es UX; la seguridad real debe estar en pagina y backend.
8. Los textos visibles deben mantenerse en Espanol consistente.
9. Los componentes compartidos deben tener nombres `Kpg*` si son parte del design system local.
10. Los restos de plantilla deben eliminarse cuando ya no cumplen una funcion.

## Cambios de bajo riesgo sugeridos

1. Eliminar `Counter.razor`, `Weather.razor` y `wwwroot/sample-data/weather.json` si no se usan.
2. Crear `Infrastructure/Http/AuthorizedApiClient.cs`.
3. Refactorizar uno o dos repositorios primero para validar el patron.
4. Extraer componentes comunes de bitacora:
   - `KpgBitacoraGrid`
   - `KpgBitacoraFilters`
5. Mover modelos y repositorios de una feature piloto, por ejemplo `Bitacora`, a:

```text
Features/Bitacora/Services/
Features/Bitacora/Models/
```

6. Si el piloto queda limpio, repetir en `Reportes`, `Registro` y `Admin`.

## Cambios que no recomiendo hacer todavia

- No dividir el frontend en varios proyectos sin una razon clara.
- No reemplazar MudBlazor; ya hay una inversion fuerte y consistente.
- No convertir todo a una arquitectura MVVM pesada.
- No mover todo de golpe; mejor hacerlo por feature y con build despues de cada tramo.
- No renombrar `Fronted` a `Frontend` en medio de otro refactor grande.

## Decision pendiente

Antes de reorganizar carpetas, conviene decidir:

### Opcion A: Mantener `Infrastructure/Repositories`, pero ordenarlo por modulo

```text
Infrastructure/Repositories/
  Registro/
  Admin/
  Reportes/
  Bitacora/
```

Menor cambio. Mantiene la idea actual de "repositories en infraestructura".

### Opcion B: Mover repositories/modelos a cada feature

```text
Features/Registro/Services
Features/Registro/Models
Features/Reportes/Services
Features/Reportes/Models
```

Mayor claridad para Blazor feature-based. Menos saltos entre carpetas.

### Recomendacion

Elegiria la Opcion B de forma gradual. En frontend, la cohesion por feature suele pesar mas que mantener una capa `Infrastructure` grande. Dejaria `Infrastructure` solo para HTTP transversal y detalles tecnicos comunes.
