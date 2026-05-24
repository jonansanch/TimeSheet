# Manual Técnico y de Operaciones — KPG Timesheet

**Versión:** 1.2  
**Fecha:** 2026-05-23  
**Audiencia:** Responsable técnico / DevOps  

---

## Tabla de Contenidos

1. [Visión General del Sistema](#1-visión-general-del-sistema)
2. [Arquitectura](#2-arquitectura)
3. [Requisitos de Infraestructura](#3-requisitos-de-infraestructura)
4. [Estructura del Repositorio](#4-estructura-del-repositorio)
5. [Configuración de Entornos](#5-configuración-de-entornos)
6. [Base de Datos](#6-base-de-datos)
7. [Compilación y Publicación](#7-compilación-y-publicación)
8. [Deploy en IIS (Windows Server)](#8-deploy-en-iis-windows-server)
9. [Backups y Recuperación](#9-backups-y-recuperación)
10. [Logs y Monitoreo](#10-logs-y-monitoreo)
11. [Solución de Problemas Comunes](#11-solución-de-problemas-comunes)
12. [Dependencias y Paquetes](#12-dependencias-y-paquetes)

---

## 1. Visión General del Sistema

**KPG Timesheet** es una aplicación web de registro de horas para 30 colaboradores de KPG. Reemplaza el control manual en hojas de Excel.

### Componentes

| Componente | Tecnología | Puerto dev |
|------------|-----------|------------|
| **Frontend** | Blazor WebAssembly (.NET 10) | https://localhost:5201 |
| **Backend API** | ASP.NET Core Minimal API (.NET 10) | https://localhost:7035 |
| **Base de datos** | SQL Server 2019+ | 1433 |

### Flujo general

```
Browser (Blazor WASM)
    │  HTTPS
    ▼
ASP.NET Core API  ──── JWT Auth ────▶ Claims / Roles
    │
    ├── EF Core (escritura) ──────────▶ SQL Server
    └── Dapper (lectura/dashboards) ──▶ SQL Server
```

El frontend Blazor WASM se descarga en el browser del usuario y hace llamadas REST a la API. No hay server-side rendering — toda la lógica de presentación corre en el cliente.

---

## 2. Arquitectura

El backend sigue **Clean Architecture** con 4 capas:

```
Backend/src/
├── Domain/          → Entidades, reglas de negocio, constantes
├── Application/     → CQRS (Commands/Queries), validadores, interfaces
├── Infrastructure/  → EF Core, Dapper, Identity, repositorios
└── Api/             → Endpoints, middleware, configuración del servidor
```

### Patrones aplicados

| Patrón | Implementación |
|--------|---------------|
| **CQRS** | MediatR — Commands para escritura, Queries para lectura |
| **Repository** | Frontend: repositorios HTTP; Backend: IApplicationDbContext |
| **Problem Details** | Todos los errores HTTP siguen RFC 7807 |
| **Soft delete** | Catálogos (Modalidad, LugarTrabajo, Empleado, Cliente) usan `IsActive` |
| **Hard delete** | Registros de horas — eliminación física |
| **JWT + Refresh Token** | Acceso 60 min, refresh 8 horas, rotación single-use |
| **Output Cache** | Endpoints de dashboard cacheados 60 s (`CacheOutput("dashboard")`, varía por query string) |
| **Server-side pagination** | Historial y gestión de usuarios usan paginación en SQL; frontend usa `MudDataGrid ServerData` |

### Optimizaciones de rendimiento implementadas

#### Capa de base de datos

| Optimización | Archivo | Detalle |
|---|---|---|
| Índices adicionales | `ApplicationDbContextInitialiser.cs` | `IX_RegistrosHoras_FechaRegistro` (INCLUDE UserId) y `IX_AspNetUsers_IsActive` — cubren queries de dashboard sin full scan |
| Paginación SQL server-side | `RegistroHorasRepository` / `IdentityService` | `ROW_NUMBER() OVER (ORDER BY …)` + `WHERE RowNum > @Skip AND RowNum <= @Skip + @Take` — portable entre SQL Server y SQLite |
| `QueryMultipleAsync` | `IdentityService.GetUsersAsync`, `DashboardRepository` | `COUNT(*) + SELECT` paginado en un solo round-trip Dapper via `GridReader` |
| LEFT JOIN acotado | `DashboardRepository.SqlPendientes` | `AND r.FechaRegistro >= @LimiteBusqueda` (hoy − 90 días) evita full scan del historial de `RegistrosHoras` al calcular pendientes críticos |
| CTE unificada | `DashboardRepository.SqlMetricas` | Tres CTEs reemplazan cinco subqueries escalares en la query de métricas globales del Admin |
| JOIN único en catálogos | `GetCatalogoClientesConProyectosQueryHandler` | LINQ LEFT JOIN reemplaza dos queries + join en C# para la carga de clientes con sus proyectos |
| Filtro de ventana | `GetSolicitudesExcepcionQueryHandler` | `WHERE Estado = Pendiente OR FechaRegistro >= hoy − 180 días` — previene carga ilimitada del historial de solicitudes |

#### Capa de API

- **Output Cache 60 s**: todos los endpoints GET de `/api/dashboard` llevan `.CacheOutput("dashboard")`. La política varía por query string para respetar parámetros de fecha/filtro. Se configura en `DependencyInjection.cs` (`AddOutputCache`) y se activa con `UseOutputCache()` en `Program.cs`.

#### Capa de frontend

- **`Task.WhenAll`**: `RegistroPage.OnInitializedAsync` lanza en paralelo la carga de la ventana de retroactividad y el calendario mensual, eliminando la espera secuencial.
- **`MudDataGrid ServerData`**: el historial personal usa `ServerData="CargarDatosAsync"` en lugar de `Items`. La función devuelve `GridData<T>` con `TotalItems` para que el pager refleje el total real. `state.Page` es 0-indexed → se pasa `state.Page + 1` a la API. Para forzar recarga tras operaciones se usa `await _grid!.ReloadServerData()`.

### Estructura del frontend

```
Fronted/src/WebUI/
├── Features/          → Páginas y componentes por épica
│   ├── Admin/         → Gestión de usuarios, catálogos, parámetros
│   ├── Dashboard/     → Dashboard supervisor/gerente/admin
│   ├── Registro/      → Formulario de registro e historial (incluye vista calendario)
│   └── Reportes/      → Reportes con filtros y exportación
├── Infrastructure/
│   └── Repositories/  → Clientes HTTP hacia la API
├── Layout/            → NavMenu, MainLayout (gestiona aviso de sesión)
├── Resources/         → Archivos de localización
│   ├── SharedResource.resx     → Cadenas en español (idioma base)
│   └── SharedResource.en.resx  → Traducciones al inglés
├── Shared/
│   ├── Components/    → KpgStatCard, KpgTableSkeleton, KpgLanguageSelector,
│   │                    KpgSessionWarningDialog (modal countdown de sesión)
│   └── Services/      → KpgAuthStateProvider, AuthStateService, CurrentUserService,
│                        CatalogosCacheService, SessionTimeoutService
└── wwwroot/           → CSS, favicon, index.html
```

#### SessionTimeoutService

`Shared/Services/SessionTimeoutService.cs` — singleton que gestiona el ciclo de vida del aviso de expiración JWT:

- `StartTracking(expiresAtUtc, warningMinutes)`: lanza dos `Task.Delay` encadenados — el primero dispara el evento `OnWarning` (N minutos antes de expirar), el segundo dispara `OnExpired`.
- `StopTracking()`: cancela los delays pendientes (se llama en logout y al renovar la sesión).
- `MainLayout` suscribe ambos eventos y usa `InvokeAsync` para marshaling al hilo del renderer.
- Al dispararse `OnWarning`, `MainLayout` muestra `KpgSessionWarningDialog` con cuenta regresiva MM:SS via `PeriodicTimer`.
- "Continuar" en el dialog llama `KpgAuthStateProvider.RefreshSessionAsync()` → reinicia el tracking con la nueva expiración.

### Localización (i18n)

El sistema soporta **español** e **inglés** mediante `Microsoft.Extensions.Localization` con archivos `.resx`.

#### Archivos de recursos

| Archivo | Idioma |
|---------|--------|
| `Resources/SharedResource.resx` | Español (idioma base / fallback) |
| `Resources/SharedResource.en.resx` | Inglés |

Las claves siguen la convención `Seccion_NombreDescriptivo` (p. ej. `Reg_SaveShift`, `Btn_Cancel`, `Label_Client`).

#### Inicialización de cultura en Program.cs

```csharp
var app = builder.Build();

// Leer cultura guardada en localStorage antes de renderizar
var js = app.Services.GetRequiredService<IJSRuntime>();
var culture = await js.InvokeAsync<string?>("globalThis.blazorCulture.get");
if (!string.IsNullOrEmpty(culture))
{
    var cultureInfo = new CultureInfo(culture);
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
}

await app.RunAsync();
```

#### Helper JS (wwwroot/index.html)

```js
window.blazorCulture = {
    get: () => localStorage["blazor-culture"],
    set: (value) => localStorage["blazor-culture"] = value
};
```

#### Componente KpgLanguageSelector

Ubicado en `Shared/Components/KpgLanguageSelector.razor`. Muestra el selector ES/EN en la AppBar. Al cambiar el idioma:

1. Llama a `blazorCulture.set(culture)` para persistir en `localStorage`.
2. Hace `NavigationManager.NavigateTo(currentUri, forceLoad: true)` para recargar la app con la nueva cultura.

---

## 3. Requisitos de Infraestructura

### Servidor de aplicación

| Requisito | Mínimo | Recomendado |
|-----------|--------|-------------|
| Sistema operativo | Windows Server 2019 | Windows Server 2022 |
| RAM | 4 GB | 8 GB |
| CPU | 2 vCores | 4 vCores |
| Disco | 20 GB libres | 50 GB libres |
| .NET Runtime | .NET 10 (ASP.NET Core Hosting Bundle) | .NET 10 |
| IIS | 10.0 | 10.0 |
| IIS módulo | ASP.NET Core Module v2 (incluido en Hosting Bundle) | — |

### Servidor de base de datos

| Requisito | Versión |
|-----------|---------|
| SQL Server | 2019 o superior |
| Collation | `SQL_Latin1_General_CP1_CI_AS` (o la collation del servidor) |
| Autenticación | SQL Server Authentication o Windows Authentication |

### Certificado SSL

Requerido para HTTPS. Opciones:
- Certificado emitido por la CA corporativa de KPG.
- Let's Encrypt (si el servidor tiene salida a internet).
- Certificado autofirmado (solo para desarrollo/pruebas internas).

### SMTP (para notificaciones)

El sistema envía correos de recordatorio a colaboradores con registros pendientes. Requisitos:
- Servidor SMTP accesible desde el servidor de aplicación.
- Puerto 587 (STARTTLS) o 465 (SSL).
- Credenciales de cuenta de envío.

Si no hay servidor SMTP disponible en producción, la funcionalidad de notificaciones puede desactivarse dejando `SmtpHost` vacío en la configuración.

---

## 4. Estructura del Repositorio

```
/
├── Backend/
│   ├── KPG.Timesheet.sln
│   ├── src/
│   │   ├── Api/                    → Proyecto API (startup, endpoints, middleware)
│   │   ├── Application/            → CQRS, validadores, interfaces
│   │   ├── Domain/                 → Entidades, constantes, excepciones
│   │   └── Infrastructure/         → EF Core, Identity, Dapper, repositorios
│   └── tests/
│       ├── Domain.UnitTests/
│       ├── Application.UnitTests/
│       └── Infrastructure.IntegrationTests/
├── Fronted/
│   ├── KPG.Timesheet.WebUI.sln
│   └── src/WebUI/                  → Proyecto Blazor WASM
├── Docs/
│   ├── stories/                    → Historias de usuario (completadas)
│   ├── operations/                 → Documentos operacionales (go-live, QA, UAT)
│   └── manuals/                    → Este manual y los demás
└── _bmad-output/
    └── planning-artifacts/         → PRD, arquitectura, épicas (referencia)
```

---

## 5. Configuración de Entornos

### appsettings.json (base)

Ubicación: `Backend/src/Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=KPG_Timesheet;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "CAMBIAR_POR_CLAVE_SEGURA_MIN_32_CHARS",
    "Issuer": "KPG.Timesheet.Api",
    "Audience": "KPG.Timesheet.WebUI",
    "ExpirationMinutes": 60,
    "RefreshExpirationHours": 8,
    "SessionWarningMinutes": 5
  },
  "SmtpSettings": {
    "Host": "",
    "Port": 587,
    "UseSsl": true,
    "Username": "",
    "Password": "",
    "FromEmail": "timesheet@kpg.com",
    "FromName": "KPG Timesheet"
  },
  "FrontendBaseUrl": "https://localhost:5201",
  "Cors": {
    "AllowedOrigins": ["https://localhost:5201"]
  }
}
```

### appsettings.Production.json

Crear este archivo en el servidor de producción (NO commitar al repositorio):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SQL_SERVER_HOST;Database=KPG_Timesheet_Prod;User Id=kpg_app;Password=PASSWORD_SEGURA;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "CLAVE_PRODUCCION_MIN_32_CHARS_MUY_SEGURA"
  },
  "SmtpSettings": {
    "Host": "smtp.kpg.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "timesheet@kpg.com",
    "Password": "PASSWORD_SMTP"
  },
  "FrontendBaseUrl": "https://timesheet.kpg.com",
  "Cors": {
    "AllowedOrigins": ["https://timesheet.kpg.com"]
  }
}
```

> **Seguridad crítica:** La `Jwt.Key` debe ser una cadena aleatoria de al menos 32 caracteres. Usar un generador de claves seguro. Nunca la misma clave en dev y producción.

### Variables de entorno (alternativa a appsettings)

Los valores de `appsettings.Production.json` pueden sobreescribirse con variables de entorno del sistema operativo, usando la notación con doble guión bajo:

```
ConnectionStrings__DefaultConnection=Server=...
Jwt__Key=clave_secreta
```

---

## 6. Base de Datos

### Inicialización

El sistema usa `EnsureCreatedAsync()` en lugar de migraciones EF Core tradicionales. Al iniciar la API por primera vez, automáticamente:
1. Crea la base de datos si no existe.
2. Crea todas las tablas.
3. Ejecuta el seeder con datos iniciales (catálogos base y usuarios de prueba).

El archivo de inicialización es: `Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`

### Tablas principales

| Tabla | Descripción |
|-------|-------------|
| `AspNetUsers` | Usuarios del sistema (ASP.NET Core Identity) |
| `AspNetRoles` | Roles: Admin, Gerente, Supervisor, Empleado |
| `AspNetUserRoles` | Asignación usuario-rol |
| `RefreshTokens` | Tokens de refresh (rotación single-use) |
| `RegistrosHoras` | Registros de jornada de cada colaborador |
| `SolicitudesExcepcion` | Solicitudes de registro fuera de ventana |
| `Empleados` | Catálogo de recursos/empleados |
| `Clientes` | Catálogo de clientes |
| `Proyectos` | Catálogo de proyectos por cliente |
| `Modalidades` | Catálogo de modalidades de trabajo |
| `LugaresTrabajo` | Catálogo de lugares de trabajo |
| `ParametrosSistema` | Configuración: ventana retroactividad, umbral notificaciones |
| `BitacoraAuditoria` | Registro de eventos sensibles |
| `NotificacionesEnviadas` | Historial de correos de recordatorio |

### Esquema de RegistrosHoras

```sql
CREATE TABLE RegistrosHoras (
    Id              INT IDENTITY PRIMARY KEY,
    UserId          NVARCHAR(450) NOT NULL,
    FechaRegistro   DATE NOT NULL,
    HoraEntradaAM   TIME NULL,
    HoraSalidaAM    TIME NULL,
    HoraEntradaPM   TIME NULL,
    HoraSalidaPM    TIME NULL,
    Cliente         NVARCHAR(200) NOT NULL,
    Proyecto        NVARCHAR(200) NOT NULL,
    Modalidad       NVARCHAR(100) NOT NULL,
    Recurso         NVARCHAR(100) NOT NULL,
    Descripcion     NVARCHAR(1000) NOT NULL,
    Lugar           NVARCHAR(200) NOT NULL,
    CONSTRAINT UQ_RegistrosHoras_User_Fecha UNIQUE (UserId, FechaRegistro)
);
```

> El índice único `(UserId, FechaRegistro)` garantiza un solo registro por usuario por día. El endpoint de creación hace **upsert** automáticamente.

### Dapper TypeHandlers

Dapper no conoce por defecto los tipos `DateOnly` y `DateTimeOffset`. Se registran TypeHandlers globales en `Infrastructure/DependencyInjection.cs` para manejar la conversión automática:

| Handler | Convierte | Motivo |
|---------|-----------|--------|
| `DateOnlyTypeHandler` | `DateTime` ↔ `DateOnly` | `DATE` en SQL Server → `DateOnly` en .NET |
| `NullableDateOnlyTypeHandler` | `DateTime?` ↔ `DateOnly?` | Versión nullable del anterior |
| `DateTimeOffsetTypeHandler` | `string` / `DateTimeOffset` → `DateTimeOffset` | SQLite devuelve `TEXT` para `datetimeoffset`; SQL Server devuelve el tipo nativo (pass-through) |
| `NullableDateTimeOffsetTypeHandler` | ídem nullable | Versión nullable del anterior |

Los handlers para `DateTimeOffset` son esenciales para la compatibilidad de los integration tests (SQLite in-memory) con las queries de producción (SQL Server). Sin ellos, Dapper falla al intentar mapear `TEXT` de SQLite a `DateTimeOffset` en los DTOs.

Los handlers se registran en `SqlMapper.AddTypeHandler(...)` una sola vez al arranque; son globales a todo el proceso.

### Usuarios de prueba (seed)

| Email | Contraseña | Rol |
|-------|-----------|-----|
| admin@kpg.com | Admin1234! | Admin |
| supervisor@kpg.com | Supervisor1234! | Supervisor |
| empleado@kpg.com | Empleado1234! | Empleado |
| ana.garcia@kpg.com | Empleado1234! | Empleado |
| carlos.ruiz@kpg.com | Empleado1234! | Empleado |

> Cambiar todas las contraseñas de prueba antes de go-live en producción.

---

## 7. Compilación y Publicación

### Requisitos previos

- .NET 10 SDK instalado en la máquina de build.
- Acceso al repositorio.

### Compilar y ejecutar tests

```bash
# Backend
dotnet build Backend/KPG.Timesheet.sln
dotnet test Backend/KPG.Timesheet.sln

# Frontend
dotnet build Fronted/KPG.Timesheet.WebUI.sln
```

Un build limpio debe mostrar:
- 0 errores, 0 advertencias en Backend y Frontend.
- 231+ tests pasando (68 Domain unit + 24 Application unit + 139 Integration).

### Publicar el Backend

```bash
dotnet publish Backend/src/Api/KPG.Timesheet.Api.csproj \
  -c Release \
  -o ./publish/api \
  --no-self-contained \
  -r win-x64
```

Esto genera en `./publish/api/`:
- `KPG.Timesheet.Api.exe` — ejecutable de la API
- `wwwroot/` — archivos estáticos si aplica
- Todas las DLLs de dependencias

### Publicar el Frontend

```bash
dotnet publish Fronted/src/WebUI/KPG.Timesheet.WebUI.csproj \
  -c Release \
  -o ./publish/webui
```

Los archivos del frontend estarán en `./publish/webui/wwwroot/`. Estos se sirven como archivos estáticos desde IIS.

---

## 8. Deploy en IIS (Windows Server)

### Prerequisitos en el servidor

1. Instalar **.NET 10 ASP.NET Core Hosting Bundle** desde: https://dotnet.microsoft.com/download/dotnet/10.0
2. Reiniciar IIS: `iisreset`
3. Verificar que el módulo ASP.NET Core esté activo en IIS Manager.

### Estructura de carpetas en el servidor

```
D:\Websites\
├── KPG.Timesheet.Api\        → Archivos publicados del Backend
├── KPG.Timesheet.WebUI\      → Archivos wwwroot del Frontend
└── Backups\                  → Backups de la base de datos
```

### Configurar el sitio de la API en IIS

1. Abrir **IIS Manager**.
2. En **Sites**, hacer clic derecho → **Add Website**.
3. Configurar:
   - **Site name**: `KPG.Timesheet.Api`
   - **Physical path**: `D:\Websites\KPG.Timesheet.Api`
   - **Binding**: HTTPS, puerto 443, hostname `api.timesheet.kpg.com`
   - **SSL certificate**: seleccionar el certificado instalado
4. Crear un **Application Pool**:
   - **Name**: `KPG.Timesheet.Api`
   - **.NET CLR version**: `No Managed Code`
   - **Managed pipeline**: `Integrated`

5. Crear el archivo `web.config` en la carpeta de la API (si `dotnet publish` no lo generó):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*"
             modules="AspNetCoreModuleV2"
             resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\KPG.Timesheet.Api.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

6. Crear la carpeta `logs\` dentro de `D:\Websites\KPG.Timesheet.Api\`.
7. Dar permisos de escritura al Application Pool identity sobre esa carpeta.

### Configurar el sitio del Frontend en IIS

El frontend Blazor WASM son archivos estáticos con routing del lado del cliente.

1. Crear un nuevo sitio en IIS:
   - **Site name**: `KPG.Timesheet.WebUI`
   - **Physical path**: `D:\Websites\KPG.Timesheet.WebUI`
   - **Binding**: HTTPS, puerto 443, hostname `timesheet.kpg.com`

2. Agregar `web.config` para manejar el routing de Blazor:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Blazor Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <mimeMap fileExtension=".wasm" mimeType="application/wasm" />
      <mimeMap fileExtension=".blat" mimeType="application/octet-stream" />
      <mimeMap fileExtension=".dat" mimeType="application/octet-stream" />
    </staticContent>
    <httpCompression>
      <dynamicTypes>
        <add mimeType="application/wasm" enabled="true" />
      </dynamicTypes>
    </httpCompression>
  </system.webServer>
</configuration>
```

### Verificar el deploy

1. Navegar a `https://timesheet.kpg.com` — debe cargar el login de Blazor.
2. Navegar a `https://api.timesheet.kpg.com/scalar` — debe mostrar la documentación de la API.
3. Iniciar sesión con `admin@kpg.com` y verificar que la navegación funciona.

---

## 9. Backups y Recuperación

### Backup automático (SQL Server Agent)

Crear un job en **SQL Server Agent** para backup diario:

```sql
-- Job: KPG_Timesheet_DailyBackup
-- Schedule: Every day at 23:00

BACKUP DATABASE [KPG_Timesheet_Prod]
TO DISK = N'D:\Backups\Timesheet\KPG_Timesheet_' 
    + CONVERT(NVARCHAR, GETDATE(), 112) + '.bak'
WITH COMPRESSION, STATS = 10;
```

Configurar retención de 30 días (eliminar backups más antiguos automáticamente).

### Backup manual pre-deploy

Antes de cualquier actualización:

```sql
BACKUP DATABASE [KPG_Timesheet_Prod]
TO DISK = N'D:\Backups\Timesheet\KPG_Timesheet_PreDeploy_20260522.bak'
WITH COMPRESSION;
```

### Procedimiento de recuperación

Ver `Docs/operations/backup-recovery.md` para el procedimiento completo.

Resumen rápido:

```sql
-- 1. Poner la base en modo de usuario único
ALTER DATABASE [KPG_Timesheet_Prod] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

-- 2. Restaurar desde backup
RESTORE DATABASE [KPG_Timesheet_Prod]
FROM DISK = N'D:\Backups\Timesheet\KPG_Timesheet_20260522.bak'
WITH REPLACE;

-- 3. Volver a modo multiusuario
ALTER DATABASE [KPG_Timesheet_Prod] SET MULTI_USER;
```

---

## 10. Logs y Monitoreo

### Logs de la API

La API usa **Serilog** configurado para escribir en:
- **Consola**: durante desarrollo.
- **Archivo**: en producción, en la carpeta `logs/` dentro del directorio de la API.

Formato de archivos de log: `logs/log-YYYYMMDD.txt` (rotación diaria).

### Revisar logs en producción

```powershell
# Ver los últimos 100 errores
Get-Content "D:\Websites\KPG.Timesheet.Api\logs\log-20260522.txt" -Tail 100 | Where-Object { $_ -match "ERR|FATAL" }
```

### Niveles de log

| Nivel | Descripción |
|-------|-------------|
| `INF` | Información normal (requests, inicializaciones) |
| `WRN` | Advertencias (tokens próximos a expirar, intentos fallidos) |
| `ERR` | Errores manejados (validaciones, 4xx) |
| `FATAL` | Errores críticos (base de datos caída, excepciones no capturadas) |

### Monitoreo básico

Verificar diariamente:
1. **Backup nocturno**: confirmar que el archivo `.bak` del día anterior existe en `D:\Backups\Timesheet\`.
2. **Logs de error**: buscar entradas `ERR` o `FATAL` en el log del día.
3. **Disponibilidad**: navegar a `https://timesheet.kpg.com` y verificar que carga.
4. **Notificaciones enviadas**: revisar `/notificaciones` en el sistema para confirmar que los correos se están enviando.

---

## 11. Solución de Problemas Comunes

### El frontend carga en blanco (pantalla vacía)

**Causa probable:** Falta el `web.config` con las reglas de rewrite, o el archivo `index.html` no está en la ruta correcta.  
**Solución:**
1. Verificar que `index.html` existe en `D:\Websites\KPG.Timesheet.WebUI\`.
2. Verificar que el `web.config` con las reglas Blazor está presente.
3. Revisar el módulo URL Rewrite de IIS esté instalado.

### Error 502 Bad Gateway al llamar a la API

**Causa probable:** El proceso de la API no está corriendo o crasheó al iniciar.  
**Solución:**
1. Revisar el archivo `logs/stdout_*.log` en la carpeta de la API para ver el error de inicio.
2. Verificar que la connection string en `appsettings.Production.json` es correcta.
3. Verificar que el .NET Hosting Bundle está instalado: `dotnet --version` en el servidor.
4. Reciclar el Application Pool en IIS Manager.

### No se puede conectar a la base de datos

**Causa probable:** Connection string incorrecta, SQL Server detenido o firewall bloqueando el puerto 1433.  
**Solución:**
1. Verificar que SQL Server está corriendo: `Get-Service MSSQLSERVER`.
2. Probar la connection string con SQL Server Management Studio.
3. Verificar que el firewall de Windows permite el puerto 1433 desde el servidor de aplicación.

### El login devuelve error 401

**Causa probable:** La `Jwt.Key` en producción es diferente a la que generó los tokens existentes, o el JWT expiró.  
**Solución:**
1. Verificar que la `Jwt.Key` en `appsettings.Production.json` no cambió.
2. Pedir al usuario que limpie cookies/caché del navegador e intente de nuevo.

### Los correos de notificación no se envían

**Causa probable:** Configuración SMTP incorrecta o servidor SMTP inaccesible.  
**Solución:**
1. Revisar los logs por errores con tag `[SMTP]` o `[Notificaciones]`.
2. Verificar `SmtpSettings` en `appsettings.Production.json`.
3. Probar conectividad SMTP desde el servidor: `Test-NetConnection smtp.kpg.com -Port 587`.
4. Si no hay SMTP disponible, dejar `SmtpHost` vacío para deshabilitar las notificaciones.

### Error "CORS policy" en el browser

**Causa probable:** La URL del frontend no está en la lista de orígenes permitidos del backend.  
**Solución:**
1. Verificar que `Cors.AllowedOrigins` en `appsettings.Production.json` incluye exactamente la URL del frontend (con https y sin barra final).
2. Ejemplo: `"AllowedOrigins": ["https://timesheet.kpg.com"]`.
3. Reiniciar el Application Pool de la API.

### La base de datos no se inicializa al primer arranque

**Causa probable:** El usuario de SQL Server no tiene permisos para crear bases de datos.  
**Solución:**
1. El usuario de la connection string necesita rol `dbcreator` para el primer arranque.
2. Alternativa: crear la base de datos manualmente antes del primer arranque y otorgar permisos `db_owner` al usuario de la aplicación.

---

## 12. Dependencias y Paquetes

### Backend (.NET 10)

| Paquete | Versión | Uso |
|---------|---------|-----|
| ASP.NET Core | 10.0 | Framework web, minimal API |
| Entity Framework Core | 10.x | ORM para escritura y migraciones |
| Dapper | 2.x | Micro-ORM para consultas de dashboard (lectura rápida) |
| MediatR | 12.x | CQRS — mediador de commands/queries |
| FluentValidation | 11.x | Validación de commands |
| ASP.NET Core Identity | 10.0 | Gestión de usuarios, roles, hashing de contraseñas |
| Ardalis.GuardClauses | 4.x | Guard clauses en domain/application |
| Scalar.AspNetCore | latest | Documentación OpenAPI (reemplaza Swagger UI) |
| MiniExcel | 1.35.0 | Exportación Excel del reporte tabular |
| ClosedXML | 0.x | Exportación Excel con formato KPG (timesheet individual) |
| QuestPDF | 2025.4.0 | Exportación PDF (licencia Community) |
| Serilog.AspNetCore | 8.x | Logging estructurado |
| Microsoft.Data.SqlClient | latest | Driver SQL Server para Dapper |

### Frontend (Blazor WASM .NET 10)

| Paquete | Versión | Uso |
|---------|---------|-----|
| MudBlazor | 8.5.0 | Componentes UI (DataGrid, Dialog, NavMenu, etc.) |
| Microsoft.AspNetCore.Components.WebAssembly | 10.0 | Runtime Blazor WASM |
| Microsoft.Extensions.Localization | 10.0 | Localización ES/EN con archivos .resx |
| System.Net.Http.Json | incluido | Llamadas HTTP con JSON |

### Tests

| Paquete | Uso |
|---------|-----|
| xUnit | Framework de tests |
| FluentAssertions | Aserciones legibles |
| Microsoft.AspNetCore.Mvc.Testing | `WebApplicationFactory<Program>` para tests de endpoints HTTP |
| Microsoft.EntityFrameworkCore.Sqlite | SQLite in-memory para integration tests (reemplaza InMemory — soporta `ExecuteDeleteAsync`) |
| NSubstitute | Mocking en unit tests (sustituye Moq) |
