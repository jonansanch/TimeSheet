# Handoff de Sesion - 2026-05-14

## Estado General

Se implemento Story 2.1: Registrar Turno AM/PM con Validacion.

Archivo de story:

- `Docs/stories/2.1-registrar-turno-am-pm-con-validacion.md`

Estado de la story:

- `Status: review`
- Todas las tareas/subtareas quedaron marcadas como completadas.
- QA formal pendiente.

## Puertos Fijos

Backend API:

- HTTPS: `https://localhost:5101`
- HTTP: `http://localhost:5100`
- Scalar/OpenAPI: `https://localhost:5101/scalar`

Frontend WebUI:

- HTTPS: `https://localhost:5201`
- HTTP: `http://localhost:5200`

El WebUI apunta al backend en:

- `https://localhost:5101`

Archivos actualizados para puertos:

- `Backend/src/Api/Properties/launchSettings.json`
- `Backend/src/Api/appsettings.Development.json`
- `Fronted/src/WebUI/Properties/launchSettings.json`
- `Fronted/src/WebUI/wwwroot/appsettings.json`
- `Fronted/src/WebUI/wwwroot/appsettings.Development.json`

## Usuario De Prueba

Empleado:

- Email: `empleado@kpg.com`
- Password: `Empleado1234!`

Admin:

- Email: `admin@kpg.com`
- Password: `Admin1234!`

Estos usuarios se crean en:

- `Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`

## Implementacion Realizada

Backend:

- Entidad `RegistroHoras`
- Enum `TurnoRegistro`
- Configuracion EF Core para tabla `RegistrosHoras`
- `DbSet<RegistroHoras>` en `IApplicationDbContext` y `ApplicationDbContext`
- Command CQRS `CreateRegistroHorasCommand`
- Validator `CreateRegistroHorasCommandValidator`
- Handler `CreateRegistroHorasCommandHandler`
- DTO `RegistroHorasDto`
- Endpoint protegido `POST /api/registros-horas`
- Fix en `CurrentUser` para leer `sub` del JWT cuando no existe `ClaimTypes.NameIdentifier`

Frontend:

- Pantalla `/registro`
- Componente `KpgShiftForm`
- Repositorio `RegistroHorasRepository`
- Modelos `CreateRegistroHorasRequest`, `RegistroHorasResponse`, `TurnoRegistro`
- Registro de `IRegistroHorasRepository` en DI
- Validacion cliente con MudBlazor
- Fecha de hoy por defecto
- Turno AM activo por defecto

Auth / Refresh Token:

- Se detecto que la base `TimesheetDb` ya existia pero no tenia tabla `RefreshTokens`.
- Se agrego inicializacion defensiva para crear `RefreshTokens` y `RegistrosHoras` si faltan.
- Se corrigio el `ProblemDetailsExceptionHandler` para devolver Problem Details 500 en errores no manejados, evitando el error de `ExceptionHandlerOptions produced a 404`.
- Se agrego `SemaphoreSlim` en `KpgAuthStateProvider` para evitar refresh simultaneos que roten el mismo token dos veces.
- `AuthRepository` ahora captura `HttpRequestException` en login/refresh y retorna `null` en vez de romper la UI.

## Archivos Tocadas En Story 2.1 Y Fixes Posteriores

Backend:

- `Backend/Directory.Packages.props`
- `Backend/src/Api/Endpoints/RegistroHoras.cs`
- `Backend/src/Api/Infrastructure/ProblemDetailsExceptionHandler.cs`
- `Backend/src/Api/Properties/launchSettings.json`
- `Backend/src/Api/Services/CurrentUser.cs`
- `Backend/src/Api/appsettings.Development.json`
- `Backend/src/Application/Common/Interfaces/IApplicationDbContext.cs`
- `Backend/src/Application/Features/RegistroHoras/Commands/CreateRegistroHoras/CreateRegistroHorasCommand.cs`
- `Backend/src/Application/Features/RegistroHoras/Commands/CreateRegistroHoras/CreateRegistroHorasCommandHandler.cs`
- `Backend/src/Application/Features/RegistroHoras/Commands/CreateRegistroHoras/CreateRegistroHorasCommandValidator.cs`
- `Backend/src/Application/Features/RegistroHoras/Commands/CreateRegistroHoras/RegistroHorasDto.cs`
- `Backend/src/Domain/Entities/RegistroHoras.cs`
- `Backend/src/Domain/Enums/TurnoRegistro.cs`
- `Backend/src/Infrastructure/Data/ApplicationDbContext.cs`
- `Backend/src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`
- `Backend/src/Infrastructure/Data/Configurations/RegistroHorasConfiguration.cs`
- `Backend/tests/Application.UnitTests/Features/RegistroHoras/CreateRegistroHorasCommandValidatorTests.cs`
- `Backend/tests/Domain.UnitTests/Entities/RegistroHorasTests.cs`
- `Backend/tests/Infrastructure.IntegrationTests/KPG.Timesheet.Infrastructure.IntegrationTests.csproj`
- `Backend/tests/Infrastructure.IntegrationTests/RegistroHoras/CreateRegistroHorasCommandHandlerTests.cs`

Frontend:

- `Fronted/src/WebUI/Features/Registro/Components/KpgShiftForm.razor`
- `Fronted/src/WebUI/Features/Registro/Pages/RegistroPage.razor`
- `Fronted/src/WebUI/Infrastructure/Repositories/IRegistroHorasRepository.cs`
- `Fronted/src/WebUI/Infrastructure/Repositories/RegistroHorasRepository.cs`
- `Fronted/src/WebUI/Infrastructure/Repositories/AuthRepository.cs`
- `Fronted/src/WebUI/Infrastructure/Repositories/Models/RegistroHorasModels.cs`
- `Fronted/src/WebUI/Properties/launchSettings.json`
- `Fronted/src/WebUI/Program.cs`
- `Fronted/src/WebUI/Shared/Services/KpgAuthStateProvider.cs`
- `Fronted/src/WebUI/_Imports.razor`
- `Fronted/src/WebUI/wwwroot/appsettings.Development.json`
- `Fronted/src/WebUI/wwwroot/appsettings.json`

Docs:

- `Docs/stories/2.1-registrar-turno-am-pm-con-validacion.md`
- `Docs/session-logs/2026-05-14-story-2-1-handoff.md`

## Verificacion Ejecutada

Backend:

```powershell
dotnet test Backend\KPG.Timesheet.sln
```

Resultado:

- Correcto
- 17 tests pasados

Frontend:

```powershell
dotnet build Fronted\KPG.Timesheet.WebUI.sln
```

Resultado:

- Correcto
- 0 warnings
- 0 errores

Prueba manual API:

- `POST https://localhost:5101/api/auth/login` con `empleado@kpg.com` / `Empleado1234!` retorno `200`.
- `POST https://localhost:5101/api/auth/refresh` con el refresh token retornado retorno `200`.

## Como Arrancar Manana

1. Confirmar que no haya procesos dotnet viejos ocupando puertos:

```powershell
netstat -ano | Select-String ":5100|:5101|:5200|:5201"
```

2. Arrancar backend:

```powershell
dotnet run --project Backend\src\Api\KPG.Timesheet.Api.csproj --launch-profile https
```

3. Arrancar frontend:

```powershell
dotnet run --project Fronted\src\WebUI\KPG.Timesheet.WebUI.csproj --launch-profile https
```

4. Abrir:

```text
https://localhost:5201
```

5. Login:

```text
empleado@kpg.com / Empleado1234!
```

6. Probar `/registro`.

## Riesgos / Pendientes Tecnicos

- La creacion de tablas faltantes en `ApplicationDbContextInitialiser` es una solucion pragmatica de desarrollo. Lo ideal para adelante es instalar `dotnet-ef` y formalizar migraciones EF.
- `RegistroHoras` usa campos simples de texto para cliente/proyecto/modalidad/recurso porque los catalogos reales son Epic 4. Esto esta alineado con la story para no adelantar CRUD de catalogos.
- `KpgShiftForm` usa listas temporales hardcodeadas para cliente/proyecto/modalidad/recurso. Story 2.2 debe reemplazar/mejorar esto con seleccion contextual, sugerencias recientes y filtros.
- QA formal de Story 2.1 pendiente.

## Siguiente Paso Recomendado

Antes de empezar Story 2.2, hacer una pasada de review QA sobre Story 2.1:

- Probar login y refresh desde navegador.
- Probar guardar AM.
- Probar intento duplicado mismo usuario/fecha/turno.
- Probar validaciones requeridas y hora salida <= entrada.

Despues:

- Ejecutar `bmad-code-review` sobre Story 2.1, o
- Continuar con Story 2.2: Seleccionar Cliente, Proyecto y Contexto con Sugerencias Recientes.
