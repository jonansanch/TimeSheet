---
stepsCompleted: ["step-01-requirements-extracted", "step-02-epics-approved", "step-03-stories-created", "step-04-final-validation-complete"]
status: complete
completedAt: "2026-05-12"
inputDocuments:
  - "_bmad-output/planning-artifacts/prd.md"
  - "_bmad-output/planning-artifacts/architecture.md"
  - "_bmad-output/planning-artifacts/ux-design-specification.md"
  - "_bmad-output/planning-artifacts/ux-design-directions.html"
  - "_bmad-output/planning-artifacts/product-brief-Timesheet.md"
  - "Docs/Propuesta-TimeSheetV3.md"
---

# Timesheet - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for Timesheet, decomposing the requirements from the PRD, UX Design if it exists, and Architecture requirements into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: El Empleado puede registrar sus horas de entrada y salida para los turnos de maÃ±ana y tarde de un dÃ­a especÃ­fico.
FR2: El Empleado puede asociar cada registro a un cliente, proyecto, modalidad y recurso.
FR3: El Empleado puede ingresar descripciÃ³n de tarea y lugar de trabajo en cada registro.
FR4: El sistema valida los campos del formulario en tiempo real antes de permitir el guardado.
FR5: El Empleado puede consultar el historial de sus propios registros.
FR6: El Empleado puede eliminar sus propios registros.
FR7: El Empleado puede registrar horas con retroactividad de hasta 3 dÃ­as hÃ¡biles.
FR8: El Empleado puede solicitar autorizaciÃ³n para registrar fuera de la ventana, adjuntando justificaciÃ³n.
FR9: El Supervisor puede registrar y eliminar horas propias con las mismas capacidades del Empleado.
FR10: El Supervisor puede editar Ãºnicamente el campo descripciÃ³n de registros de su equipo.
FR11: El Supervisor puede eliminar registros de colaboradores bajo su supervisiÃ³n.
FR12: El Admin puede autorizar o rechazar solicitudes de registro fuera de ventana.
FR13: El Admin puede eliminar cualquier registro del sistema.
FR14: El Admin puede editar Ãºnicamente el campo descripciÃ³n de cualquier registro.
FR15: El sistema impide modificaciones a campos distintos a la descripciÃ³n una vez guardado el registro, salvo autorizaciÃ³n explÃ­cita del Admin.
FR16: El usuario puede iniciar sesiÃ³n con credenciales y recibir un JWT firmado.
FR17: El sistema renueva la sesiÃ³n activa mediante refresh token antes de su expiraciÃ³n.
FR18: El usuario puede cerrar sesiÃ³n revocando el token activo.
FR19: El sistema restringe acceso a funcionalidades y datos segÃºn el rol del usuario (Admin, Gerente, Supervisor, Empleado).
FR20: El Admin puede crear, activar, desactivar y eliminar cuentas de usuario.
FR21: El Admin puede asignar y modificar el rol de cualquier usuario.
FR22: El Admin puede crear, consultar, actualizar y desactivar registros de empleados.
FR23: El Admin puede crear, consultar, actualizar y desactivar registros de clientes.
FR24: El Admin puede crear, consultar, actualizar y desactivar proyectos asociados a un cliente.
FR25: El Admin puede crear, consultar, actualizar y desactivar parÃ¡metros del sistema (modalidades, recursos, lugares de trabajo).
FR26: El Admin puede configurar la ventana de registro retroactivo como parÃ¡metro del sistema.
FR27: El Supervisor puede generar un reporte de horas filtrado por perÃ­odo (dÃ­a, semana, mes o rango personalizado).
FR28: El Supervisor puede filtrar reportes por empleado, cliente o proyecto.
FR29: El Supervisor puede exportar reportes en Excel (.xlsx) y PDF.
FR30: El Gerente puede generar y exportar los mismos reportes disponibles para el Supervisor.
FR31: El Admin puede generar y exportar reportes para cualquier combinaciÃ³n de filtros.
FR32: El Supervisor puede visualizar el estado de registro del dÃ­a de cada miembro de su equipo (registrado / pendiente).
FR33: El Supervisor puede visualizar la distribuciÃ³n de horas de su equipo por consultor en el perÃ­odo seleccionado.
FR34: El Gerente puede visualizar la distribuciÃ³n de horas por cliente y por proyecto.
FR35: El Gerente puede visualizar indicadores de carga de trabajo por consultor.
FR36: El Admin puede visualizar mÃ©tricas globales del sistema (totales, distribuciones, tendencias).
FR37: El sistema registra en bitÃ¡cora: inicios de sesiÃ³n, cambios de rol, altas/bajas de usuarios, modificaciones autorizadas y aprobaciones de excepciones de ventana.
FR38: El Admin puede consultar y exportar la bitÃ¡cora con filtros por fecha, usuario y tipo de acciÃ³n.
FR39: El Supervisor puede consultar la bitÃ¡cora de acciones de su equipo.
FR40: El Gerente puede consultar la bitÃ¡cora del sistema.
FR41: El sistema identifica empleados o consultores con varios dÃ­as sin registrar horas y los marca como pendientes crÃ­ticos.
FR42: El sistema envÃ­a notificaciones por correo al empleado o consultor cuando acumula varios dÃ­as sin registrar horas.
FR43: El Supervisor y el Admin pueden consultar el historial de notificaciones enviadas por registros pendientes.

### NonFunctional Requirements

NFR1: Carga inicial de la aplicaciÃ³n (bundle Blazor WASM) menor o igual a 5 segundos en red interna.
NFR2: Guardado del formulario de registro completado en menos de 2 segundos.
NFR3: GeneraciÃ³n de cualquier reporte completada en menos de 10 segundos independientemente del rango.
NFR4: Carga del dashboard menor o igual a 3 segundos.
NFR5: Sistema soporta 30 usuarios concurrentes sin degradaciÃ³n perceptible del rendimiento.
NFR6: Toda comunicaciÃ³n cliente-servidor sobre HTTPS.
NFR7: Tokens JWT almacenados en memoria del cliente, no en localStorage ni sessionStorage.
NFR8: JWT expira en mÃ¡ximo 60 minutos; refresh token no excede 8 horas.
NFR9: Cada endpoint de API verifica rol del usuario antes de procesar, independientemente de restricciones en frontend.
NFR10: ContraseÃ±as almacenadas con hash bcrypt o argon2, nunca en texto plano.
NFR11: CORS restringido a orÃ­genes internos autorizados.
NFR12: Toda entrada validada y sanitizada en backend, independientemente de la validaciÃ³n frontend.
NFR13: Disponibilidad mayor o igual a 99% en horario laboral (lunes a viernes, 7am-8pm).
NFR14: Respaldos automÃ¡ticos diarios de base de datos; recuperaciÃ³n ante falla en menor o igual a 4 horas.
NFR15: PÃ©rdida de conexiÃ³n durante llenado del formulario no resulta en pÃ©rdida de datos ya guardados.
NFR16: Errores internos registrados en log con contexto de diagnÃ³stico, sin exponer informaciÃ³n sensible al usuario final.
NFR17: Arquitectura soporta crecimiento hasta 100 usuarios sin cambios estructurales en backend ni base de datos.
NFR18: Consultas de reportes y dashboard con Ã­ndices optimizados que no degraden con crecimiento del volumen de registros.
NFR19: Formulario de registro completable mediante teclado sin requerir mouse.
NFR20: Mensajes de error de validaciÃ³n indican exactamente quÃ© campo requiere correcciÃ³n y por quÃ©.
NFR21: La V1 opera en EspaÃ±ol como Ãºnico idioma soportado; la interfaz, validaciones y mensajes visibles deben mantenerse consistentes en EspaÃ±ol, dejando documentada la estrategia para incorporar InglÃ©s como idioma futuro sin rediseÃ±ar flujos crÃ­ticos.

### Additional Requirements

- Inicializar el proyecto como primera historia con soluciÃ³n `Backend/KPG.Timesheet.sln`, estructura Clean Architecture y Blazor WebAssembly.
- Usar scaffolding hÃ­brido: Jason Taylor CleanArchitecture para backend y `dotnet new blazorwasm` para frontend.
- Crear cinco proyectos principales: Domain, Application, Infrastructure, Api y WebUI.
- Usar .NET 10, C# 13, SQL Server 2019+, Entity Framework Core 10.x para escritura y Dapper 2.x para lecturas de reportes/dashboard.
- Agregar paquetes base: MediatR, FluentValidation, AutoMapper, Dapper, MiniExcel, QuestPDF, MudBlazor y Blazor.ApexCharts.
- Implementar ASP.NET Core Identity, JWT stateless, refresh token revocable en tabla `RefreshTokens` y `KpgAuthStateProvider` custom en Blazor.
- Almacenar JWT exclusivamente en memoria del cliente; el refresh token permite reconstruir sesiÃ³n al recargar si no estÃ¡ expirado o revocado.
- Implementar RBAC con roles fijos Admin, Gerente, Supervisor y Empleado en API y UI.
- Configurar todos los endpoints protegidos con `[Authorize(Roles = "...")]`; nunca depender solo de ocultar navegaciÃ³n en frontend.
- Implementar errores con Problem Details RFC 9457 y `ValidationProblemDetails` para validaciones de formulario.
- Exponer documentaciÃ³n de API con Scalar + OpenAPI nativo de .NET 10.
- Usar controllers REST delgados que delegan a MediatR; no incluir reglas de negocio en controllers.
- Organizar Application por vertical slices bajo `Features`, con Command, Handler, Validator y DTO en la misma carpeta de caso de uso.
- Usar excepciones de dominio tipadas para errores esperados; no retornar errores en payloads de Ã©xito.
- Modelar tablas crÃ­ticas: `RegistrosHoras`, `SolicitudesExcepcion`, `RefreshTokens`, `BitacoraAuditoria`, `ParametrosSistema`, `Empleados`, `Clientes` y `Proyectos`.
- Implementar `RegistroHoras` como entidad con inmutabilidad de campos post-guardado, excepto `Descripcion` bajo reglas de rol/autorizaciÃ³n.
- Implementar `SolicitudExcepcion` con ciclo de vida Pendiente, Aprobada y Rechazada.
- Implementar `BitacoraAuditoria` append-only con tipo de evento, usuario, entidad afectada, timestamp y metadata JSON.
- Implementar `ParametroSistema` para ventana de retroactividad y otros parÃ¡metros configurables; no hardcodear la ventana de 3 dÃ­as.
- Usar soft delete en catÃ¡logos para preservar integridad referencial histÃ³rica.
- Crear Ã­ndices SQL para columnas de filtro frecuente: fecha, empleado, cliente, proyecto y tipo de evento de auditorÃ­a.
- Usar MiniExcel para exportaciÃ³n Excel y QuestPDF para exportaciÃ³n PDF, manteniendo ambas implementaciones en `Infrastructure/Reports`.
- Usar Serilog 4.x con logging estructurado y sink a archivo con rotaciÃ³n diaria en producciÃ³n.
- Habilitar compresiÃ³n Brotli/gzip en IIS para cumplir la carga inicial de Blazor WASM.
- Desplegar API en IIS + Kestrel y WebUI como sitio estÃ¡tico IIS separado.
- Mantener secretos de producciÃ³n fuera del repositorio en configuraciÃ³n del servidor.
- Implementar respaldos diarios con SQL Server Agent y recuperaciÃ³n documentada en mÃ¡ximo 4 horas.
- Preparar build con `dotnet build Backend/KPG.Timesheet.sln` y pruebas con `dotnet test Backend/KPG.Timesheet.sln`.
- Crear estructura de pruebas: Domain.UnitTests, Application.UnitTests, Infrastructure.IntegrationTests, Api.IntegrationTests y WebUI.ComponentTests.
- Resolver explÃ­citamente el estado del proyecto `Fronted/` Angular existente como prototipo legacy antes de implementar historias frontend.
- Mantener V1 sin integraciones externas, sin mÃ³dulo de vacaciones, sin notificaciones automÃ¡ticas, sin acceso mÃ³vil y sin cÃ¡lculo automÃ¡tico de nÃ³mina/facturaciÃ³n.
- Arrancar sin migraciÃ³n de histÃ³ricos desde Excel segÃºn PRD; los Excel anteriores al go-live quedan fuera del sistema V1.
- Preparar checklist de go-live con QA funcional/integraciÃ³n, UAT de negocio, accesos, roles, backups, monitoreo bÃ¡sico, ventana de reversiÃ³n e hiper care.
- Implementar envÃ­o de correo para alertas de registros pendientes con configuraciÃ³n SMTP o proveedor interno definido por infraestructura.
- Configurar como parÃ¡metro del sistema la cantidad de dÃ­as sin registro que dispara notificaciÃ³n por correo.
- Registrar cada notificaciÃ³n enviada con destinatario, fecha/hora, motivo, resultado de envÃ­o y usuario/equipo asociado.

### UX Design Requirements

UX-DR1: Implementar una pantalla de carga inicial Blazor WASM con identidad KPG, barra de progreso y texto "Cargando KPG Timesheet..." para comunicar la demora inicial.
UX-DR2: Implementar layout desktop-first con sidebar fijo de 240px, top app bar de 64px y contenido centrado con ancho mÃ¡ximo de 1200px.
UX-DR3: Soportar resoluciones desktop de 1280px, 1440px y 1920px; por debajo de 1280px la experiencia puede degradarse y debe comunicarse antes del go-live.
UX-DR4: Configurar tema MudBlazor con tokens KPG: Primary `#0D3B5E`, Secondary `#5BB8D4`, Warning `#FFD300`, Accent `#F57C20`, Success `#2E7D32`, Error `#C62828`, Background `#F5F5F5`.
UX-DR5: Usar Poppins para tÃ­tulos y Roboto para cuerpo, siguiendo tamaÃ±os definidos: 24px page title, 20px card title, 16px body, 14px secondary y 12px caption.
UX-DR6: Implementar navegaciÃ³n lateral por rol; los Ã­tems no permitidos para el rol no aparecen y el Ã­tem activo usa indicador visual lateral.
UX-DR7: Implementar formulario de registro con fecha preseleccionada en hoy y turno AM activo por defecto.
UX-DR8: Implementar `KpgDatePicker` como wrapper de MudDatePicker con semÃ¡foro visual de fechas: disponible, retroactiva en ventana y fuera de ventana.
UX-DR9: `KpgDatePicker` debe exponer `WindowDays`, `DisabledDates`, `OnDateSelected(date, DateAvailability)` y `OnExceptionRequested(date)`.
UX-DR10: `KpgDatePicker` debe incluir `aria-label` por celda indicando disponibilidad y navegaciÃ³n por teclado con flechas.
UX-DR11: Implementar `KpgShiftForm` para la experiencia central AM/PM con estados Empty, InProgress y Saved.
UX-DR12: `KpgShiftForm` debe usar estructura AM/PM por tabs o tarjetas, focus automÃ¡tico en Entrada al activar turno y avance lÃ³gico con Tab.
UX-DR13: Al guardar AM, el formulario PM debe activarse automÃ¡ticamente y ofrecer acciÃ³n "Copiar configuraciÃ³n del AM".
UX-DR14: Al completar ambos turnos, la pantalla debe mostrar "Jornada completa" con total de horas registradas.
UX-DR15: Implementar `KpgRecentSuggestions` con las Ãºltimas 3-5 combinaciones cliente/proyecto usadas por el usuario autenticado.
UX-DR16: `KpgRecentSuggestions` debe permitir prellenar cliente/proyecto con clic o teclado y usar fuente `GET /api/registros/recientes?userId=X&top=5` o endpoint equivalente.
UX-DR17: Usar MudAutocomplete para cliente y proyecto; nunca usar select nativo con mÃ¡s de 10 Ã­tems.
UX-DR18: Filtrar proyectos por cliente seleccionado.
UX-DR19: Usar chips para modalidad y asegurar comportamiento accesible tipo radiogroup.
UX-DR20: Validar campos en `onBlur`, no mientras el usuario escribe; el botÃ³n Guardar se habilita solo con campos requeridos vÃ¡lidos.
UX-DR21: Los errores de validaciÃ³n deben aparecer bajo el campo especÃ­fico, en una lÃ­nea y asociados con `aria-describedby`.
UX-DR22: Implementar `KpgSaveConfirmationBanner` inline, no modal, con resumen del registro guardado, `role="status"` y `aria-live="polite"`.
UX-DR23: El banner de confirmaciÃ³n se auto-oculta a los 8 segundos o al iniciar la siguiente acciÃ³n.
UX-DR24: Implementar `KpgExceptionDialog` para fechas fuera de ventana con tono de ayuda, header Ã¡mbar, justificaciÃ³n requerida y acciones Cancelar/Enviar solicitud.
UX-DR25: `KpgExceptionDialog` debe enfocar el textarea al abrir y devolver focus a la fecha que lo activÃ³ al cerrar.
UX-DR26: DespuÃ©s de enviar solicitud de excepciÃ³n, mostrar confirmaciÃ³n inline de solicitud enviada.
UX-DR27: Implementar vista Admin de solicitudes de excepciÃ³n pendientes con datos de solicitante, fecha, justificaciÃ³n y acciones Aprobar/Rechazar.
UX-DR28: Implementar dashboard de Supervisor con el nÃºmero de pendientes de registro como mÃ©trica principal visible sin scroll.
UX-DR29: Implementar `KpgTeamStatusCard` con avatar de iniciales, nombre, horas del dÃ­a, badge de estado y estados Complete, Partial, Pending.
UX-DR30: Las tarjetas de equipo pendientes deben permitir expandir detalle de Ãºltimos registros del colaborador.
UX-DR31: Implementar `KpgStatCard` para mÃ©tricas de dashboard con variantes Default, Alert y Success.
UX-DR32: Integrar ApexCharts for Blazor para distribuciÃ³n de horas donde aplique.
UX-DR33: Usar MudDataGrid compacto para reportes, historial y bitÃ¡cora, con ordenamiento por fecha/nombre/horas.
UX-DR34: Configurar paginaciÃ³n de tablas en 25 filas por defecto con selector 25/50/100.
UX-DR35: Ubicar acciones Exportar Excel y Exportar PDF en el header de tablas/reportes.
UX-DR36: Formatear fechas como `dd/MM/yyyy` y horas como `HH:mm` en UI.
UX-DR37: Implementar estados vacÃ­os especÃ­ficos para Mis Registros, Reportes sin resultados y BitÃ¡cora sin eventos.
UX-DR38: Implementar loading de datos con MudProgressCircular y skeleton rows en tablas.
UX-DR39: Implementar guardado en progreso con spinner inline, texto "Guardando..." y botÃ³n deshabilitado.
UX-DR40: Implementar confirmaciÃ³n de eliminaciÃ³n con modal especÃ­fico y botÃ³n destructivo; no usar modales para Ã©xito o alertas informativas.
UX-DR41: Cumplir WCAG 2.1 AA con navegaciÃ³n completa por teclado, focus visible, HTML semÃ¡ntico y Ã¡reas interactivas mÃ­nimas de 44x44px.
UX-DR42: Verificar accesibilidad con Axe DevTools, navegaciÃ³n solo teclado y NVDA para confirmaciones `aria-live`.
UX-DR43: Probar flujos J1-J4 en Chrome, Edge y Firefox antes de go-live.
UX-DR44: Elegir una direcciÃ³n de navegaciÃ³n visual entre A (sidebar navy), B (sidebar blanco) o C (sidebar icÃ³nico compacto) antes de implementar el shell definitivo.
UX-DR45: Si se elige sidebar icÃ³nico compacto, agregar tooltips para mitigar la curva de aprendizaje.
UX-DR46: Implementar login corporativo simple con identidad KPG, usuario, contraseÃ±a, botÃ³n Iniciar sesiÃ³n y texto de recuperaciÃ³n orientado a contactar al Admin.
UX-DR47: Definir polÃ­tica de idioma V1 en EspaÃ±ol, revisar que los textos visibles no mezclen EspaÃ±ol/InglÃ©s innecesariamente, y documentar una ruta tÃ©cnica para habilitar InglÃ©s en una versiÃ³n futura.

### FR Coverage Map

FR1: Epic 2 - Registro de horas AM/PM por empleado.
FR2: Epic 2 - AsociaciÃ³n de registros a cliente, proyecto, modalidad y recurso.
FR3: Epic 2 - Captura de descripciÃ³n de tarea y lugar de trabajo.
FR4: Epic 2 - Validaciones en tiempo real del formulario de registro.
FR5: Epic 2 - Consulta de historial personal de registros.
FR6: Epic 2 - EliminaciÃ³n de registros propios del empleado.
FR7: Epic 3 - Registro retroactivo dentro de ventana permitida.
FR8: Epic 3 - Solicitud de autorizaciÃ³n para registro fuera de ventana.
FR9: Epic 2 - Capacidades de registro y eliminaciÃ³n propias para Supervisor.
FR10: Epic 3 - EdiciÃ³n limitada de descripciÃ³n en registros del equipo por Supervisor.
FR11: Epic 3 - EliminaciÃ³n de registros del equipo por Supervisor.
FR12: Epic 3 - AprobaciÃ³n o rechazo Admin de solicitudes fuera de ventana.
FR13: Epic 3 - EliminaciÃ³n Admin de cualquier registro.
FR14: Epic 3 - EdiciÃ³n Admin limitada a descripciÃ³n de cualquier registro.
FR15: Epic 3 - Inmutabilidad de campos post-guardado salvo autorizaciÃ³n explÃ­cita.
FR16: Epic 1 - Inicio de sesiÃ³n con credenciales y JWT firmado.
FR17: Epic 1 - RenovaciÃ³n de sesiÃ³n mediante refresh token.
FR18: Epic 1 - Cierre de sesiÃ³n con revocaciÃ³n de token activo.
FR19: Epic 1 - RestricciÃ³n de funcionalidades y datos por rol.
FR20: Epic 4 - CreaciÃ³n, activaciÃ³n, desactivaciÃ³n y eliminaciÃ³n de cuentas.
FR21: Epic 4 - AsignaciÃ³n y modificaciÃ³n de roles de usuarios.
FR22: Epic 4 - CRUD/desactivaciÃ³n de empleados.
FR23: Epic 4 - CRUD/desactivaciÃ³n de clientes.
FR24: Epic 4 - CRUD/desactivaciÃ³n de proyectos asociados a clientes.
FR25: Epic 4 - CRUD/desactivaciÃ³n de parÃ¡metros del sistema.
FR26: Epic 4 - ConfiguraciÃ³n Admin de ventana de registro retroactivo.
FR27: Epic 5 - Reporte de horas filtrado por perÃ­odo para Supervisor.
FR28: Epic 5 - Filtros de reporte por empleado, cliente o proyecto.
FR29: Epic 5 - ExportaciÃ³n de reportes en Excel y PDF para Supervisor.
FR30: Epic 5 - Reportes y exportaciÃ³n para Gerente.
FR31: Epic 5 - Reportes y exportaciÃ³n global para Admin.
FR32: Epic 5 - Estado diario de registro del equipo para Supervisor.
FR33: Epic 5 - DistribuciÃ³n de horas del equipo por consultor.
FR34: Epic 5 - DistribuciÃ³n de horas por cliente y proyecto para Gerente.
FR35: Epic 5 - Indicadores de carga por consultor para Gerente.
FR36: Epic 5 - MÃ©tricas globales del sistema para Admin.
FR37: Epic 6 - Registro de eventos sensibles en bitÃ¡cora.
FR38: Epic 6 - Consulta y exportaciÃ³n Admin de bitÃ¡cora con filtros.
FR39: Epic 6 - Consulta de bitÃ¡cora del equipo por Supervisor.
FR40: Epic 6 - Consulta de bitÃ¡cora del sistema por Gerente.
FR41: Epic 5 - IdentificaciÃ³n de pendientes crÃ­ticos por varios dÃ­as sin registro.
FR42: Epic 5 - EnvÃ­o de correos automÃ¡ticos por registros pendientes acumulados.
FR43: Epic 5 - Consulta de historial de notificaciones por Supervisor y Admin.

## Epic List

### Epic 1: Plataforma Base, Acceso Seguro y Shell por Rol

Los usuarios pueden entrar al sistema, mantener sesiÃ³n segura, ver solo las secciones permitidas por su rol y usar una base tÃ©cnica estable para el resto de flujos.

**FRs covered:** FR16, FR17, FR18, FR19

**Implementation Notes:** Incluye scaffolding Clean Architecture + Blazor WebAssembly, ASP.NET Core Identity, JWT, refresh token, RBAC, shell MudBlazor, navegaciÃ³n por rol, tema KPG, loading inicial y resoluciÃ³n explÃ­cita del proyecto `Fronted/` Angular como prototipo legacy.

### Epic 2: Registro Diario de Horas y Historial Personal

Empleados y supervisores pueden registrar, validar, consultar y eliminar sus propias horas AM/PM con una experiencia rÃ¡pida, clara y accesible.

**FRs covered:** FR1, FR2, FR3, FR4, FR5, FR6, FR9

**Implementation Notes:** Concentra la experiencia central: `KpgShiftForm`, `KpgDatePicker`, sugerencias recientes, confirmaciÃ³n de guardado, validaciones, accesibilidad por teclado, historial personal y eliminaciÃ³n de registros propios.

### Epic 3: Control de Retroactividad, Excepciones e Inmutabilidad

Los usuarios pueden manejar registros tardÃ­os con autorizaciÃ³n, y la organizaciÃ³n conserva control sobre quÃ© se puede modificar despuÃ©s de guardar.

**FRs covered:** FR7, FR8, FR10, FR11, FR12, FR13, FR14, FR15

**Implementation Notes:** Incluye ventana configurable, solicitudes de excepciÃ³n, aprobaciÃ³n/rechazo Admin, ediciÃ³n limitada de descripciÃ³n, eliminaciÃ³n por rol y reglas de dominio de inmutabilidad.

### Epic 4: AdministraciÃ³n Operativa de Usuarios y CatÃ¡logos

El Admin puede mantener usuarios, roles, empleados, clientes, proyectos y parÃ¡metros sin tocar la base de datos directamente.

**FRs covered:** FR20, FR21, FR22, FR23, FR24, FR25, FR26

**Implementation Notes:** Agrupa administraciÃ³n porque comparte pantallas, patrones CRUD, soft delete, validaciones, permisos y auditorÃ­a de acciones sensibles.

### Epic 5: Dashboard, Reportes y ExportaciÃ³n para Cierre Mensual

Supervisores, gerentes y admins pueden monitorear estado/carga del equipo y generar reportes exportables para facturaciÃ³n.

**FRs covered:** FR27, FR28, FR29, FR30, FR31, FR32, FR33, FR34, FR35, FR36, FR41, FR42, FR43

**Implementation Notes:** Cubre dashboard operativo, mÃ©tricas por rol, Dapper para lecturas, ApexCharts, filtros, exportaciÃ³n Excel/PDF, notificaciones por correo para registros pendientes y requisitos de rendimiento para dashboard/reportes.

### Epic 6: AuditorÃ­a, Trazabilidad y PreparaciÃ³n de Go-Live

La organizaciÃ³n puede consultar acciones sensibles, exportar bitÃ¡coras y operar con trazabilidad suficiente para auditorÃ­as y salida a producciÃ³n.

**FRs covered:** FR37, FR38, FR39, FR40

**Implementation Notes:** Incluye bitÃ¡cora append-only, filtros por rol, exportaciÃ³n, logging, backups, checklist de despliegue, QA/UAT, criterios de go-live e hiper care.

## Epic 1: Plataforma Base, Acceso Seguro y Shell por Rol

Los usuarios pueden entrar al sistema, mantener sesiÃ³n segura, ver solo las secciones permitidas por su rol y usar una base tÃ©cnica estable para el resto de flujos.

### Story 1.1: Inicializar SoluciÃ³n Full-Stack KPG Timesheet

**Requirements:** Arquitectura base, UX-DR1, UX-DR44

As a desarrollador,
I want una soluciÃ³n .NET 10 con Clean Architecture y Blazor WebAssembly inicializada,
So that el equipo pueda construir todos los mÃ³dulos sobre una estructura consistente y compilable.

**Acceptance Criteria:**

**Given** el repositorio Timesheet sin soluciÃ³n backend definitiva
**When** se inicializa la soluciÃ³n `Backend/KPG.Timesheet.sln`
**Then** existen proyectos Domain, Application, Infrastructure, Api y WebUI
**And** la soluciÃ³n compila con `dotnet build`

**Given** la arquitectura aprobada
**When** se agregan dependencias base
**Then** quedan disponibles MediatR, FluentValidation, AutoMapper, EF Core, Dapper, MudBlazor, ApexCharts, MiniExcel y QuestPDF segÃºn corresponda
**And** no se crean tablas o entidades de mÃ³dulos futuros todavÃ­a

**Given** existe un proyecto Angular `Fronted/`
**When** se documenta el estado del frontend oficial
**Then** `Fronted/` queda marcado como prototipo legacy
**And** Blazor WebAssembly queda definido como frontend V1

### Story 1.2: AutenticaciÃ³n con Login y JWT

**Requirements:** FR16, NFR6, NFR10, NFR12, UX-DR46

As a usuario interno,
I want iniciar sesiÃ³n con mis credenciales,
So that pueda acceder de forma segura a KPG Timesheet.

**Acceptance Criteria:**

**Given** un usuario activo con credenciales vÃ¡lidas
**When** envÃ­a usuario y contraseÃ±a desde la pantalla de login
**Then** el API valida credenciales con ASP.NET Core Identity
**And** retorna un JWT firmado y un refresh token vÃ¡lido

**Given** credenciales invÃ¡lidas o usuario inactivo
**When** el usuario intenta iniciar sesiÃ³n
**Then** el sistema rechaza el acceso con Problem Details
**And** no expone informaciÃ³n sensible sobre la causa exacta

**Given** la pantalla de login corporativo
**When** se muestra al usuario
**Then** incluye identidad KPG, usuario, contraseÃ±a, botÃ³n Iniciar sesiÃ³n y texto de recuperaciÃ³n orientado a contactar al Admin

### Story 1.3: SesiÃ³n Segura con Refresh Token y Logout

**Requirements:** FR17, FR18, NFR7, NFR8

As a usuario autenticado,
I want que mi sesiÃ³n se renueve de forma segura y pueda cerrarla explÃ­citamente,
So that no pierda trabajo durante el uso normal y pueda terminar mi sesiÃ³n cuando corresponda.

**Acceptance Criteria:**

**Given** un usuario autenticado con refresh token vigente
**When** el JWT estÃ¡ por expirar o la app recarga
**Then** el sistema solicita renovaciÃ³n al endpoint de refresh
**And** recibe un nuevo JWT si el refresh token no estÃ¡ expirado ni revocado

**Given** un usuario autenticado
**When** cierra sesiÃ³n
**Then** el refresh token activo queda revocado en servidor
**And** el JWT se elimina de memoria del cliente

**Given** la polÃ­tica de seguridad aprobada
**When** el cliente almacena tokens
**Then** el JWT vive exclusivamente en memoria
**And** no se guarda en localStorage ni sessionStorage

### Story 1.4: AutorizaciÃ³n por Rol en API y UI

**Requirements:** FR19, NFR9, NFR11, NFR12, UX-DR6

As a usuario de KPG,
I want ver y ejecutar solo las funcionalidades permitidas por mi rol,
So that la informaciÃ³n y las acciones sensibles queden protegidas.

**Acceptance Criteria:**

**Given** un usuario con rol Empleado, Supervisor, Gerente o Admin
**When** solicita un endpoint protegido
**Then** el API verifica el rol con autorizaciÃ³n server-side
**And** rechaza accesos no permitidos aunque el frontend haya sido manipulado

**Given** un usuario autenticado
**When** se renderiza la navegaciÃ³n principal
**Then** solo aparecen las secciones permitidas para su rol
**And** las secciones no permitidas no se muestran deshabilitadas

**Given** endpoints protegidos
**When** se revisan controllers/actions de la Ã‰pica 1
**Then** usan autorizaciÃ³n por rol donde aplique
**And** los errores siguen formato Problem Details

### Story 1.5: Shell Visual KPG y Tema Base

**Requirements:** UX-DR2, UX-DR3, UX-DR4, UX-DR5, UX-DR6, UX-DR44, UX-DR45

As a usuario autenticado,
I want una interfaz base clara y consistente con mi rol,
So that pueda orientarme rÃ¡pidamente en la plataforma.

**Acceptance Criteria:**

**Given** un usuario autenticado
**When** entra a la aplicaciÃ³n
**Then** ve un layout desktop-first con sidebar fijo, top app bar y contenido principal
**And** el Ã­tem activo se distingue visualmente

**Given** el tema KPG aprobado
**When** la app renderiza componentes MudBlazor
**Then** usa tokens de color KPG, Poppins para tÃ­tulos y Roboto para cuerpo
**And** conserva contraste compatible con WCAG 2.1 AA

**Given** las direcciones UX A, B y C
**When** se implementa el shell definitivo
**Then** se usa la direcciÃ³n visual aprobada
**And** si se elige sidebar icÃ³nico compacto, incluye tooltips

### Story 1.6: Experiencia de Carga Inicial Blazor WASM

**Requirements:** NFR1, UX-DR1

As a usuario interno,
I want ver feedback claro durante la carga inicial,
So that no perciba la aplicaciÃ³n como rota mientras descarga el runtime.

**Acceptance Criteria:**

**Given** la primera carga de Blazor WebAssembly
**When** la app descarga recursos iniciales
**Then** se muestra pantalla de carga con identidad KPG, barra de progreso y texto "Cargando KPG Timesheet..."
**And** la experiencia comunica honestamente la demora inicial

**Given** la app publicada en entorno IIS
**When** se sirven assets Blazor WASM
**Then** Brotli/gzip estÃ¡ habilitado para reducir el bundle
**And** la carga inicial objetivo es menor o igual a 5 segundos en red interna

## Epic 2: Registro Diario de Horas y Historial Personal

Empleados y supervisores pueden registrar, validar, consultar y eliminar sus propias horas AM/PM con una experiencia rÃ¡pida, clara y accesible.

### Story 2.1: Registrar Turno AM/PM con ValidaciÃ³n

**Requirements:** FR1, FR2, FR3, FR4, NFR2, UX-DR7, UX-DR11, UX-DR20

As a empleado,
I want registrar mis horas de entrada y salida por turno AM/PM,
So that mis horas diarias queden guardadas de forma precisa.

**Acceptance Criteria:**

**Given** un empleado autenticado
**When** abre la pantalla de registro
**Then** la fecha de hoy aparece preseleccionada
**And** el turno AM aparece activo por defecto

**Given** el formulario de turno AM o PM
**When** el empleado ingresa hora de entrada, hora de salida, cliente, proyecto, modalidad, recurso, descripciÃ³n y lugar
**Then** el sistema valida campos requeridos antes de guardar
**And** no permite guardar si hay errores de validaciÃ³n

**Given** un formulario vÃ¡lido
**When** el empleado guarda el turno
**Then** el registro queda asociado al usuario autenticado
**And** el guardado responde en menos de 2 segundos en condiciones normales de red interna

### Story 2.2: Seleccionar Cliente, Proyecto y Contexto con Sugerencias Recientes

**Requirements:** FR2, UX-DR15, UX-DR16, UX-DR17, UX-DR18, UX-DR19

As a empleado,
I want seleccionar rÃ¡pidamente cliente, proyecto y contexto de trabajo,
So that pueda completar mi registro diario en menos de 2 minutos.

**Acceptance Criteria:**

**Given** un empleado con registros previos
**When** abre el formulario de registro
**Then** ve las Ãºltimas 3-5 combinaciones cliente/proyecto usadas
**And** puede aplicar una sugerencia con clic o teclado

**Given** un cliente seleccionado
**When** el empleado abre el selector de proyecto
**Then** solo se muestran proyectos asociados a ese cliente
**And** el selector permite bÃºsqueda integrada

**Given** el campo modalidad
**When** el empleado navega por las opciones
**Then** puede seleccionar modalidad mediante chips accesibles
**And** los chips funcionan con teclado como grupo de opciÃ³n Ãºnica

### Story 2.3: ConfirmaciÃ³n de Guardado y Flujo AM a PM

**Requirements:** FR1, UX-DR13, UX-DR14, UX-DR22, UX-DR23

As a empleado,
I want recibir confirmaciÃ³n clara despuÃ©s de guardar cada turno,
So that tenga certeza de que mis horas quedaron registradas correctamente.

**Acceptance Criteria:**

**Given** un turno guardado exitosamente
**When** el sistema confirma la operaciÃ³n
**Then** muestra un banner inline con turno, fecha, horas, cliente y proyecto
**And** el banner usa `role="status"` y `aria-live="polite"`

**Given** el turno AM guardado
**When** la confirmaciÃ³n se muestra
**Then** el turno PM se activa automÃ¡ticamente
**And** aparece la opciÃ³n de copiar configuraciÃ³n del AM

**Given** ambos turnos guardados
**When** el empleado vuelve a la pantalla de registro del dÃ­a
**Then** la pantalla muestra "Jornada completa"
**And** muestra el total de horas registradas

### Story 2.4: Historial Personal de Registros

**Requirements:** FR5, UX-DR33, UX-DR34, UX-DR36, UX-DR37

As a empleado,
I want consultar mi historial de registros,
So that pueda revisar quÃ© horas he reportado.

**Acceptance Criteria:**

**Given** un empleado autenticado
**When** abre "Mis Registros"
**Then** ve Ãºnicamente sus propios registros
**And** los registros muestran fecha, turno, horas, cliente, proyecto, modalidad y descripciÃ³n

**Given** una lista con mÃºltiples registros
**When** el empleado ordena o filtra por fecha
**Then** la tabla actualiza los resultados correctamente
**And** mantiene formato `dd/MM/yyyy` para fechas y `HH:mm` para horas

**Given** el empleado no tiene registros
**When** abre "Mis Registros"
**Then** ve un estado vacÃ­o especÃ­fico
**And** puede iniciar registro de horas desde esa vista

### Story 2.5: Eliminar Registro Propio

**Requirements:** FR6, NFR9, UX-DR40

As a empleado,
I want eliminar un registro propio cuando corresponda,
So that pueda corregir capturas equivocadas dentro de mis permisos.

**Acceptance Criteria:**

**Given** un empleado viendo su historial
**When** selecciona eliminar un registro propio
**Then** el sistema muestra una confirmaciÃ³n especÃ­fica del registro
**And** la acciÃ³n destructiva aparece claramente diferenciada

**Given** el empleado confirma la eliminaciÃ³n
**When** el registro pertenece al usuario autenticado
**Then** el registro queda eliminado segÃºn la regla de dominio definida
**And** deja de aparecer en el historial personal

**Given** un empleado intenta eliminar un registro que no le pertenece
**When** la solicitud llega al API
**Then** el sistema rechaza la acciÃ³n
**And** retorna error en formato Problem Details

### Story 2.6: Registro Propio para Supervisor

**Requirements:** FR9, FR19

As a supervisor,
I want registrar y eliminar mis propias horas igual que un empleado,
So that mi rol de supervisiÃ³n no me impida cumplir mi timesheet personal.

**Acceptance Criteria:**

**Given** un usuario con rol Supervisor
**When** abre la pantalla de registro personal
**Then** puede crear registros propios con los mismos campos que un Empleado
**And** las reglas de validaciÃ³n son las mismas

**Given** un supervisor autenticado
**When** consulta su historial personal
**Then** ve sus propios registros
**And** no se mezclan con registros del equipo en esta vista personal

**Given** un supervisor elimina un registro propio
**When** confirma la acciÃ³n
**Then** la eliminaciÃ³n respeta las mismas reglas aplicadas al Empleado
**And** no requiere funcionalidades futuras de supervisiÃ³n de equipo

### Story 2.7: Accesibilidad y Estados del Formulario de Registro

**Requirements:** FR4, NFR15, NFR19, NFR20, UX-DR12, UX-DR21, UX-DR38, UX-DR39, UX-DR41, UX-DR42, UX-DR43

As a empleado,
I want completar el formulario usando teclado y recibir errores claros,
So that pueda registrar horas sin fricciÃ³n ni ambigÃ¼edad.

**Acceptance Criteria:**

**Given** el formulario de registro
**When** el usuario navega con `Tab`, `Shift+Tab`, `Enter`, `Space` y flechas
**Then** puede completar cliente, proyecto, modalidad, horas y acciones sin mouse
**And** el foco visible nunca se pierde

**Given** un campo invÃ¡lido
**When** el usuario sale del campo
**Then** aparece un mensaje especÃ­fico bajo el input
**And** el mensaje indica quÃ© corregir y estÃ¡ asociado con `aria-describedby`

**Given** una llamada de guardado en curso
**When** el usuario presiona Guardar
**Then** el botÃ³n muestra spinner inline y texto "Guardando..."
**And** el formulario conserva los valores si ocurre un error del sistema

## Epic 3: Control de Retroactividad, Excepciones e Inmutabilidad

Los usuarios pueden manejar registros tardÃ­os con autorizaciÃ³n, y la organizaciÃ³n conserva control sobre quÃ© se puede modificar despuÃ©s de guardar.

### Story 3.1: Aplicar Ventana de Registro Retroactivo

**Requirements:** FR7, FR15, UX-DR8, UX-DR9, UX-DR10

As a empleado,
I want saber quÃ© fechas puedo registrar directamente,
So that pueda registrar dentro de la ventana permitida sin pedir ayuda.

**Acceptance Criteria:**

**Given** la ventana de retroactividad configurada en parÃ¡metros del sistema
**When** el usuario abre el selector de fecha
**Then** el sistema marca visualmente fechas disponibles, retroactivas permitidas y fuera de ventana
**And** la regla usa dÃ­as hÃ¡biles segÃºn configuraciÃ³n, no un valor hardcodeado

**Given** una fecha dentro de la ventana permitida
**When** el empleado registra horas para esa fecha
**Then** el sistema permite guardar si el resto del formulario es vÃ¡lido
**And** registra que fue un registro retroactivo cuando aplique

**Given** una fecha fuera de ventana
**When** el empleado intenta registrar directamente
**Then** el sistema bloquea el guardado directo
**And** ofrece iniciar solicitud de excepciÃ³n

### Story 3.2: Solicitar ExcepciÃ³n de Registro Fuera de Ventana

**Requirements:** FR8, UX-DR24, UX-DR25, UX-DR26

As a empleado,
I want solicitar autorizaciÃ³n para registrar una fecha fuera de ventana,
So that pueda manejar casos justificados sin salir del sistema.

**Acceptance Criteria:**

**Given** una fecha fuera de ventana seleccionada
**When** el usuario activa la solicitud de excepciÃ³n
**Then** se abre `KpgExceptionDialog` con fecha, explicaciÃ³n y campo de justificaciÃ³n requerido
**And** el tono visual usa advertencia Ã¡mbar, no error destructivo

**Given** el diÃ¡logo de excepciÃ³n abierto
**When** la justificaciÃ³n estÃ¡ vacÃ­a
**Then** el botÃ³n Enviar solicitud permanece deshabilitado
**And** el foco inicial estÃ¡ en el textarea de justificaciÃ³n

**Given** una justificaciÃ³n vÃ¡lida
**When** el usuario envÃ­a la solicitud
**Then** se crea una solicitud en estado Pendiente
**And** se muestra confirmaciÃ³n inline de solicitud enviada

### Story 3.3: Aprobar o Rechazar Solicitudes de ExcepciÃ³n

**Requirements:** FR12, UX-DR27

As an admin,
I want revisar solicitudes de registro fuera de ventana,
So that pueda autorizar o rechazar casos excepcionales con trazabilidad.

**Acceptance Criteria:**

**Given** solicitudes de excepciÃ³n pendientes
**When** el Admin abre la vista de solicitudes
**Then** ve solicitante, fecha solicitada, antigÃ¼edad, justificaciÃ³n y estado
**And** puede aprobar o rechazar cada solicitud

**Given** una solicitud pendiente
**When** el Admin la aprueba
**Then** la solicitud cambia a Aprobada
**And** el usuario solicitante puede registrar horas para esa fecha autorizada

**Given** una solicitud pendiente
**When** el Admin la rechaza
**Then** la solicitud cambia a Rechazada
**And** el usuario no puede registrar directamente para esa fecha

### Story 3.4: Editar Solo DescripciÃ³n de Registros Permitidos

**Requirements:** FR10, FR14, FR15, NFR9, NFR12

As a supervisor or admin,
I want editar Ãºnicamente la descripciÃ³n de registros permitidos,
So that pueda corregir contexto textual sin alterar datos financieros crÃ­ticos.

**Acceptance Criteria:**

**Given** un Supervisor viendo registros de su equipo
**When** edita un registro permitido
**Then** solo puede modificar el campo descripciÃ³n
**And** no puede cambiar fecha, horas, cliente, proyecto, modalidad ni recurso

**Given** un Admin viendo cualquier registro
**When** edita un registro permitido
**Then** solo puede modificar el campo descripciÃ³n
**And** cualquier intento de modificar otros campos es rechazado por el dominio/API

**Given** un usuario sin permiso intenta editar descripciÃ³n
**When** la solicitud llega al API
**Then** el sistema la rechaza con autorizaciÃ³n server-side
**And** retorna Problem Details

### Story 3.5: Eliminar Registros por Rol Autorizado

**Requirements:** FR11, FR13, NFR9, UX-DR40

As a supervisor or admin,
I want eliminar registros dentro de mi alcance,
So that pueda corregir errores operativos sin exponer datos fuera de mis permisos.

**Acceptance Criteria:**

**Given** un Supervisor autenticado
**When** intenta eliminar un registro de un colaborador bajo su supervisiÃ³n
**Then** el sistema permite la eliminaciÃ³n tras confirmaciÃ³n explÃ­cita
**And** rechaza registros fuera de su equipo

**Given** un Admin autenticado
**When** intenta eliminar cualquier registro
**Then** el sistema permite la eliminaciÃ³n tras confirmaciÃ³n explÃ­cita
**And** la acciÃ³n respeta la polÃ­tica de dominio definida

**Given** una eliminaciÃ³n solicitada desde UI
**When** se muestra confirmaciÃ³n
**Then** el modal identifica fecha, turno y usuario del registro
**And** el botÃ³n destructivo estÃ¡ claramente diferenciado

### Story 3.6: Reglas de Inmutabilidad en Dominio y API

**Requirements:** FR15, NFR12, NFR16

As a organizaciÃ³n,
I want que los registros guardados sean inmutables salvo excepciones autorizadas,
So that la informaciÃ³n que soporta facturaciÃ³n sea confiable.

**Acceptance Criteria:**

**Given** un registro de horas ya guardado
**When** cualquier usuario intenta cambiar campos distintos a descripciÃ³n mediante API
**Then** el dominio rechaza la modificaciÃ³n
**And** el rechazo ocurre aunque el frontend haya sido manipulado

**Given** una regla de excepciÃ³n aprobada por Admin
**When** el usuario registra en la fecha autorizada
**Then** el sistema permite crear el registro asociado a la aprobaciÃ³n
**And** no abre permisos generales para modificar registros existentes

**Given** un error de regla de negocio
**When** el API responde
**Then** usa una excepciÃ³n de dominio tipada
**And** el middleware la traduce a Problem Details sin exponer detalles sensibles

## Epic 4: AdministraciÃ³n Operativa de Usuarios y CatÃ¡logos

El Admin puede mantener usuarios, roles, empleados, clientes, proyectos y parÃ¡metros sin tocar la base de datos directamente.

### Story 4.1: Gestionar Cuentas de Usuario

**Requirements:** FR20, NFR10, NFR16

As an admin,
I want crear, activar, desactivar y eliminar cuentas de usuario,
So that pueda controlar quiÃ©n accede a KPG Timesheet.

**Acceptance Criteria:**

**Given** un Admin autenticado
**When** abre la administraciÃ³n de usuarios
**Then** ve una tabla con usuarios, estado, rol y acciones disponibles
**And** la tabla usa paginaciÃ³n y ordenamiento

**Given** datos vÃ¡lidos de un nuevo usuario
**When** el Admin crea la cuenta
**Then** el usuario queda activo y puede iniciar sesiÃ³n segÃºn su rol
**And** la contraseÃ±a se almacena usando hashing seguro de Identity

**Given** una cuenta existente
**When** el Admin la desactiva o elimina
**Then** el usuario pierde acceso segÃºn la acciÃ³n ejecutada
**And** la operaciÃ³n no rompe registros histÃ³ricos asociados

### Story 4.2: Asignar y Modificar Roles

**Requirements:** FR21, FR19, NFR9

As an admin,
I want asignar y modificar roles de usuarios,
So that cada persona tenga permisos correctos segÃºn su funciÃ³n.

**Acceptance Criteria:**

**Given** un usuario existente
**When** el Admin asigna rol Empleado, Supervisor, Gerente o Admin
**Then** el sistema guarda el rol seleccionado
**And** las siguientes sesiones reflejan la navegaciÃ³n y permisos del nuevo rol

**Given** un cambio de rol
**When** el Admin confirma la modificaciÃ³n
**Then** el sistema valida que el rol sea uno de los roles permitidos
**And** rechaza valores fuera del catÃ¡logo de roles

**Given** un usuario con sesiÃ³n activa
**When** su rol cambia
**Then** el nuevo rol se aplica en el siguiente refresh/relogin segÃºn la polÃ­tica de sesiÃ³n
**And** el API sigue validando rol server-side

### Story 4.3: Gestionar Empleados

**Requirements:** FR22, NFR12, NFR17, UX-DR33, UX-DR37

As an admin,
I want crear, consultar, actualizar y desactivar empleados,
So that el sistema tenga informaciÃ³n operativa correcta para registro y reportes.

**Acceptance Criteria:**

**Given** el Admin autenticado
**When** abre catÃ¡logo de empleados
**Then** puede listar, buscar, crear, editar y desactivar empleados
**And** los empleados desactivados no aparecen como seleccionables en nuevos registros

**Given** un empleado con registros histÃ³ricos
**When** el Admin lo desactiva
**Then** los registros histÃ³ricos se conservan
**And** la relaciÃ³n histÃ³rica sigue disponible en reportes y auditorÃ­a

**Given** datos incompletos o invÃ¡lidos
**When** el Admin intenta guardar empleado
**Then** el sistema muestra errores especÃ­ficos por campo
**And** el API valida de nuevo antes de persistir

### Story 4.4: Gestionar Clientes y Proyectos

**Requirements:** FR23, FR24, NFR12, NFR17

As an admin,
I want mantener clientes y proyectos asociados,
So that los registros de horas reflejen correctamente la jerarquÃ­a de facturaciÃ³n.

**Acceptance Criteria:**

**Given** el Admin autenticado
**When** abre catÃ¡logo de clientes
**Then** puede crear, consultar, actualizar y desactivar clientes
**And** los clientes desactivados no aparecen en nuevos registros

**Given** un cliente existente
**When** el Admin administra sus proyectos
**Then** puede crear, consultar, actualizar y desactivar proyectos asociados
**And** cada proyecto queda vinculado a un cliente

**Given** un proyecto con registros histÃ³ricos
**When** el Admin lo desactiva
**Then** no puede seleccionarse en nuevos registros
**And** se mantiene en registros histÃ³ricos y reportes

### Story 4.5: Gestionar ParÃ¡metros del Sistema

**Requirements:** FR25, FR26, FR7, FR41, FR42

As an admin,
I want administrar modalidades, recursos, lugares de trabajo y ventana retroactiva,
So that las reglas operativas puedan ajustarse sin cambios de cÃ³digo.

**Acceptance Criteria:**

**Given** el Admin autenticado
**When** abre parÃ¡metros del sistema
**Then** puede mantener modalidades, recursos y lugares de trabajo
**And** los valores activos aparecen en el formulario de registro

**Given** el parÃ¡metro de ventana retroactiva
**When** el Admin lo modifica
**Then** el nuevo valor queda disponible para la lÃ³gica de registro
**And** se valida que sea un valor permitido y seguro

**Given** un parÃ¡metro desactivado
**When** un empleado abre el formulario
**Then** el parÃ¡metro no aparece para nuevas capturas
**And** registros histÃ³ricos conservan el valor usado originalmente

### Story 4.6: Validaciones, Estados y Accesibilidad de AdministraciÃ³n

**Requirements:** NFR12, NFR19, NFR20, UX-DR38, UX-DR40, UX-DR41, UX-DR42, UX-DR43

As an admin,
I want pantallas de administraciÃ³n claras, accesibles y consistentes,
So that pueda mantener datos operativos sin errores.

**Acceptance Criteria:**

**Given** cualquier tabla administrativa
**When** carga datos
**Then** usa estado de carga y skeleton rows
**And** muestra estados vacÃ­os especÃ­ficos cuando no hay registros

**Given** formularios de administraciÃ³n
**When** el Admin navega por teclado
**Then** puede completar, guardar, cancelar y confirmar acciones sin mouse
**And** el foco visible se conserva

**Given** acciones destructivas o desactivaciones
**When** el Admin las ejecuta
**Then** el sistema solicita confirmaciÃ³n especÃ­fica
**And** conserva datos histÃ³ricos mediante soft delete cuando aplique

## Epic 5: Dashboard, Reportes, Notificaciones y ExportaciÃ³n para Cierre Mensual

Supervisores, gerentes y admins pueden monitorear estado/carga del equipo, identificar pendientes crÃ­ticos, enviar notificaciones y generar reportes exportables para facturaciÃ³n.

### Story 5.1: Dashboard de Estado Diario del Equipo

**Requirements:** FR32, NFR4, UX-DR28, UX-DR29

As a supervisor,
I want ver quiÃ©n registrÃ³ y quiÃ©n estÃ¡ pendiente hoy,
So that pueda actuar antes del cierre del dÃ­a.

**Acceptance Criteria:**

**Given** un Supervisor autenticado
**When** abre el dashboard
**Then** ve el nÃºmero de pendientes como mÃ©trica principal sin scroll
**And** ve el estado diario de cada miembro de su equipo

**Given** miembros con registros completos, parciales o pendientes
**When** se renderiza la lista del equipo
**Then** cada colaborador aparece con nombre, iniciales, horas del dÃ­a y badge de estado
**And** los estados distinguen Complete, Partial y Pending

**Given** el dashboard cargando
**When** se solicitan datos
**Then** muestra estado de carga
**And** la carga objetivo es menor o igual a 3 segundos

### Story 5.2: DistribuciÃ³n de Horas del Equipo para Supervisor

**Requirements:** FR33, NFR4, NFR18, UX-DR30, UX-DR31, UX-DR32

As a supervisor,
I want ver la distribuciÃ³n de horas de mi equipo por consultor,
So that pueda entender carga y avance del perÃ­odo.

**Acceptance Criteria:**

**Given** un Supervisor autenticado
**When** selecciona un perÃ­odo
**Then** ve distribuciÃ³n de horas por consultor de su equipo
**And** no ve datos de usuarios fuera de su alcance

**Given** datos disponibles para el perÃ­odo
**When** el dashboard renderiza visualizaciones
**Then** usa `KpgStatCard` y grÃ¡ficos ApexCharts donde aplique
**And** conserva formato consistente de horas y etiquetas

**Given** no hay registros en el perÃ­odo
**When** el Supervisor consulta la distribuciÃ³n
**Then** se muestra un estado vacÃ­o especÃ­fico
**And** no se renderizan grÃ¡ficos engaÃ±osos

### Story 5.3: Dashboard Gerencial por Cliente, Proyecto y Carga

**Requirements:** FR34, FR35, NFR9, NFR18

As a gerente,
I want visualizar horas por cliente, proyecto y consultor,
So that pueda gestionar capacidad y priorizar decisiones operativas.

**Acceptance Criteria:**

**Given** un Gerente autenticado
**When** abre el dashboard gerencial
**Then** ve distribuciÃ³n de horas por cliente y proyecto
**And** ve indicadores de carga de trabajo por consultor

**Given** filtros de perÃ­odo aplicados
**When** el Gerente cambia el rango
**Then** las mÃ©tricas y grÃ¡ficos se recalculan correctamente
**And** las consultas respetan Ã­ndices para evitar degradaciÃ³n perceptible

**Given** un usuario sin rol Gerente o Admin
**When** intenta acceder al dashboard gerencial
**Then** el API rechaza el acceso
**And** la UI no muestra la secciÃ³n no permitida

### Story 5.4: MÃ©tricas Globales para Admin

**Requirements:** FR36, NFR4, NFR16, NFR18

As an admin,
I want visualizar mÃ©tricas globales del sistema,
So that pueda monitorear uso, tendencias y distribuciÃ³n general.

**Acceptance Criteria:**

**Given** un Admin autenticado
**When** abre mÃ©tricas globales
**Then** ve totales, distribuciones y tendencias del sistema
**And** puede filtrar por perÃ­odo

**Given** datos globales disponibles
**When** se cargan las mÃ©tricas
**Then** las consultas usan lecturas optimizadas con Dapper
**And** la carga cumple el objetivo de dashboard

**Given** errores al cargar mÃ©tricas
**When** ocurre un fallo interno
**Then** se muestra mensaje de error no sensible
**And** se registra contexto tÃ©cnico en logs

### Story 5.5: Reporte de Horas con Filtros

**Requirements:** FR27, FR28, NFR3, UX-DR33, UX-DR37

As a supervisor,
I want generar reportes filtrados por perÃ­odo, empleado, cliente o proyecto,
So that pueda preparar el cierre mensual sin consolidaciÃ³n manual.

**Acceptance Criteria:**

**Given** un Supervisor autenticado
**When** abre reportes
**Then** puede filtrar por dÃ­a, semana, mes o rango personalizado
**And** puede filtrar por empleado, cliente o proyecto dentro de su alcance

**Given** filtros vÃ¡lidos
**When** genera el reporte
**Then** el sistema devuelve resultados en menos de 10 segundos
**And** muestra registros con fecha, empleado, cliente, proyecto, horas y descripciÃ³n

**Given** filtros sin resultados
**When** genera el reporte
**Then** se muestra estado vacÃ­o especÃ­fico
**And** permite ajustar filtros sin perder la selecciÃ³n actual

### Story 5.6: Reportes para Gerente y Admin

**Requirements:** FR30, FR31, NFR9

As a gerente or admin,
I want generar reportes segÃºn mi alcance de rol,
So that pueda analizar horas desde una perspectiva gerencial o global.

**Acceptance Criteria:**

**Given** un Gerente autenticado
**When** genera reportes
**Then** tiene las mismas capacidades de reporte del Supervisor
**And** su alcance corresponde a los datos permitidos para Gerente

**Given** un Admin autenticado
**When** genera reportes
**Then** puede usar cualquier combinaciÃ³n de filtros globales
**And** no queda limitado al equipo de un supervisor

**Given** un usuario sin permisos de reportes
**When** intenta acceder al endpoint de reportes
**Then** el API rechaza la solicitud
**And** la UI no expone el mÃ³dulo de reportes

### Story 5.7: Exportar Reportes a Excel y PDF

**Requirements:** FR29, FR30, FR31, NFR3, UX-DR35

As a supervisor,
I want exportar reportes en Excel y PDF,
So that pueda entregar informaciÃ³n digital lista para facturaciÃ³n.

**Acceptance Criteria:**

**Given** un reporte generado con filtros vÃ¡lidos
**When** el usuario selecciona Exportar Excel
**Then** el sistema genera un archivo `.xlsx` con los datos filtrados
**And** usa MiniExcel segÃºn arquitectura

**Given** un reporte generado con filtros vÃ¡lidos
**When** el usuario selecciona Exportar PDF
**Then** el sistema genera un archivo PDF legible y listo para envÃ­o
**And** usa QuestPDF segÃºn arquitectura

**Given** una exportaciÃ³n en curso
**When** el archivo se estÃ¡ generando
**Then** la UI muestra estado de procesamiento
**And** maneja errores sin perder los filtros seleccionados

### Story 5.8: Tablas, Estados y Accesibilidad de Reportes/Dashboard

**Requirements:** UX-DR33, UX-DR34, UX-DR35, UX-DR36, UX-DR38, UX-DR41, UX-DR42, UX-DR43

As a usuario de dashboard o reportes,
I want tablas y filtros consistentes, rÃ¡pidos y accesibles,
So that pueda revisar informaciÃ³n sin fricciÃ³n.

**Acceptance Criteria:**

**Given** tablas de reportes, historial o bitÃ¡cora usadas en este epic
**When** se renderizan
**Then** usan MudDataGrid compacto, ordenamiento por fecha/nombre/horas y paginaciÃ³n 25/50/100
**And** las acciones de exportaciÃ³n aparecen en el header cuando aplique

**Given** filtros y tablas
**When** el usuario navega solo con teclado
**Then** puede ajustar filtros, generar reportes, ordenar tablas y exportar
**And** el foco visible se mantiene

**Given** datos cargando o ausentes
**When** el usuario consulta dashboard o reportes
**Then** ve skeleton/loading o estado vacÃ­o especÃ­fico
**And** no se muestran datos obsoletos como si estuvieran actualizados

### Story 5.9: Identificar Pendientes CrÃ­ticos por DÃ­as sin Registro

**Requirements:** FR41, FR32, FR36

As a supervisor,
I want ver quÃ© consultores llevan varios dÃ­as sin registrar horas,
So that pueda priorizar seguimiento antes de que afecte el cierre mensual.

**Acceptance Criteria:**

**Given** el parÃ¡metro de dÃ­as sin registro configurado
**When** el dashboard calcula estado del equipo
**Then** identifica empleados/consultores que superan ese umbral
**And** los marca como pendientes crÃ­ticos

**Given** un Supervisor autenticado
**When** abre el dashboard
**Then** ve pendientes crÃ­ticos diferenciados de pendientes del dÃ­a
**And** solo ve personas dentro de su equipo

**Given** un Admin autenticado
**When** abre mÃ©tricas globales
**Then** puede ver pendientes crÃ­ticos de toda la organizaciÃ³n
**And** puede filtrar por equipo, empleado o perÃ­odo

### Story 5.10: Enviar Correos AutomÃ¡ticos por Registros Pendientes

**Requirements:** FR42, FR37, NFR16

As a empleado or consultor,
I want recibir un correo cuando llevo varios dÃ­as sin registrar horas,
So that pueda corregir el atraso antes del cierre mensual.

**Acceptance Criteria:**

**Given** un empleado supera el umbral de dÃ­as sin registro
**When** corre el proceso de notificaciÃ³n
**Then** el sistema envÃ­a un correo al empleado/consultor
**And** el correo indica dÃ­as pendientes y acciÃ³n esperada

**Given** configuraciÃ³n SMTP o proveedor interno vÃ¡lida
**When** el proceso envÃ­a correos
**Then** registra resultado de envÃ­o por destinatario
**And** no duplica notificaciones para el mismo pendiente dentro de la ventana configurada

**Given** falla el envÃ­o de un correo
**When** el proveedor retorna error
**Then** el sistema registra el fallo con contexto tÃ©cnico
**And** no interrumpe el procesamiento de otros destinatarios

### Story 5.11: Configurar Umbral e Historial de Notificaciones

**Requirements:** FR43, FR26, FR41, FR42

As an admin,
I want configurar cuÃ¡ndo se envÃ­an correos y consultar el historial,
So that el seguimiento de pendientes sea controlable y auditable.

**Acceptance Criteria:**

**Given** un Admin autenticado
**When** abre parÃ¡metros del sistema
**Then** puede configurar el nÃºmero de dÃ­as sin registro que dispara notificaciÃ³n
**And** el valor se valida antes de guardarse

**Given** notificaciones enviadas
**When** el Supervisor consulta historial de su equipo
**Then** ve destinatario, fecha/hora, motivo y resultado del envÃ­o
**And** no ve notificaciones de otros equipos

**Given** notificaciones enviadas
**When** el Admin consulta historial global
**Then** puede filtrar por empleado, equipo, fecha y resultado
**And** puede usar esa informaciÃ³n para seguimiento operativo

## Epic 6: AuditorÃ­a, Trazabilidad y PreparaciÃ³n de Go-Live

La organizaciÃ³n puede consultar acciones sensibles, exportar bitÃ¡coras y operar con trazabilidad suficiente para auditorÃ­as y salida a producciÃ³n.

### Story 6.1: Registrar Eventos Sensibles en BitÃ¡cora

**Requirements:** FR37, FR43, NFR16

As a organizaciÃ³n,
I want que las acciones sensibles queden registradas en una bitÃ¡cora append-only,
So that exista trazabilidad confiable para auditorÃ­as.

**Acceptance Criteria:**

**Given** una acciÃ³n sensible del sistema
**When** ocurre inicio de sesiÃ³n, cambio de rol, alta/baja de usuario, modificaciÃ³n autorizada, aprobaciÃ³n/rechazo de excepciÃ³n o notificaciÃ³n enviada
**Then** el sistema registra evento en bitÃ¡cora
**And** incluye tipo de evento, usuario, entidad afectada, timestamp y metadata relevante

**Given** un evento de negocio crÃ­tico
**When** el handler correspondiente termina la operaciÃ³n
**Then** llama explÃ­citamente al servicio de bitÃ¡cora
**And** la bitÃ¡cora no permite ediciÃ³n o eliminaciÃ³n desde la aplicaciÃ³n

**Given** un fallo interno
**When** se registra diagnÃ³stico tÃ©cnico
**Then** no expone informaciÃ³n sensible al usuario final
**And** conserva contexto suficiente para soporte

### Story 6.2: Consultar BitÃ¡cora como Admin

**Requirements:** FR38, UX-DR33, UX-DR34, UX-DR37

As an admin,
I want consultar y filtrar la bitÃ¡cora completa,
So that pueda responder auditorÃ­as y revisar acciones sensibles.

**Acceptance Criteria:**

**Given** un Admin autenticado
**When** abre la bitÃ¡cora
**Then** puede filtrar por fecha, usuario y tipo de acciÃ³n
**And** ve eventos de todo el sistema

**Given** resultados de bitÃ¡cora
**When** se muestran en tabla
**Then** usan MudDataGrid compacto con ordenamiento y paginaciÃ³n
**And** fechas y horas son legibles y consistentes

**Given** filtros sin resultados
**When** el Admin consulta
**Then** se muestra estado vacÃ­o especÃ­fico
**And** los filtros se conservan para ajustes

### Story 6.3: Consultar BitÃ¡cora por Supervisor y Gerente

**Requirements:** FR39, FR40, NFR9

As a supervisor or gerente,
I want consultar bitÃ¡cora segÃºn mi alcance,
So that pueda revisar acciones relevantes sin acceder a informaciÃ³n no autorizada.

**Acceptance Criteria:**

**Given** un Supervisor autenticado
**When** consulta bitÃ¡cora
**Then** solo ve acciones de su equipo
**And** no puede consultar eventos fuera de su alcance

**Given** un Gerente autenticado
**When** consulta bitÃ¡cora
**Then** ve la bitÃ¡cora del sistema segÃºn alcance gerencial definido
**And** el API valida permisos server-side

**Given** un usuario sin permisos de auditorÃ­a
**When** intenta acceder a bitÃ¡cora
**Then** el API rechaza la solicitud
**And** la UI no expone el mÃ³dulo

### Story 6.4: Exportar BitÃ¡cora para AuditorÃ­a

**Requirements:** FR38, NFR16, UX-DR35

As an admin,
I want exportar bitÃ¡coras filtradas,
So that pueda entregar evidencia digital a clientes o direcciÃ³n.

**Acceptance Criteria:**

**Given** un Admin con filtros aplicados
**When** selecciona exportar bitÃ¡cora
**Then** el sistema genera archivo con los eventos filtrados
**And** incluye criterios de filtro y fecha de generaciÃ³n

**Given** una exportaciÃ³n de bitÃ¡cora
**When** contiene metadata tÃ©cnica
**Then** excluye secretos o informaciÃ³n sensible innecesaria
**And** conserva datos necesarios para trazabilidad

**Given** una exportaciÃ³n en curso
**When** ocurre un error
**Then** la UI muestra error no sensible
**And** el sistema registra diagnÃ³stico tÃ©cnico

### Story 6.5: Logging, Backups y RecuperaciÃ³n Operativa

**Requirements:** NFR13, NFR14, NFR16

As a responsable tÃ©cnico,
I want tener logging, backups y recuperaciÃ³n definidos,
So that la plataforma sea operable en producciÃ³n.

**Acceptance Criteria:**

**Given** ambiente de producciÃ³n
**When** la aplicaciÃ³n registra logs
**Then** usa Serilog con logging estructurado y rotaciÃ³n diaria
**And** no escribe secretos en logs

**Given** base de datos productiva
**When** termina cada dÃ­a operativo
**Then** SQL Server Agent ejecuta respaldo automÃ¡tico diario
**And** existe procedimiento documentado de recuperaciÃ³n en mÃ¡ximo 4 horas

**Given** horario laboral lunes a viernes 7am-8pm
**When** se evalÃºa disponibilidad objetivo
**Then** la plataforma apunta a disponibilidad mayor o igual a 99%
**And** los fallos internos quedan registrados para diagnÃ³stico

### Story 6.6: Checklist de Go-Live, QA y UAT

**Requirements:** UX-DR41, UX-DR42, UX-DR43, NFR13

As a product owner,
I want una validaciÃ³n final antes de producciÃ³n,
So that el equipo lance solo cuando los flujos crÃ­ticos estÃ©n listos.

**Acceptance Criteria:**

**Given** el MVP implementado
**When** inicia preparaciÃ³n de go-live
**Then** existe checklist aprobado con ambientes, accesos, roles, backups, monitoreo bÃ¡sico y ventana de reversiÃ³n
**And** ningÃºn defecto crÃ­tico de seguridad o flujo principal queda abierto

**Given** QA funcional e integraciÃ³n
**When** se validan flujos J1-J4
**Then** registro, excepciones, reportes, administraciÃ³n y auditorÃ­a pasan pruebas principales
**And** Chrome, Edge y Firefox quedan cubiertos

**Given** UAT de negocio
**When** el Product Owner valida flujo de registro, catÃ¡logos y reportes principales
**Then** la salida a producciÃ³n queda autorizada
**And** se activa hiper care de la primera semana

### Story 6.7: PolÃ­tica de Idioma y PreparaciÃ³n para LocalizaciÃ³n

**Requirements:** NFR21, UX-DR47, UX-DR36, NFR20

As a product owner,
I want definir y validar la polÃ­tica de idioma de la V1,
So that el sistema salga a producciÃ³n con textos consistentes en EspaÃ±ol y pueda evolucionar a InglÃ©s sin rehacer la experiencia.

**Acceptance Criteria:**

**Given** la V1 lista para go-live
**When** se revisan pantallas, formularios, mensajes de validaciÃ³n, estados vacÃ­os, confirmaciones y errores visibles
**Then** todos los textos de usuario final estÃ¡n en EspaÃ±ol
**And** no hay mezcla innecesaria de EspaÃ±ol/InglÃ©s en navegaciÃ³n, acciones, etiquetas o mensajes.

**Given** que InglÃ©s queda fuera del alcance funcional de V1
**When** se documenta la estrategia de localizaciÃ³n
**Then** queda explÃ­cito que V1 soporta solo EspaÃ±ol
**And** se registra InglÃ©s como capacidad futura sin selector de idioma en V1.

**Given** una futura versiÃ³n multi idioma
**When** el equipo prepare la implementaciÃ³n
**Then** existe una ruta tÃ©cnica propuesta para recursos localizables, cultura de UI, formatos de fecha/hora, mensajes de validaciÃ³n y Problem Details/API
**And** se identifican los textos hardcodeados que deberÃ¡n migrarse a recursos.
