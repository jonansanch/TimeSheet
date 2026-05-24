# Arquitectura del Backend — KPG Timesheet

**Versión:** 1.0  
**Fecha:** 2026-05-23  
**Stack:** .NET 10 · ASP.NET Core Minimal API · EF Core · Dapper · MediatR · SQL Server

---

## Tabla de Contenidos

1. [Visión General](#1-visión-general)
2. [Clean Architecture — Capas](#2-clean-architecture--capas)
3. [Capa Domain](#3-capa-domain)
4. [Capa Application](#4-capa-application)
5. [Capa Infrastructure](#5-capa-infrastructure)
6. [Capa Api](#6-capa-api)
7. [Flujo de una Petición](#7-flujo-de-una-petición)
8. [Patrones Clave](#8-patrones-clave)
9. [Base de Datos](#9-base-de-datos)
10. [Autenticación y Autorización](#10-autenticación-y-autorización)
11. [Diagrama de Dependencias](#11-diagrama-de-dependencias)

---

## 1. Visión General

El backend de KPG Timesheet sigue **Clean Architecture** (arquitectura limpia), un estilo donde el código se organiza en capas concéntricas con una regla de dependencia estricta: **las capas internas no conocen a las externas**.

```
┌─────────────────────────────────────┐
│              Api (Web)              │  ← Capa más externa
│  ┌───────────────────────────────┐  │
│  │        Infrastructure        │  │  ← Implementaciones concretas
│  │  ┌─────────────────────────┐ │  │
│  │  │      Application        │ │  │  ← Lógica de negocio / casos de uso
│  │  │  ┌───────────────────┐  │ │  │
│  │  │  │      Domain       │  │ │  │  ← Núcleo: entidades y reglas
│  │  │  └───────────────────┘  │ │  │
│  │  └─────────────────────────┘ │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

**Regla de oro:** Domain no importa nada. Application solo importa Domain. Infrastructure e Api pueden importar Application.

---

## 2. Clean Architecture — Capas

| Proyecto | Carpeta | Responsabilidad |
|----------|---------|-----------------|
| `KPG.Timesheet.Domain` | `Backend/src/Domain/` | Entidades, enums, excepciones de dominio |
| `KPG.Timesheet.Application` | `Backend/src/Application/` | Casos de uso (Commands/Queries), interfaces, validaciones |
| `KPG.Timesheet.Infrastructure` | `Backend/src/Infrastructure/` | EF Core, Identity, SMTP, jobs, repositorios |
| `KPG.Timesheet.Api` | `Backend/src/Api/` | Endpoints HTTP, middleware, startup |

---

## 3. Capa Domain

**Carpeta:** `Backend/src/Domain/`

Es el núcleo del sistema. No tiene dependencias de ninguna otra capa ni de ningún framework. Contiene lo que el negocio *es*, no lo que el sistema *hace*.

### 3.1 Entidades

Representan las tablas principales de la base de datos y encapsulan las reglas que siempre deben cumplirse.

| Entidad | Descripción |
|---------|-------------|
| `RegistroHoras` | Jornada diaria de un colaborador. Contiene turnos AM/PM, cliente, proyecto, recurso, descripción y lugar. La restricción principal es que existe **un único registro por usuario por fecha**. |
| `SolicitudExcepcion` | Solicitud de un colaborador para registrar fuera de la ventana de retroactividad. Tiene estados: Pendiente, Aprobada, Rechazada. |
| `Empleado` | Catálogo de recursos humanos. Soft-delete con `IsActive`. |
| `Cliente` | Catálogo de empresas cliente. Soft-delete con `IsActive`. |
| `Proyecto` | Proyectos agrupados por cliente (FK → Cliente). Soft-delete con `IsActive`. |
| `Modalidad` | Catálogo de modalidades de trabajo (Presencial, Remoto, Híbrido). |
| `LugarTrabajo` | Catálogo de lugares físicos de trabajo. |
| `ParametroSistema` | Configuración global: ventana de retroactividad en días hábiles, umbral de notificaciones. |
| `RefreshToken` | Tokens de renovación JWT almacenados en base de datos. Single-use con rotación. |
| `BitacoraAuditoria` | Registro inmutable de eventos sensibles del sistema. |
| `NotificacionEnviada` | Historial de correos de recordatorio enviados. |

### 3.2 Clase Base

`BaseAuditableEntity` — todas las entidades heredan de esta clase y obtienen automáticamente:
- `Id` (int, PK)
- `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy` — llenados por el interceptor de EF Core.

### 3.3 Constantes y Enums

```
Constants/
  Roles.cs          → "Admin", "Gerente", "Supervisor", "Empleado"

Enums/
  EstadoSolicitud.cs → Pendiente | Aprobada | Rechazada
```

### 3.4 Excepciones de Dominio

`DomainRuleException` — se lanza cuando una regla de negocio del dominio se viola (p. ej. intentar registrar horas con salida antes que entrada).

---

## 4. Capa Application

**Carpeta:** `Backend/src/Application/`

Contiene todos los **casos de uso** del sistema. Orquesta las entidades del Domain usando los contratos (interfaces) que Infrastructure implementará. No sabe cómo se persisten los datos ni cómo se envían correos — solo sabe *qué* debe pasar.

### 4.1 Patrón CQRS con MediatR

Todas las operaciones se expresan como **Commands** (escriben/modifican estado) o **Queries** (solo leen). Cada una tiene su propio Handler.

```
Feature/
  Commands/
    CreateXxx/
      CreateXxxCommand.cs         ← El "mensaje" (datos de entrada)
      CreateXxxCommandValidator.cs ← Validación con FluentValidation
      CreateXxxCommandHandler.cs  ← La lógica real
  Queries/
    GetXxx/
      GetXxxQuery.cs
      GetXxxQueryHandler.cs
      XxxDto.cs                   ← DTO de respuesta
```

**¿Por qué CQRS?** Separa claramente la lectura de la escritura. Los Queries pueden usar Dapper para lecturas optimizadas; los Commands usan EF Core para escrituras con validaciones completas.

### 4.2 Features (Módulos de negocio)

| Feature | Commands | Queries |
|---------|----------|---------|
| **Auth** | Login, RefreshToken, Logout | — |
| **Users** | CreateUser, ActivateUser, DeactivateUser, DeleteUser, ChangeUserRole | GetUsers |
| **RegistroHoras** | CreateRegistroHoras, UpdateDescripcion, DeleteRegistroHoras | GetMisRegistros, GetRegistrosRecientes |
| **Catalogos** | Create/Update/Toggle para Clientes, Empleados, Proyectos, Modalidades, LugaresTrabajo | GetXxx de cada catálogo |
| **SolicitudesExcepcion** | CreateSolicitud, AprobarSolicitud, RechazarSolicitud | GetSolicitudesExcepcion |
| **Bitacora** | — | GetBitacora, ExportarBitacora |
| **Dashboard** | — | GetMetricasGlobales, GetPendientesCriticos |
| **Reportes** | — | GetReporteHoras, ExportarReporteHoras, ExportarTimesheet |
| **Notificaciones** | — | GetHistorialNotificaciones |
| **Sistema** | UpdateVentanaRetroactividad, UpdateUmbralNotificacion | GetVentanaRetroactividad, GetUmbralNotificacion |

### 4.3 Interfaces (Contratos)

Application define **qué necesita** sin saber **cómo se implementa**:

| Interfaz | Para qué sirve |
|----------|---------------|
| `IApplicationDbContext` | Acceso a las entidades de la base de datos |
| `IIdentityService` | Crear, activar, desactivar y eliminar usuarios |
| `IJwtTokenService` | Generar access token y refresh token |
| `IBitacoraService` | Registrar eventos de auditoría |
| `IEmailService` | Enviar correos electrónicos |
| `IClock` | Obtener la hora actual (inyectable, facilita tests) |
| `IUser` | Obtener el ID y roles del usuario autenticado actual |
| `IDashboardRepository` | Consultas optimizadas para el dashboard |
| `IReportesRepository` | Consultas para reportes tabulares |

### 4.4 Pipeline de MediatR (Behaviors)

Antes de que llegue al Handler, cada request pasa por una cadena de behaviors:

```
Request
  → LoggingBehaviour        (registra entrada y salida en logs)
  → PerformanceBehaviour    (mide tiempo, alerta si > 500ms)
  → ValidationBehaviour     (ejecuta FluentValidation, lanza 400 si falla)
  → AuthorizationBehaviour  (verifica atributos [Authorize] del command)
  → Handler                 (la lógica real)
```

Este pipeline garantiza que **ningún handler** necesita implementar logging, validación o autorización por su cuenta.

### 4.5 Validaciones con FluentValidation

Cada Command que modifica datos tiene su `Validator`. Ejemplo para `CreateRegistroHorasCommand`:
- FechaRegistro no puede ser futura
- Si hay Turno AM, HoraEntradaAM < HoraSalidaAM
- Cliente, Proyecto, Modalidad, Recurso y Descripción son requeridos
- La fecha debe estar dentro de la ventana de retroactividad (o tener excepción aprobada)

---

## 5. Capa Infrastructure

**Carpeta:** `Backend/src/Infrastructure/`

Implementa todos los contratos definidos en Application. Es la única capa que tiene dependencias de frameworks externos (EF Core, SQL Server, SMTP, etc.).

### 5.1 Acceso a Datos (`Data/`)

**`ApplicationDbContext.cs`**  
DbContext principal. Extiende `IdentityDbContext<ApplicationUser>` para integrar ASP.NET Core Identity con las entidades del dominio. Contiene DbSets para todas las entidades y aplica las configuraciones Fluent API.

**`ApplicationDbContextInitialiser.cs`**  
Se ejecuta al arrancar la aplicación en Development:
1. Aplica migraciones pendientes.
2. Si la base de datos está vacía, ejecuta el seeder con datos iniciales: roles, usuarios de prueba, clientes, proyectos, empleados y parámetros del sistema.

**`AuditableEntityInterceptor.cs`**  
Interceptor de EF Core que se ejecuta antes de cada `SaveChanges`. Rellena automáticamente `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy` en cualquier entidad que herede de `BaseAuditableEntity`, usando el usuario autenticado del `IUser` inyectado.

**Configuraciones Fluent API (`Configurations/`)**  
Cada entidad tiene su archivo de configuración donde se define:
- Nombre de tabla
- Restricciones de longitud
- Índices únicos (p. ej. `(UserId, FechaRegistro)` en `RegistroHoras`)
- Relaciones FK

### 5.2 Identity (`Identity/`)

| Archivo | Rol |
|---------|-----|
| `ApplicationUser.cs` | Extiende `IdentityUser` con `NombreCompleto` |
| `IdentityService.cs` | Implementa `IIdentityService` usando `UserManager<ApplicationUser>` |
| `JwtTokenService.cs` | Implementa `IJwtTokenService`. Genera JWT con claims de userId, email y roles. Los refresh tokens se persisten en `RefreshTokens` con rotación single-use. |

### 5.3 Repositorios de Lectura

Para las consultas complejas que alimentan el Dashboard y los Reportes se usa **Dapper** en lugar de EF Core. Dapper ejecuta SQL directo, lo que es más rápido para consultas de lectura con múltiples joins y agregaciones.

| Repositorio | Usa | Para qué |
|-------------|-----|---------|
| `DashboardRepository` | Dapper | Métricas del dashboard: totales, pendientes, tendencias |
| `ReportesRepository` | Dapper | Datos tabulares para reportes con filtros |
| `NotificacionesRepository` | Dapper | Historial de correos enviados |
| `BitacoraQueryRepository` | Dapper | Consultas filtradas de la bitácora de auditoría |

### 5.4 Servicios (`Services/` y otros)

| Servicio | Implementa | Descripción |
|---------|-----------|-------------|
| `BitacoraService` | `IBitacoraService` | Persiste entradas inmutables en `BitacoraAuditoria` |
| `SmtpEmailService` | `IEmailService` | Envío de correos via SMTP (MailKit/SmtpClient) |
| `SystemClock` | `IClock` | Devuelve `DateTimeOffset.UtcNow`. Reemplazable en tests. |
| `NotificacionesPendientesJob` | `IHostedService` | Background job que corre diariamente a las 8:00 AM. Busca colaboradores que superan el umbral de días sin registrar y les envía correo de recordatorio. |

### 5.5 Exportación de Reportes

| Handler | Librería | Genera |
|---------|----------|--------|
| `ExportarReporteHorasQueryHandler` | MiniExcel | Excel tabular con filtros aplicados |
| `ExportarTimesheetQueryHandler` | ClosedXML | Excel con formato oficial KPG (columnas EntradaAM, SalidaAM, EntradaPM, SalidaPM, etc.) |
| `ExportarBitacoraQueryHandler` | MiniExcel | Excel de la bitácora filtrada |

---

## 6. Capa Api

**Carpeta:** `Backend/src/Api/`

Es el punto de entrada HTTP. Recibe las peticiones, las convierte en Commands/Queries de MediatR, y devuelve la respuesta. No contiene lógica de negocio.

### 6.1 Minimal API con Grupos de Endpoints

Cada feature tiene su propio archivo que implementa la interfaz `IEndpointGroup`:

```csharp
public interface IEndpointGroup
{
    void Map(RouteGroupBuilder group);
}
```

Al iniciar, `EndpointRouteBuilderExtensions` descubre todos los `IEndpointGroup` por reflexión y los registra automáticamente. Esto evita un `Program.cs` gigante.

| Archivo | Prefijo de ruta | Endpoints principales |
|---------|----------------|----------------------|
| `Auth.cs` | `/api/auth` | POST /login, /refresh, /logout · GET /me |
| `RegistroHoras.cs` | `/api/registros` | GET lista · POST crear (upsert) · DELETE · GET recientes |
| `Users.cs` | `/api/users` | GET lista · POST crear · PUT rol · PATCH activo · DELETE |
| `Clientes.cs` | `/api/clientes` | CRUD completo + toggle activo |
| `Empleados.cs` | `/api/empleados` | CRUD completo + toggle activo |
| `Proyectos.cs` | `/api/proyectos` | CRUD por cliente + toggle activo |
| `Modalidades.cs` | `/api/modalidades` | CRUD + toggle activo |
| `LugaresTrabajo.cs` | `/api/lugares-trabajo` | CRUD + toggle activo |
| `SolicitudesExcepcion.cs` | `/api/solicitudes-excepcion` | GET · POST crear · PATCH aprobar/rechazar |
| `Dashboard.cs` | `/api/dashboard` | GET métricas globales · GET pendientes críticos |
| `Reportes.cs` | `/api/reportes` | GET datos · GET export Excel · GET timesheet |
| `Bitacora.cs` | `/api/bitacora` | GET filtrada · GET export Excel |
| `Notificaciones.cs` | `/api/notificaciones` | GET historial |
| `Sistema.cs` | `/api/sistema` | GET/PUT parámetros del sistema |

### 6.2 Manejo de Errores

`ProblemDetailsExceptionHandler` captura todas las excepciones no manejadas y las convierte en respuestas **RFC 7231 Problem Details**:

| Excepción | HTTP Status |
|-----------|-------------|
| `ValidationException` | 400 Bad Request (incluye errores por campo) |
| `NotFoundException` | 404 Not Found |
| `ForbiddenAccessException` | 403 Forbidden |
| `UnauthorizedAccessException` | 401 Unauthorized |
| Cualquier otra | 500 Internal Server Error |

### 6.3 Documentación OpenAPI

La API expone documentación interactiva con **Scalar** (`/scalar`) en Development. Los transformers registrados documentan automáticamente el esquema de seguridad Bearer y los posibles errores de cada endpoint.

---

## 7. Flujo de una Petición

Ejemplo: el empleado guarda su jornada del día.

```
Frontend (Blazor WASM)
  │  POST /api/registros  { fecha, horaEntradaAM, cliente, ... }
  ▼
[Api] RegistroHoras.cs endpoint
  │  → crea CreateRegistroHorasCommand
  │  → mediator.Send(command)
  ▼
[Application] Pipeline de MediatR
  │  → LoggingBehaviour       (log de entrada)
  │  → PerformanceBehaviour   (inicia cronómetro)
  │  → ValidationBehaviour    → CreateRegistroHorasCommandValidator
  │       Valida: fechas, horas, campos requeridos, ventana retroactividad
  │       Si falla → lanza ValidationException → 400 Bad Request
  │  → AuthorizationBehaviour (verifica JWT claim del rol)
  ▼
[Application] CreateRegistroHorasCommandHandler
  │  → busca si ya existe registro para (userId, fecha) via IApplicationDbContext
  │  → si existe: actualiza (upsert), si no: crea nuevo RegistroHoras
  │  → llama IBitacoraService.RegistrarEvento("RegistroCreado", ...)
  │  → await context.SaveChangesAsync()
  │       El AuditableEntityInterceptor rellena Created/CreatedBy
  ▼
[Infrastructure] EF Core → SQL Server
  │  INSERT o UPDATE en tabla RegistrosHoras
  ▼
[Api] Devuelve 200 OK con RegistroHorasDto
  ▼
Frontend recibe confirmación
```

---

## 8. Patrones Clave

### Soft Delete vs Hard Delete

| Entidad | Estrategia | Razón |
|---------|-----------|-------|
| Catálogos (Cliente, Empleado, Proyecto, Modalidad, LugarTrabajo) | **Soft delete** (`IsActive = false`) | Los registros históricos referencian estos valores; borrarlos rompería el historial |
| RegistroHoras | **Hard delete** (DELETE físico) | El empleado tiene derecho a borrar sus propios registros sin dejar rastro en la tabla principal (la bitácora sí lo registra) |
| Usuarios | **Soft delete** (`IsActive`) + posible hard delete si nunca registraron | Permite reactivar colaboradores; los registros de horas no se pierden |

### Upsert en RegistroHoras

El endpoint `POST /api/registros` hace **upsert**: si ya existe un registro para `(userId, fechaRegistro)`, lo actualiza en lugar de crear uno nuevo. Esto permite al empleado registrar el turno AM y más tarde agregar el turno PM sin duplicados.

### Repository Pattern en el Frontend

El frontend no hace llamadas HTTP directas a la API. Todo pasa por interfaces (`IRegistroHorasRepository`, `IClienteRepository`, etc.) que se resuelven con implementaciones HTTP concretas. Esto hace el frontend testeable y desacoplado de las URLs reales.

---

## 9. Base de Datos

### Tablas Principales

```
AspNetUsers          ← Usuarios del sistema (Identity)
AspNetRoles          ← Roles: Admin, Gerente, Supervisor, Empleado
AspNetUserRoles      ← Relación N:M usuario-rol

RegistrosHoras       ← Jornadas diarias (UNIQUE: UserId + FechaRegistro)
RefreshTokens        ← JWT refresh tokens (FK → AspNetUsers)
SolicitudesExcepcion ← Solicitudes de registro retroactivo

Empleados            ← Catálogo de recursos (soft-delete)
Clientes             ← Catálogo de clientes (soft-delete)
Proyectos            ← Catálogo de proyectos FK → Clientes (soft-delete)
Modalidades          ← Catálogo de modalidades (soft-delete)
LugaresTrabajo       ← Catálogo de lugares (soft-delete)

ParametrosSistema    ← Configuración: ventana retroactividad, umbral notif.
BitacoraAuditoria    ← Registro inmutable de eventos
NotificacionesEnviadas ← Historial de correos enviados
```

### Restricción Clave de RegistroHoras

```sql
CONSTRAINT UQ_RegistrosHoras_User_Fecha
    UNIQUE (UserId, FechaRegistro)
```

Un colaborador puede tener solo un registro por día. El upsert del backend se apoya en esta restricción.

---

## 10. Autenticación y Autorización

### Flujo JWT

```
1. POST /api/auth/login (email + password)
   → IdentityService verifica credenciales con UserManager
   → JwtTokenService genera:
       • Access Token  (JWT, 60 minutos, claims: userId, email, roles)
       • Refresh Token (GUID opaco, 8 horas, guardado en BD)
   → Respuesta: { accessToken, refreshToken }

2. Requests autenticados
   → Header: Authorization: Bearer <accessToken>
   → Middleware JWT valida firma, expiración y claims

3. Renovar token (POST /api/auth/refresh)
   → Valida refresh token en BD (single-use: se invalida al usarse)
   → Genera nuevo par de tokens
   → El token anterior queda marcado como usado

4. Logout (POST /api/auth/logout)
   → Invalida el refresh token en BD
```

### Roles y Autorización

Los endpoints usan `[Authorize(Roles = "...")]`. Los roles están jerárquicamente definidos:

| Rol | Acceso |
|----|--------|
| `Empleado` | Registrar y ver sus propias jornadas |
| `Supervisor` | Todo lo anterior + Dashboard equipo, Reportes, Bitácora equipo |
| `Gerente` | Todo lo anterior + Dashboard gerencial (por cliente/proyecto) |
| `Admin` | Acceso completo + gestión de usuarios, catálogos y parámetros |

---

## 11. Diagrama de Dependencias

```
Domain          ← No importa nada
   ↑
Application     ← Importa Domain
   ↑          ← Define interfaces (IApplicationDbContext, IEmailService, etc.)
Infrastructure  ← Importa Application + frameworks externos
   |               (EF Core, Identity, SMTP, Dapper, MiniExcel, ClosedXML)
   ↑
Api             ← Importa Application (MediatR.Send)
                ← Importa Infrastructure solo para DI (Program.cs)
                   (No usa Infrastructure directamente en endpoints)
```

**Regla que nunca se rompe:** los endpoints de Api solo llaman a `mediator.Send(command)`. Nunca instancian ni llaman directamente a repositorios, DbContext o servicios de Infrastructure.
