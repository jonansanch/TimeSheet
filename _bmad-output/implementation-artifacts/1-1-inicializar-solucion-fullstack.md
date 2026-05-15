# Story 1.1: Inicializar Solución Full-Stack KPG Timesheet

Status: review

## Story

As a desarrollador,
I want una solución .NET 10 con Clean Architecture y Blazor WebAssembly inicializada y compilable en `Backend/`,
so that el equipo pueda construir todos los módulos sobre una estructura consistente sin tomar decisiones de scaffolding en historias futuras.

## Acceptance Criteria

1. **Given** el repositorio Timesheet con `Backend/` vacío, **When** se inicializa la solución, **Then** existe `Backend/KPG.Timesheet.sln` con cinco proyectos: `KPG.Timesheet.Domain`, `KPG.Timesheet.Application`, `KPG.Timesheet.Infrastructure`, `KPG.Timesheet.Api` y `KPG.Timesheet.WebUI`, y la solución compila sin errores con `dotnet build Backend/KPG.Timesheet.sln`.

2. **Given** la arquitectura aprobada, **When** se agregan paquetes base, **Then** los siguientes paquetes están disponibles en sus proyectos correspondientes y la solución sigue compilando:
   - **Infrastructure:** Dapper 2.x, MiniExcel (última estable), QuestPDF (última Community)
   - **WebUI:** MudBlazor (última estable), Blazor.ApexCharts (última estable)
   - **Ya incluidos por template:** MediatR, FluentValidation, AutoMapper, EF Core 10.x, xUnit, FluentAssertions, NSubstitute
   - **No se crean** entidades de dominio, migrations, ni handlers de módulos futuros.

3. **Given** existe `Fronted/` con un proyecto Angular, **When** se documenta el estado del frontend, **Then** existe un archivo `Fronted/LEGACY.md` que indica claramente que ese directorio es un prototipo Angular legacy descartado y que Blazor WebAssembly (`Backend/src/WebUI`) es el frontend oficial de V1.

4. **Given** la solución inicializada, **When** se ejecuta `dotnet test Backend/KPG.Timesheet.sln`, **Then** los proyectos de prueba existen y corren sin errores (pueden estar vacíos o con el test de smoke del template).

## Tasks / Subtasks

- [x] **T1 — Preparar entorno y verificar prerequisitos** (AC: 1)
  - [x] Verificar `dotnet --version` retorna .NET 10.x; si no, instalar SDK .NET 10
  - [x] Verificar que `Backend/` existe y está vacío (crearlo si no existe)
  - [x] Instalar template: `dotnet new install CleanArchitecture.Template` (última versión compatible con .NET 10)

- [x] **T2 — Inicializar solución Clean Architecture** (AC: 1)
  - [x] Desde `Backend/`, ejecutar: `dotnet new ca-sln` (genera solución con proyectos Domain, Application, Infrastructure, Api + tests)
  - [x] Verificar que los proyectos generados se nombran con prefijo `KPG.Timesheet.*`; si el template usa otro prefijo, renombrar manualmente los `.csproj` y referencias
  - [x] Confirmar que el namespace raíz de cada proyecto es `KPG.Timesheet.[Capa]`
  - [x] Confirmar que la estructura de carpetas es `src/Domain/`, `src/Application/`, `src/Infrastructure/`, `src/Api/` y `tests/`

- [x] **T3 — Agregar proyecto Blazor WebAssembly** (AC: 1)
  - [x] Desde `Backend/`, ejecutar: `dotnet new blazorwasm -o src/WebUI -n KPG.Timesheet.WebUI`
  - [x] Agregar a la solución: `dotnet sln add src/WebUI/KPG.Timesheet.WebUI.csproj`
  - [x] Agregar referencia de `WebUI` a `Application` en el proyecto WebUI: `dotnet add src/WebUI reference src/Application/KPG.Timesheet.Application.csproj` (para acceder a DTOs)

- [x] **T4 — Limpiar código de muestra del template** (AC: 1, 2)
  - [x] Eliminar la feature `TodoItems` que el template genera como ejemplo: carpetas `Features/TodoItems` en Application, entidad `TodoItem` en Domain, migration de ejemplo en Infrastructure, controller `TodoItemsController` en Api
  - [x] Eliminar registros DI relacionados a la feature de ejemplo
  - [x] Verificar que la solución sigue compilando después de la limpieza

- [x] **T5 — Agregar paquetes NuGet adicionales** (AC: 2)
  - [x] `dotnet add src/Infrastructure package Dapper` (versión 2.x)
  - [x] `dotnet add src/Infrastructure package MiniExcel`
  - [x] `dotnet add src/Infrastructure package QuestPDF`
  - [x] `dotnet add src/WebUI package MudBlazor`
  - [x] `dotnet add src/WebUI package Blazor.ApexCharts`
  - [x] Centralizar versiones en `Directory.Packages.props` en la raíz de `Backend/`

- [x] **T6 — Configurar archivos base del repositorio** (AC: 1)
  - [x] Crear `Backend/global.json` fijando .NET 10 (`"sdk": { "version": "10.x.xxx", "rollForward": "latestMinor" }`)
  - [x] Crear/verificar `Backend/Directory.Build.props` con propiedades comunes (Nullable enable, ImplicitUsings enable, LangVersion latest)
  - [x] Verificar que existe `Backend/.editorconfig` con configuración C# estándar (si no existe, crearlo con reglas mínimas)
  - [x] Crear `Backend/README.md` explicando la estructura: Clean Architecture, 5 proyectos, comandos de build/test/run

- [x] **T7 — Estructura de carpetas vacías en WebUI** (AC: 1)
  - [x] Crear estructura de carpetas vacías en `src/WebUI/` según arquitectura:
    - `Features/Registro/`, `Features/Dashboard/`, `Features/Reportes/`, `Features/Admin/`
    - `Shared/Components/`, `Shared/Layout/`, `Shared/Services/`
    - `Infrastructure/Repositories/`
  - [x] Agregar `.gitkeep` en cada carpeta vacía para preservarlas en git

- [x] **T8 — Documentar Angular como legacy** (AC: 3)
  - [x] Crear `Fronted/LEGACY.md` con el contenido: indicar que este directorio es un prototipo Angular descartado, que **no** se usa en V1, y que el frontend oficial es `Backend/src/WebUI` (Blazor WebAssembly)

- [x] **T9 — Verificación final** (AC: 1, 2, 4)
  - [x] `dotnet build Backend/KPG.Timesheet.sln` — debe compilar sin errores ni warnings de error
  - [x] `dotnet test Backend/KPG.Timesheet.sln` — debe ejecutarse sin fallos
  - [x] Verificar que los 5 proyectos aparecen en la solución: `dotnet sln list`

## Dev Notes

### Contexto crítico — por qué esta historia importa

Esta es la historia fundacional. Todas las historias siguientes (1.2 en adelante) asumen que la estructura, los nombres de proyecto y los paquetes base ya existen y son exactamente los especificados aquí. Desviarse de la estructura en esta historia causa errores en cascada en historias futuras.

### Herramienta de scaffolding

El template oficial es **Jason Taylor CleanArchitecture** (`CleanArchitecture.Template` en NuGet). El comando `dotnet new ca-sln` genera la estructura Clean Architecture con los proyectos `Domain`, `Application`, `Infrastructure`, `Api` y los proyectos de prueba correspondientes.

**Advertencia de versión:** Verificar que la versión instalada del template sea compatible con .NET 10. Si `dotnet new ca-sln` genera un `global.json` con .NET 8 o 9, actualizar a .NET 10 en `global.json` y en los `TargetFramework` de los `.csproj`.

### Estructura esperada en Backend/

```
Backend/
├── KPG.Timesheet.sln
├── Directory.Build.props          ← Nullable, ImplicitUsings, LangVersion
├── Directory.Packages.props       ← Versiones centralizadas NuGet
├── global.json                    ← SDK .NET 10 fijado
├── README.md
├── .editorconfig
├── src/
│   ├── Domain/                    ← KPG.Timesheet.Domain.csproj
│   │   └── (carpetas vacías: Common/, Constants/, Entities/, Enums/, Exceptions/)
│   ├── Application/               ← KPG.Timesheet.Application.csproj
│   │   └── (carpetas vacías: Common/Behaviours/, Common/Exceptions/, Common/Interfaces/, Features/)
│   ├── Infrastructure/            ← KPG.Timesheet.Infrastructure.csproj (ref→Application)
│   │   └── (carpetas vacías: Data/Configurations/, Identity/, Persistence/, Reports/, Security/, Services/)
│   ├── Api/                       ← KPG.Timesheet.Api.csproj (ref→Application, Infrastructure)
│   │   ├── Controllers/           ← vacío por ahora
│   │   ├── Middleware/
│   │   ├── OpenApi/
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   └── WebUI/                     ← KPG.Timesheet.WebUI.csproj (ref→Application)
│       ├── Features/
│       │   ├── Registro/
│       │   ├── Dashboard/
│       │   ├── Reportes/
│       │   └── Admin/
│       ├── Shared/
│       │   ├── Components/
│       │   ├── Layout/
│       │   └── Services/
│       ├── Infrastructure/
│       │   └── Repositories/
│       ├── wwwroot/
│       ├── App.razor
│       ├── Program.cs
│       └── Routes.razor
└── tests/
    ├── Domain.UnitTests/           ← KPG.Timesheet.Domain.UnitTests.csproj
    ├── Application.UnitTests/     ← KPG.Timesheet.Application.UnitTests.csproj
    └── Infrastructure.IntegrationTests/
```

### Dependencias entre proyectos

```
WebUI    → Application (DTOs)
Api      → Application + Infrastructure
Infrastructure → Application
Application → Domain
Domain  → (sin dependencias internas)
```

**El template Jason Taylor configura estas referencias automáticamente para los 4 proyectos backend.** Solo hay que agregar `WebUI → Application` manualmente.

### Convenciones de nombre — reglas para esta historia

| Artefacto | Patrón | Ejemplo |
|-----------|--------|---------|
| Proyecto | `KPG.Timesheet.{Capa}` | `KPG.Timesheet.Domain` |
| Namespace raíz | `KPG.Timesheet.{Capa}` | `KPG.Timesheet.Application` |
| Namespace de test | `KPG.Timesheet.{Capa}.{TestTipo}` | `KPG.Timesheet.Application.UnitTests` |

Si el template genera un nombre de proyecto distinto (ej: `CleanArchitecture.Domain`), renombrar el `.csproj`, el directorio y el namespace antes de continuar.

### Paquetes NuGet y sus proyectos destino

| Paquete | Proyecto | Propósito |
|---------|---------|-----------|
| Dapper 2.x | Infrastructure | Lectura de reportes/dashboard (nunca escritura) |
| MiniExcel | Infrastructure | Exportación Excel streaming |
| QuestPDF | Infrastructure | Exportación PDF (Community License, gratis) |
| MudBlazor | WebUI | Componentes UI — TODA la UI usa MudBlazor |
| Blazor-ApexCharts | WebUI | Gráficas en dashboard (paquete correcto: Blazor-ApexCharts con guion) |
| MediatR | Application | Ya incluido en template |
| FluentValidation | Application | Ya incluido en template |
| AutoMapper | Application | Ya incluido en template |
| EF Core 10.x | Infrastructure | Ya incluido en template |
| Serilog | Api | Puede venir en template; si no, agregar en historia 1.2 |

**IMPORTANTE:** No crear ninguna entidad de dominio, migration ni handler en esta historia. Los paquetes se agregan ahora para que las historias futuras puedan usarlos sin modificar el proyecto base.

### Notas de implementación

- El template v10.8.0 genera un archivo `.slnx` (formato nuevo) y subdirectorio. Se movió el contenido y se creó `.sln` con `dotnet new sln --format sln`.
- El template incluye proyectos Aspire (AppHost, ServiceDefaults, Shared) que fueron removidos por no ser parte del scope.
- El nombre correcto del paquete ApexCharts es `Blazor-ApexCharts` (con guion, no punto).
- Los tests fueron migrados de NUnit/Moq/Shouldly a xUnit/NSubstitute/FluentAssertions.
- Se configuró `NuGetAuditMode=direct` en Directory.Build.props para suprimir warnings de dependencias transitivas.

### Directory.Packages.props — gestión centralizada de versiones

Usar Central Package Management para evitar inconsistencias entre proyectos:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Dapper" Version="2.1.66" />
    <PackageVersion Include="MiniExcel" Version="1.35.0" />
    <PackageVersion Include="QuestPDF" Version="2025.4.0" />
    <PackageVersion Include="MudBlazor" Version="8.5.0" />
    <PackageVersion Include="Blazor-ApexCharts" Version="6.1.0" />
    <!-- Las versiones del template (MediatR, EF Core, FluentValidation, etc.) se agregan aquí también -->
  </ItemGroup>
</Project>
```

Cuando se usa `Directory.Packages.props`, los `PackageReference` en los `.csproj` no deben incluir `Version`. Si el template los incluye, eliminar el atributo `Version` de los `.csproj` y mover las versiones al archivo central.

### Sobre el proyecto Angular existente (Fronted/)

El directorio `Fronted/` contiene un proyecto Angular generado como prototipo. **No modificar ni eliminar** ningún archivo del directorio Angular — solo agregar `Fronted/LEGACY.md`. La decisión de eliminar o mantener el prototipo se toma fuera del alcance de esta historia.

### Alcance estricto de esta historia

**HACER en esta historia:**
- Scaffolding de la solución y proyectos
- Agregar paquetes NuGet
- Configurar archivos base (global.json, Directory.Build.props, Directory.Packages.props, .editorconfig)
- Crear estructura de carpetas vacías
- Documentar Angular como legacy

**NO HACER en esta historia (pertenece a historias futuras):**
- Configurar ASP.NET Core Identity o JWT → Historia 1.2
- Implementar `KpgAuthStateProvider` → Historia 1.3
- Configurar EF Core DbContext o migrations → Historia 1.2
- Implementar cualquier controller, handler, o componente → Historias 2+
- Configurar MudBlazor tema KPG → Historia 1.5
- Agregar pantalla de login → Historia 1.2
- Pantalla de carga Blazor → Historia 1.6

### Project Structure Notes

- La solución vive bajo `Backend/` (no en la raíz del repositorio Timesheet)
- El comando de build global es `dotnet build Backend/KPG.Timesheet.sln` (desde la raíz del repositorio)
- El comando de test global es `dotnet test Backend/KPG.Timesheet.sln`
- `Fronted/` (con typo en el nombre) es el directorio Angular legacy — mantener nombre tal como está para no romper referencias existentes

### References

- [Architecture doc — Scaffolding selection](../_bmad-output/planning-artifacts/architecture.md#scaffolding-seleccionado-híbrido-jason-taylor--blazor-wasm-estándar)
- [Architecture doc — Project structure](../_bmad-output/planning-artifacts/architecture.md#complete-project-directory-structure)
- [Architecture doc — Naming conventions](../_bmad-output/planning-artifacts/architecture.md#patrones-de-nomenclatura)
- [Architecture doc — Package decisions](../_bmad-output/planning-artifacts/architecture.md#evaluación-de-template-de-inicio)
- [Architecture doc — Mandatory agent rules](../_bmad-output/planning-artifacts/architecture.md#reglas-obligatorias-para-todos-los-agentes)
- [Epics — Story 1.1](../_bmad-output/planning-artifacts/epics.md#story-11-inicializar-solución-full-stack-kpg-timesheet)
- [Epics — Additional requirements](../_bmad-output/planning-artifacts/epics.md#additional-requirements)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- Template v10.8.0 genera `.slnx` (nuevo formato); se recreó como `.sln` con `dotnet new sln --format sln`
- Template incluye proyectos Aspire (AppHost, ServiceDefaults, Shared) — removidos
- Paquete ApexCharts: nombre correcto es `Blazor-ApexCharts` (con guion), versión `6.1.0`
- Tests migrados de NUnit/Moq/Shouldly → xUnit/NSubstitute/FluentAssertions según spec de historia
- `NuGetAuditMode=direct` configurado para evitar NU1903 de dependencias transitivas

### Completion Notes List

- Solución `Backend/KPG.Timesheet.sln` creada con 5 proyectos fuente + 3 de test
- Proyectos renombrados con prefijo `KPG.Timesheet.*` y namespaces actualizados
- Template limpiado: eliminadas features TodoItems, TodoLists, WeatherForecasts, Colour, PriorityLevel
- Paquetes Infrastructure: Dapper 2.1.66, MiniExcel 1.35.0, QuestPDF 2025.4.0
- Paquetes WebUI: MudBlazor 8.5.0, Blazor-ApexCharts 6.1.0
- Build: 0 errores, 0 advertencias
- Tests: 3 pasados (ValidationExceptionTests en Application.UnitTests)
- `Fronted/LEGACY.md` creado documentando Angular como legacy
- WebUI folder structure creada con .gitkeep files

### File List

Backend/KPG.Timesheet.sln
Backend/global.json
Backend/Directory.Build.props
Backend/Directory.Packages.props
Backend/README.md
Backend/.editorconfig
Backend/src/Domain/KPG.Timesheet.Domain.csproj
Backend/src/Domain/GlobalUsings.cs
Backend/src/Domain/Common/BaseAuditableEntity.cs
Backend/src/Domain/Common/BaseEntity.cs
Backend/src/Domain/Common/BaseEvent.cs
Backend/src/Domain/Common/ValueObject.cs
Backend/src/Domain/Constants/Roles.cs
Backend/src/Application/KPG.Timesheet.Application.csproj
Backend/src/Application/GlobalUsings.cs
Backend/src/Application/DependencyInjection.cs
Backend/src/Application/Common/Behaviours/AuthorizationBehaviour.cs
Backend/src/Application/Common/Behaviours/LoggingBehaviour.cs
Backend/src/Application/Common/Behaviours/PerformanceBehaviour.cs
Backend/src/Application/Common/Behaviours/UnhandledExceptionBehaviour.cs
Backend/src/Application/Common/Behaviours/ValidationBehaviour.cs
Backend/src/Application/Common/Exceptions/ForbiddenAccessException.cs
Backend/src/Application/Common/Exceptions/ValidationException.cs
Backend/src/Application/Common/Interfaces/IApplicationDbContext.cs
Backend/src/Application/Common/Interfaces/IIdentityService.cs
Backend/src/Application/Common/Interfaces/IUser.cs
Backend/src/Application/Common/Models/LookupDto.cs
Backend/src/Application/Common/Models/Result.cs
Backend/src/Application/Common/Security/AuthorizeAttribute.cs
Backend/src/Infrastructure/KPG.Timesheet.Infrastructure.csproj
Backend/src/Infrastructure/GlobalUsings.cs
Backend/src/Infrastructure/DependencyInjection.cs
Backend/src/Infrastructure/Data/ApplicationDbContext.cs
Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs
Backend/src/Infrastructure/Data/Interceptors/AuditableEntityInterceptor.cs
Backend/src/Infrastructure/Data/Interceptors/DispatchDomainEventsInterceptor.cs
Backend/src/Infrastructure/Identity/ApplicationUser.cs
Backend/src/Infrastructure/Identity/IdentityResultExtensions.cs
Backend/src/Infrastructure/Identity/IdentityService.cs
Backend/src/Api/KPG.Timesheet.Api.csproj
Backend/src/Api/GlobalUsings.cs
Backend/src/Api/DependencyInjection.cs
Backend/src/Api/Program.cs
Backend/src/Api/Endpoints/Users.cs
Backend/src/Api/Infrastructure/ApiExceptionOperationTransformer.cs
Backend/src/Api/Infrastructure/BearerSecuritySchemeTransformer.cs
Backend/src/Api/Infrastructure/EndpointRouteBuilderExtensions.cs
Backend/src/Api/Infrastructure/IdentityApiOperationTransformer.cs
Backend/src/Api/Infrastructure/IEndpointGroup.cs
Backend/src/Api/Infrastructure/MethodInfoExtensions.cs
Backend/src/Api/Infrastructure/ProblemDetailsExceptionHandler.cs
Backend/src/Api/Infrastructure/WebApplicationExtensions.cs
Backend/src/Api/Services/CurrentUser.cs
Backend/src/WebUI/KPG.Timesheet.WebUI.csproj
Backend/src/WebUI/App.razor
Backend/src/WebUI/Program.cs
Backend/src/WebUI/Routes.razor
Backend/src/WebUI/Features/Registro/.gitkeep
Backend/src/WebUI/Features/Dashboard/.gitkeep
Backend/src/WebUI/Features/Reportes/.gitkeep
Backend/src/WebUI/Features/Admin/.gitkeep
Backend/src/WebUI/Shared/Components/.gitkeep
Backend/src/WebUI/Shared/Layout/.gitkeep
Backend/src/WebUI/Shared/Services/.gitkeep
Backend/src/WebUI/Infrastructure/Repositories/.gitkeep
Backend/tests/Domain.UnitTests/KPG.Timesheet.Domain.UnitTests.csproj
Backend/tests/Application.UnitTests/KPG.Timesheet.Application.UnitTests.csproj
Backend/tests/Application.UnitTests/Common/Exceptions/ValidationExceptionTests.cs
Backend/tests/Infrastructure.IntegrationTests/KPG.Timesheet.Infrastructure.IntegrationTests.csproj
Backend/tests/Infrastructure.IntegrationTests/GlobalUsings.cs
Fronted/LEGACY.md

### Change Log

- 2026-05-13: Historia 1.1 implementada completa. Solución KPG.Timesheet inicializada con Clean Architecture + Blazor WASM. Template adaptado (Aspire removido, proyectos renombrados, tests migrados a xUnit). Paquetes base agregados. Estructura WebUI creada. Angular documentado como legacy.
- 2026-05-13: Corrección arquitectónica — WebUI separado de Backend. Ahora: Backend/KPG.Timesheet.sln (4 proyectos backend) + Fronted/KPG.Timesheet.WebUI.sln (Blazor WASM). Angular legacy movido a Fronted/legacy-angular-prototype/.
