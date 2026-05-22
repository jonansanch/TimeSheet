# Arquitectura Backend Implementada

Fecha de revision: 2026-05-20

## Resumen ejecutivo

El backend esta implementado con una arquitectura por capas cercana a Clean Architecture:

- `Domain`: reglas de negocio, entidades, enums, constantes y excepciones de dominio.
- `Application`: casos de uso, comandos, queries, validaciones, contratos e interfaces.
- `Infrastructure`: EF Core, Identity, Dapper, servicios concretos, jobs, email, reportes, bitacora y migraciones.
- `Api`: Minimal APIs, autenticacion/autorizacion HTTP, OpenAPI/Scalar, middleware de errores y composicion de servicios.

El flujo principal es:

```text
HTTP request
  -> Api/Endpoints/*
  -> MediatR command/query
  -> Application handler o Infrastructure handler
  -> IApplicationDbContext / IDbConnection / servicios concretos
  -> SQL Server
```

La separacion general es sana, pero hay algunos puntos donde el orden de carpetas y las responsabilidades pueden aclararse mejor, especialmente en handlers Dapper ubicados en `Infrastructure`, endpoints agrupados por recurso, y servicios transversales como bitacora/reportes/dashboard.

## Proyectos y dependencias

### `KPG.Timesheet.Domain`

Responsabilidad:

- Modelo de negocio puro.
- Entidades e invariantes.
- Excepciones tipadas de dominio.
- Constantes y enums usados por capas superiores.

Carpetas actuales:

```text
Backend/src/Domain/
  Common/
  Constants/
  Entities/
  Enums/
  Exceptions/
```

Ejemplos:

- `Entities/RegistroHoras.cs`
- `Entities/SolicitudExcepcion.cs`
- `Entities/BitacoraAuditoria.cs`
- `Exceptions/DomainRuleException.cs`
- `Constants/Roles.cs`
- `Constants/TipoEventoBitacora.cs`

Dependencias:

- Solo referencia `MediatR.Contracts`.
- No referencia `Application`, `Infrastructure` ni `Api`.

Observacion:

Esta capa esta bien aislada. Las reglas de negocio esperadas ya usan `DomainRuleException`, lo cual evita depender de excepciones genericas como `ArgumentException` para reglas de dominio.

### `KPG.Timesheet.Application`

Responsabilidad:

- Casos de uso del sistema.
- Commands y queries MediatR.
- Validadores FluentValidation.
- Contratos hacia infraestructura.
- Behaviours transversales de MediatR.
- Excepciones de aplicacion.

Carpetas actuales:

```text
Backend/src/Application/
  Common/
    Behaviours/
    Exceptions/
    Interfaces/
    Models/
    Security/
  Features/
    Auth/
    Bitacora/
    Catalogos/
    Dashboard/
    Notificaciones/
    RegistroHoras/
    Reportes/
    Sistema/
    SolicitudesExcepcion/
    Users/
```

Patron dominante:

```text
Features/{Modulo}/{Commands|Queries}/{CasoDeUso}/
  CasoDeUsoCommand.cs
  CasoDeUsoCommandValidator.cs
  CasoDeUsoCommandHandler.cs
  Dto.cs
```

Ejemplos:

- `Features/RegistroHoras/Commands/CreateRegistroHoras/CreateRegistroHorasCommand.cs`
- `Features/RegistroHoras/Commands/CreateRegistroHoras/CreateRegistroHorasCommandHandler.cs`
- `Features/SolicitudesExcepcion/Commands/AprobarSolicitudExcepcion/AprobarSolicitudExcepcionCommandHandler.cs`
- `Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs`

Dependencias:

- Referencia `Domain`.
- Define interfaces como `IApplicationDbContext`, `IUser`, `IIdentityService`, `IEmailService`, `IBitacoraService`, `IClock`.
- Usa EF Core abstraido por `IApplicationDbContext`.

Observacion:

Aqui vive buena parte de la logica de escritura. Sin embargo, no todos los handlers viven aqui: varios queries pesados con Dapper viven en `Infrastructure`. Eso es funcional, pero rompe la expectativa visual de que todos los handlers de una feature esten juntos.

### `KPG.Timesheet.Infrastructure`

Responsabilidad:

- Implementaciones concretas de interfaces de `Application`.
- EF Core DbContext, configuraciones, migraciones e interceptores.
- Identity y JWT.
- Dapper para consultas/reportes.
- Generacion de Excel/PDF.
- Email.
- Jobs background.
- Bitacora.

Carpetas actuales:

```text
Backend/src/Infrastructure/
  Bitacora/
  Dashboard/
  Data/
    Configurations/
    Interceptors/
  Email/
  Identity/
  Jobs/
  Migrations/
  Notificaciones/
  Reportes/
  Services/
```

Ejemplos:

- `Data/ApplicationDbContext.cs`
- `Data/ApplicationDbContextInitialiser.cs`
- `Data/Interceptors/AuditableEntityInterceptor.cs`
- `Data/Interceptors/RegistroHorasImmutabilityInterceptor.cs`
- `Identity/IdentityService.cs`
- `Identity/JwtTokenService.cs`
- `Bitacora/GetBitacoraQueryHandler.cs`
- `Reportes/ExportarTimesheetQueryHandler.cs`
- `Dashboard/GetEstadoEquipoQueryHandler.cs`
- `Jobs/NotificacionesPendientesJob.cs`

Dependencias:

- Referencia `Application`.
- Por transitividad accede a `Domain`.
- Registra EF Core, Identity, JWT, Dapper, email, bitacora y handlers MediatR de infraestructura.

Observacion:

Esta capa actualmente contiene handlers MediatR para queries que usan Dapper. El contrato de la query vive en `Application`, pero el handler concreto vive en `Infrastructure`. Es una decision valida si se quiere mantener SQL/Dapper fuera de `Application`, pero debe documentarse como regla arquitectonica porque no es obvia al navegar carpetas.

### `KPG.Timesheet.Api`

Responsabilidad:

- Host ASP.NET Core.
- Minimal APIs.
- Autenticacion/autorizacion HTTP.
- Problem Details.
- OpenAPI/Scalar.
- CORS, Serilog y pipeline HTTP.
- Composicion de dependencias.

Carpetas actuales:

```text
Backend/src/Api/
  Endpoints/
  Infrastructure/
  Services/
  Properties/
  wwwroot/
```

Ejemplos:

- `Program.cs`
- `DependencyInjection.cs`
- `Endpoints/RegistroHoras.cs`
- `Endpoints/Bitacora.cs`
- `Infrastructure/ProblemDetailsExceptionHandler.cs`
- `Infrastructure/EndpointRouteBuilderExtensions.cs`
- `Services/CurrentUser.cs`

Dependencias:

- Referencia `Application`.
- Referencia `Infrastructure`.

Observacion:

Los endpoints estan agrupados por recurso y despachan a MediatR. El patron es limpio: el endpoint no contiene logica de negocio relevante, solo binding, autorizacion, metadata OpenAPI y envio de command/query.

## Flujo de ejecucion

### Escrituras transaccionales con EF Core

Ejemplo: crear registro de horas.

```text
POST /api/registros-horas
  -> Api/Endpoints/RegistroHoras.Create
  -> CreateRegistroHorasCommand
  -> CreateRegistroHorasCommandHandler
  -> IApplicationDbContext.RegistrosHoras.Add
  -> SaveChangesAsync
  -> EF interceptors
  -> SQL Server
```

Puntos relevantes:

- El handler vive en `Application`.
- Las invariantes de `RegistroHoras` viven en `Domain`.
- La persistencia real vive en `Infrastructure/Data/ApplicationDbContext`.
- Los interceptores aplican auditoria e inmutabilidad.

### Lecturas/reportes con Dapper

Ejemplo: consultar bitacora.

```text
GET /api/bitacora
  -> Api/Endpoints/Bitacora.GetBitacora
  -> GetBitacoraQuery
  -> Infrastructure/Bitacora/GetBitacoraQueryHandler
  -> IDbConnection + Dapper
  -> SQL Server
```

Puntos relevantes:

- El request/response contract vive en `Application/Features/Bitacora/Queries/GetBitacora`.
- El handler vive en `Infrastructure/Bitacora`.
- La autorizacion se declara en el query con `[Authorize]` y tambien en el endpoint con `RequireAuthorization`.
- Este patron se repite en dashboard, reportes, notificaciones y bitacora.

### Autorizacion

Hay dos niveles:

1. Nivel HTTP en endpoints con `RequireAuthorization`.
2. Nivel MediatR con `AuthorizationBehaviour` leyendo atributos `[Authorize]` en commands/queries.

Ejemplo:

- `GetBitacoraQuery` tiene `[Authorize(Roles = Roles.Admin)]`.
- `Endpoints/Bitacora.cs` tambien restringe el endpoint a Admin.

Observacion:

La doble autorizacion es defensiva. Si se mantiene, conviene establecer como regla que todo caso de uso sensible tenga atributo en `Application`, y que el endpoint sea una segunda barrera.

### Validacion y errores

Capas de validacion:

- FluentValidation en `Application` para request/command.
- Invariantes en `Domain` para reglas del negocio.
- `ProblemDetailsExceptionHandler` en `Api` para traducir excepciones.

Mapeo actual:

- `ValidationException` -> 400.
- `DomainRuleException` -> 400.
- `NotFoundException` -> 404.
- `UnauthorizedAccessException` -> 401.
- `ForbiddenAccessException` -> 403.
- excepciones no previstas -> 500.

### Persistencia y auditoria

`ApplicationDbContext` hereda de `IdentityDbContext<ApplicationUser>` y expone DbSets del dominio:

- `RegistrosHoras`
- `SolicitudesExcepcion`
- `ParametrosSistema`
- `Clientes`
- `Proyectos`
- `Empleados`
- `Modalidades`
- `LugaresTrabajo`
- `NotificacionesEnviadas`
- `BitacoraAuditoria`
- `RefreshTokens`

Interceptors registrados:

- `AuditableEntityInterceptor`: llena `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy`.
- `RegistroHorasImmutabilityInterceptor`: impide cambios a campos inmutables de registros guardados.
- `DispatchDomainEventsInterceptor`: despacha eventos de dominio si existen.

Observacion:

El orden de registro actual es auditoria, inmutabilidad, eventos. Como `RegistroHorasImmutabilityInterceptor` permite `LastModified` y `LastModifiedBy`, no choca con auditoria.

## Organizacion actual por feature

### Features con handlers principalmente en `Application`

- `Auth`
- `Catalogos`
- `RegistroHoras`
- `Sistema`
- `SolicitudesExcepcion`
- `Users`

Estos modulos mantienen command/query/handler dentro de la misma carpeta de `Application/Features`.

### Features con query contract en `Application` y handler en `Infrastructure`

- `Bitacora`
- `Dashboard`
- `Notificaciones`
- `Reportes`

Motivo probable:

- Usan Dapper o generacion concreta de archivos.
- Requieren SQL optimizado, `IDbConnection`, ClosedXML, QuestPDF o MiniExcel.
- Evitan meter infraestructura concreta en `Application`.

Riesgo de navegacion:

- Al buscar un handler desde `Application/Features/Reportes`, no esta ahi.
- El nombre de carpeta en `Infrastructure` agrupa por modulo, pero ya no replica exactamente `Features/{Modulo}/Queries/{CasoDeUso}`.

## Puntos donde podria haber cambio arquitectonico

### 1. Handlers repartidos entre `Application` e `Infrastructure`

Estado actual:

- Handlers EF Core simples viven en `Application`.
- Handlers Dapper/reportes viven en `Infrastructure`.

Ventaja:

- `Application` queda libre de SQL/Dapper/ClosedXML/QuestPDF.

Costo:

- La feature queda partida entre dos proyectos.
- Es menos evidente donde implementar un nuevo query.

Opciones:

- Mantenerlo, pero documentar la regla: "Commands y queries EF viven en Application; queries Dapper/exportaciones viven en Infrastructure".
- O mover todos los handlers a `Application` y crear interfaces tipo `IReporteReadRepository`, `IBitacoraReadRepository`, etc. Infrastructure implementaria esos repositorios. Esto mejora consistencia de MediatR, pero agrega mas interfaces.
- O replicar estructura de carpetas en Infrastructure: `Infrastructure/Features/Bitacora/Queries/GetBitacora/GetBitacoraQueryHandler.cs`. Esto conserva SQL fuera de Application y hace mas clara la relacion con el contrato.

Recomendacion inicial:

Mantener handlers Dapper en Infrastructure, pero reordenarlos bajo `Infrastructure/Features/{Modulo}/{Queries|Commands}/{CasoDeUso}` para que coincidan visualmente con `Application/Features`.

### 2. `Application` expone `DbSet` en `IApplicationDbContext`

Estado actual:

`IApplicationDbContext` expone `DbSet<T>` directamente.

Ventaja:

- Simple y rapido.
- Los handlers pueden usar LINQ/EF sin repositorios ceremoniales.

Costo:

- `Application` depende de EF Core.
- Los casos de uso conocen detalles de persistencia.

Opciones:

- Mantenerlo. Es comun en Clean Architecture pragmatica con EF Core.
- Si se quiere una capa mas estricta, cambiar a repositorios/puertos por agregado. Eso es mas trabajo y no necesariamente aporta para este proyecto.

Recomendacion inicial:

No cambiar todavia. Para este MVP, `IApplicationDbContext` con `DbSet` es aceptable.

### 3. `ApplicationDbContextInitialiser` mezcla EnsureCreated, SQL manual y seed

Estado actual:

- En desarrollo se llama `InitialiseDatabaseAsync`.
- Usa `EnsureCreatedAsync`.
- Ejecuta SQL manual para asegurar tablas/columnas.
- Ejecuta seeds de roles, usuarios, catalogos, registros.

Riesgo:

- Mezcla migraciones EF con inicializacion manual.
- Puede causar diferencias entre ambientes.
- Algunas tablas historicas parecen haber sido creadas antes por SQL manual, mientras nuevas features usan migraciones.

Recomendacion:

Separar en:

```text
Data/
  ApplicationDbContext.cs
  Migrations/
  Seed/
    DevelopmentDataSeeder.cs
    IdentitySeeder.cs
    CatalogSeeder.cs
```

Y decidir una regla unica:

- Desarrollo: `Database.MigrateAsync()` + seed idempotente.
- Produccion: migraciones controladas por deploy, sin `EnsureCreated`.

### 4. Bitacora como modulo transversal

Estado actual:

- Contrato `IBitacoraService` vive en `Application/Common/Interfaces`.
- Implementacion `BitacoraService` vive en `Infrastructure/Bitacora`.
- Entidad `BitacoraAuditoria` vive en `Domain`.
- Queries viven entre `Application/Features/Bitacora` e `Infrastructure/Bitacora`.

Observacion:

Esta bien como modulo transversal, pero ahora que Epic 6 crecio, Bitacora ya no es solo servicio tecnico: es una feature completa.

Recomendacion:

Ordenar como feature:

```text
Application/Features/Bitacora/
  Queries/
  Services? solo contratos si aplica

Infrastructure/Features/Bitacora/
  Queries/
  Services/
```

### 5. Reportes y Dashboard viven como carpetas planas en Infrastructure

Estado actual:

```text
Infrastructure/Reportes/
Infrastructure/Dashboard/
Infrastructure/Notificaciones/
```

Recomendacion:

Si se decide estandarizar, mover a:

```text
Infrastructure/Features/Reportes/Queries/...
Infrastructure/Features/Dashboard/Queries/...
Infrastructure/Features/Notificaciones/Queries/...
```

Esto haria que `Application/Features` e `Infrastructure/Features` se lean como espejo.

### 6. Endpoints Minimal API estan bien, pero pueden crecer mucho

Estado actual:

- Un archivo por grupo de endpoints.
- Cada archivo contiene mapping y handlers HTTP.

Esto esta bien para el tamano actual. Si crece, una opcion seria:

```text
Api/Endpoints/
  RegistroHoras/
    RegistroHorasEndpoints.cs
    RegistroHorasRequests.cs
  Bitacora/
    BitacoraEndpoints.cs
```

Recomendacion:

No cambiar aun salvo que algun archivo se vuelva dificil de navegar.

## Propuesta de orden objetivo

Sin cambiar comportamiento, el reordenamiento mas util seria:

```text
Backend/src/
  Domain/
    Common/
    Constants/
    Entities/
    Enums/
    Exceptions/

  Application/
    Common/
      Behaviours/
      Exceptions/
      Interfaces/
      Models/
      Security/
    Features/
      RegistroHoras/
      SolicitudesExcepcion/
      Catalogos/
      Users/
      Auth/
      Sistema/
      Bitacora/
      Dashboard/
      Reportes/
      Notificaciones/

  Infrastructure/
    Data/
      Configurations/
      Interceptors/
      Seed/
      Migrations/
    Identity/
    Email/
    Jobs/
    Services/
    Features/
      Bitacora/
        Queries/
        Services/
      Dashboard/
        Queries/
      Reportes/
        Queries/
      Notificaciones/
        Queries/

  Api/
    Endpoints/
    Infrastructure/
    Services/
```

## Reglas arquitectonicas recomendadas

1. `Domain` no depende de ninguna capa.
2. `Application` depende de `Domain`, pero no de `Infrastructure` ni `Api`.
3. `Infrastructure` implementa interfaces de `Application`.
4. `Api` solo compone y expone HTTP.
5. Commands que escriben datos deben vivir en `Application`.
6. Queries simples pueden vivir completamente en `Application` si usan `IApplicationDbContext`.
7. Queries optimizadas con Dapper deben tener contrato en `Application` y handler en `Infrastructure/Features/{Modulo}`.
8. Exportaciones con ClosedXML/QuestPDF deben permanecer en `Infrastructure`.
9. Reglas de negocio esperadas van en `Domain` y lanzan `DomainRuleException`.
10. Validaciones de request van en FluentValidation dentro de `Application`.
11. Autorizacion sensible debe declararse en el command/query con `[Authorize]`; el endpoint puede repetirla como barrera HTTP.
12. Seeds y datos de QA deben estar separados de inicializacion/migracion estructural.

## Cambios de bajo riesgo sugeridos

1. Crear `Infrastructure/Features/`.
2. Mover handlers actuales:
   - `Infrastructure/Bitacora/*QueryHandler.cs` -> `Infrastructure/Features/Bitacora/Queries/...`
   - `Infrastructure/Dashboard/*QueryHandler.cs` -> `Infrastructure/Features/Dashboard/Queries/...`
   - `Infrastructure/Reportes/*QueryHandler.cs` -> `Infrastructure/Features/Reportes/Queries/...`
   - `Infrastructure/Notificaciones/*QueryHandler.cs` -> `Infrastructure/Features/Notificaciones/Queries/...`
3. Dejar servicios concretos donde correspondan:
   - `BitacoraService` puede ir a `Infrastructure/Features/Bitacora/Services/`.
   - `SmtpEmailService` puede quedarse en `Infrastructure/Email/`.
   - `IdentityService` se queda en `Infrastructure/Identity/`.
4. Separar seed:
   - `ApplicationDbContextInitialiser` conserva inicializacion.
   - Crear seeders especializados en `Infrastructure/Data/Seed/`.
5. Actualizar namespaces y verificar build/tests.

## Cambios que no recomiendo hacer todavia

- No introducir repositorios para todo el dominio solo por pureza.
- No mover entidades de `Domain`.
- No mover endpoints a controllers; Minimal API esta funcionando bien.
- No eliminar la doble autorizacion hasta tener una regla clara de seguridad.
- No mezclar SQL Dapper dentro de `Application` si el objetivo es mantener infraestructura fuera de casos de uso.

## Decision pendiente

Antes de reorganizar carpetas, conviene decidir una de estas dos reglas:

### Opcion A: Pragmatica actual, documentada

Handlers con EF Core en `Application`; handlers Dapper/exportacion en `Infrastructure`.

Esta opcion requiere solo ordenar carpetas y namespaces.

### Opcion B: MediatR centralizado en Application

Todos los handlers viven en `Application`. Para Dapper/reportes se crean puertos como:

- `IBitacoraReadService`
- `IReporteHorasReader`
- `IDashboardReader`

Infrastructure implementa esos servicios.

Esta opcion deja los casos de uso juntos, pero aumenta interfaces y archivos.

### Recomendacion

Para el estado actual del proyecto, elegiria la Opcion A con una reorganizacion de `Infrastructure/Features`. Es el menor cambio, respeta el diseño ya implementado y mejora bastante la navegabilidad.
