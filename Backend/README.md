# KPG Timesheet — Backend

Solución .NET 10 con Clean Architecture. Generada con [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) v10.8.0 y adaptada para KPG Timesheet.

## Estructura

```
Backend/
├── KPG.Timesheet.sln
├── Directory.Build.props          — Nullable, ImplicitUsings, LangVersion
├── Directory.Packages.props       — Versiones NuGet centralizadas
├── global.json                    — SDK .NET 10 fijado
├── .editorconfig                  — Estilos de código C#
├── src/
│   ├── Domain/                    — KPG.Timesheet.Domain
│   ├── Application/               — KPG.Timesheet.Application
│   ├── Infrastructure/            — KPG.Timesheet.Infrastructure
│   ├── Api/                       — KPG.Timesheet.Api (ASP.NET Core Minimal API)
│   └── WebUI/                     — KPG.Timesheet.WebUI (Blazor WebAssembly)
└── tests/
    ├── Domain.UnitTests/
    ├── Application.UnitTests/
    └── Infrastructure.IntegrationTests/
```

## Dependencias entre proyectos

```
WebUI       → Application
Api         → Application + Infrastructure
Infrastructure → Application
Application → Domain
Domain      → (sin dependencias internas)
```

## Comandos

### Build

```bash
dotnet build Backend/KPG.Timesheet.sln
```

### Test

```bash
dotnet test Backend/KPG.Timesheet.sln
```

### Ejecutar API

```bash
dotnet run --project Backend/src/Api
```

### Ejecutar WebUI (Blazor)

```bash
dotnet run --project Backend/src/WebUI
```

## Scaffolding de casos de uso

```bash
dotnet new ca-usecase --name CreateRegistro --feature-name Registros --usecase-type command --return-type int
dotnet new ca-usecase --name GetRegistros --feature-name Registros --usecase-type query --return-type RegistrosVm
```

## Código de estilos

El proyecto incluye soporte para [EditorConfig](https://editorconfig.org/) definido en `.editorconfig`.
