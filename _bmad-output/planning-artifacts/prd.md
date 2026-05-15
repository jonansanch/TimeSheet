---
stepsCompleted: ["step-01-init", "step-02-discovery", "step-02b-vision", "step-02c-executive-summary", "step-03-success", "step-04-journeys", "step-05-domain-skipped", "step-06-innovation-skipped", "step-07-project-type", "step-08-scoping", "step-09-functional", "step-10-nonfunctional", "step-11-polish", "step-12-complete"]
status: complete
completedAt: "2026-05-12"
releaseMode: phased
inputDocuments:
  - "_bmad-output/planning-artifacts/product-brief-Timesheet.md"
  - "Docs/Propuesta-TimeSheetV3.md"
workflowType: 'prd'
classification:
  projectType: web_app
  domain: general_enterprise_internal
  complexity: medium
  projectContext: brownfield
---

# Product Requirements Document — KPG Timesheet Platform

**Autor:** Jonathan
**Fecha:** 2026-05-12

---

## Resumen Ejecutivo

KPG Timesheet Platform reemplaza el sistema de registro de horas actual — archivos Excel con macros compartidos entre 30 colaboradores — por una plataforma web centralizada, auditada y segura. Cada hora trabajada queda registrada con precisión, firmada por usuario e inmutable, disponible para facturación en tiempo real. Para una firma de consultoría que factura por horas, la exactitud del timesheet es igual a la exactitud del ingreso.

El problema actual tiene tres dimensiones críticas: (1) cualquier usuario con acceso al Excel puede modificar registros históricos sin dejar rastro, comprometiendo los datos que soportan la facturación mensual; (2) los colaboradores registran horas con días o semanas de retraso, degradando la precisión; (3) el cierre mensual depende de imprimir reportes y transferirlos físicamente, introduciendo errores de transcripción y demoras en el ciclo de cobro.

La plataforma se construye con .NET 10 y Blazor WebAssembly, por equipo interno sobre infraestructura y licencias ya disponibles. **Inversión neta adicional: $0**, frente a ~$3,600–5,000/año de una solución SaaS equivalente. Entrega en 7 semanas.

### Lo Que Hace Especial a Este Producto

KPG construye internamente por cuatro razones: (1) costo cero — ningún gasto recurrente por 30 usuarios; (2) control total — la lógica de roles, ventana de 3 días y jerarquía cliente-proyecto no encaja en soluciones genéricas sin personalización costosa; (3) independencia — los datos quedan en infraestructura propia, sin riesgo de cambios de precio o discontinuación de proveedor; (4) trazabilidad contractual — cada registro es inmutable con bitácora completa, protegiendo a KPG ante auditorías de clientes sobre horas facturadas.

El diferenciador es estratégico: KPG construye su propia fuente de verdad financiera, controlada y extensible hacia notificaciones automáticas, integración de facturación y módulo de vacaciones en versiones futuras.

---

## Clasificación del Proyecto

- **Tipo:** Aplicación web SPA (Blazor WebAssembly) — uso exclusivo desktop
- **Dominio:** Herramienta empresarial interna — gestión de tiempo y facturación en firma de consultoría
- **Complejidad:** Media — RBAC con 4 roles, múltiples módulos interdependientes, integridad de datos crítica para facturación
- **Contexto:** Brownfield — reemplaza sistema Excel en producción con 30 usuarios; nueva plataforma construida desde cero sin migración de históricos

---

## Criterios de Éxito

### Éxito del Usuario

- **Empleado/Supervisor:** completa el registro diario en menos de 2 minutos sin corrección posterior; abandona el Excel en las primeras 2 semanas post go-live
- **Supervisor:** genera el reporte mensual para facturación en menos de 5 minutos sin consolidación manual
- **Admin:** administra catálogos y roles desde una sola pantalla sin acceso directo a base de datos
- **Momento "aha":** primer cierre de mes donde el reporte se genera con un clic y no requiere ningún paso manual adicional

### Éxito del Negocio

Medición a 3 meses del go-live:

| Métrica | Línea base | Meta | Método de medición |
|---------|-----------|------|-------------------|
| % de registros completados el mismo día | < 40% (estimado) | ≥ 85% | Reporte de admin + dashboard |
| Modificaciones no autorizadas a registros | No detectable | 0 | Bitácora de auditoría |
| Tiempo de generación del reporte mensual | Horas / días | < 5 minutos | Medición directa por supervisor |
| Errores de captura detectados por validación | No medido | Reducción ≥ 70% vs. mes 1 | Conteo de validaciones fallidas |
| Adopción (abandono del Excel) | 0% | 100% de 30 colaboradores en semana 2 | Reporte de admin |

### Éxito Técnico

- Cobertura QA: 100% de flujos críticos (registro, reportes, gestión de roles) cubiertos antes del go-live
- Cero brechas de seguridad: ningún acceso no autorizado a datos de otro usuario o rol
- Al cierre del **mes 1:** 100% de colaboradores en plataforma, 0 registros en Excel
- Al cierre del **mes 3:** KPIs alcanzados; primer ciclo de facturación completo generado digitalmente

---

## Scoping y Entrega por Fases

### Estrategia

MVP orientado a resolver el problema: la V1 elimina los tres riesgos críticos y reemplaza completamente el Excel. No se lanza nada que no sea un reemplazo total del proceso actual.

**Recursos:**

| Rol | Dedicación | Período |
|-----|-----------|---------|
| Desarrollador Backend (.NET) | 100% | Semanas 2–6 |
| Desarrollador Frontend (Blazor) | 100% | Semanas 2–6 |
| Product Owner / Líder funcional | 20–30% | Completo |
| QA | 50–100% | Semanas 5–7 |
| DevOps/Infraestructura | 10–20% | Semanas 1 y 7 |

### Fase 1 — MVP (7 semanas)

**Jornadas cubiertas:** J1 (empleado - registro exitoso), J2 (registro tardío con excepción), J3 (supervisor - reporte mensual), J4 (admin - catálogos y auditoría)

**Capacidades:**
- Registro de horas AM/PM con validaciones en tiempo real
- Ventana de retroactividad de 3 días con flujo de excepción y aprobación Admin
- CRUD de catálogos: empleados, clientes, proyectos, parámetros
- Reportes exportables (Excel/PDF) con filtros por período, empleado y proyecto
- Dashboard operativo: distribución de horas, estado del equipo, carga por cliente
- Seguridad: JWT, 4 roles, refresh token, bitácora de auditoría
- Arranque desde cero — sin migración de históricos

**Excluye en V1:** integración con sistemas externos, módulo de vacaciones, notificaciones automáticas, acceso móvil

### Fase 2 — Crecimiento (Post-MVP)

- Notificaciones automáticas a colaboradores sin registro del día
- Exportación/integración directa con herramienta de facturación
- Módulo de vacaciones y ausencias

### Fase 3 — Visión (Futuro)

- Versión móvil
- Reportes de rentabilidad por proyecto y cliente
- Analítica avanzada de capacidad

### Mitigación de Riesgos

| Riesgo | Probabilidad | Mitigación |
|--------|-------------|------------|
| Adopción — resistencia al cambio del Excel | Media | Capacitación < 1h + hiper care semana 1 post go-live |
| Carga inicial Blazor WASM lenta | Baja | Compresión Brotli/gzip + comunicación previa a usuarios |
| Defectos críticos en QA semana 6 | Media | Extensión máxima de 1 semana antes de reevaluar alcance |
| Catálogos incompletos al arrancar | Alta | Semana 1 dedicada a levantamiento y validación con negocio |
| Reglas de negocio ambiguas | Media | Product Owner disponible 20–30% para validación rápida |

---

## Jornadas de Usuario

### Jornada 1 — Empleado: Registro diario (camino exitoso)

**Personaje:** Carlos, consultor senior con 3 años en KPG. Llena el Excel los viernes "de memoria" para toda la semana — sabe que sus datos no son exactos.

**Escena:** Son las 5:30pm del martes. Carlos termina una reunión con un cliente y abre el sistema antes de cerrar su laptop.

**Flujo:** Ingresa con sus credenciales. Selecciona turno mañana: entrada 8:00am, salida 1:00pm. Elige cliente, proyecto, modalidad "presencial" y escribe la descripción. Repite para turno tarde. El sistema le avisa que el campo "tarea" está vacío antes de guardar. Lo completa. Registro guardado en 90 segundos.

**Resultado:** Carlos sabe que sus horas están registradas correctamente, firmadas, e inmutables. La semana próxima no reconstruirá nada de memoria.

**Capacidades reveladas:** FR1–FR4, FR7, FR15 — formulario AM/PM, selección de contexto, validaciones en tiempo real, inmutabilidad.

---

### Jornada 2 — Empleado: Registro tardío fuera de ventana (caso borde)

**Personaje:** Ana, consultora junior. Estuvo de viaje con cliente 4 días sin acceso al sistema.

**Escena:** Ana regresa el jueves e intenta registrar el lunes — fuera de la ventana de 3 días hábiles.

**Flujo:** Al seleccionar la fecha, el sistema muestra: "Fecha fuera de la ventana permitida. Tu solicitud requiere autorización del administrador." Ana adjunta justificación y envía. Ricardo (Admin) revisa, aprueba. Ana completa sus registros. La bitácora registra la excepción con usuario, fecha y hora.

**Resultado:** El caso excepcional queda controlado y trazable — no bloqueado arbitrariamente, pero auditado.

**Capacidades reveladas:** FR7, FR8, FR12 — ventana de tiempo, solicitud de excepción, aprobación Admin.

---

### Jornada 3 — Supervisor: Cierre mensual y reporte de facturación

**Personaje:** Laura, supervisora. Dedica 2–4 horas al mes consolidando Excel de 8 personas para generar el reporte de facturación.

**Escena:** Último día hábil de mayo. Laura abre el sistema a las 9am.

**Flujo:** El dashboard muestra que 7 de 8 colaboradores tienen registros completos. Marcos tiene 3 días sin registro — visible de inmediato. Laura le envía aviso. Marcos completa en 10 minutos. Laura genera reporte "Mayo 2026" filtrado por su equipo, exporta PDF y lo envía digitalmente a facturación.

**Resultado:** Cierre en 20 minutos vs. 4 horas. Sin impresión, sin transcripción, sin riesgo de error manual.

**Capacidades reveladas:** FR27–FR29, FR32–FR33 — dashboard de estado, filtros de reporte, exportación digital.

---

### Jornada 4 — Admin: Gestión de catálogos y auditoría

**Personaje:** Ricardo, administrador. Configura el sistema y garantiza la integridad de los datos.

**Alta de empleado:** KPG incorpora a Sofía. Ricardo crea su perfil, asigna rol "Empleado" y la asocia al Proyecto X. Sofía registra desde su primer día.

**Auditoría ante cliente:** Un cliente solicita validar horas facturadas del mes anterior. Ricardo filtra la bitácora por proyecto del cliente, exporta el log completo en 3 minutos — qué registró cada consultor, cuándo, y si hubo modificaciones autorizadas.

**Resultado:** Ricardo pasa de "el que limpia los Excel" a "guardián de la integridad financiera de KPG". Control total, visibilidad completa.

**Capacidades reveladas:** FR20–FR26, FR37–FR38 — CRUD catálogos, bitácora filtrable y exportable.

---

### Resumen de Trazabilidad Jornadas → Capacidades

| Capacidad | Jornada | FRs |
|-----------|---------|-----|
| Formulario AM/PM con validaciones en tiempo real | J1 | FR1–FR4 |
| Inmutabilidad de registros por rol | J1, J4 | FR15 |
| Ventana de 3 días + flujo de excepción | J2 | FR7–FR8, FR12 |
| Dashboard de estado del equipo | J3 | FR32–FR33 |
| Reporte con filtros y exportación digital | J3 | FR27–FR29 |
| CRUD de catálogos | J4 | FR22–FR26 |
| Bitácora de auditoría filtrable y exportable | J4 | FR37–FR38 |

---

## Arquitectura y Decisiones Técnicas

KPG Timesheet es una SPA construida con Blazor WebAssembly. La lógica de presentación corre en el navegador; la comunicación con el servidor es exclusivamente vía API REST. Opera en red interna corporativa sin exposición pública.

### Backend — .NET 10

| Componente | Decisión y justificación |
|------------|--------------------------|
| Arquitectura | Clean Architecture — Domain, Application, Infrastructure, API |
| Comandos / Queries | CQRS con MediatR — casos de uso independientes, sin servicios masivos |
| Persistencia escritura | Entity Framework Core |
| Persistencia lectura | Dapper — SQL de alto rendimiento para reportes y dashboard |
| Mapeo | AutoMapper — DTOs independientes del dominio |
| Errores | Middleware global — respuestas estandarizadas + log de auditoría |
| Autenticación | JWT sin estado + refresh token con revocación |

### Frontend — Blazor WebAssembly

| Capa | Responsabilidad |
|------|----------------|
| Views (Razor Components) | Visualización y eventos; data binding con ViewModel |
| ViewModels | Estado de vista y lógica de presentación |
| Services | Lógica de negocio client-side |
| Repositories | Único acceso a HttpClient y endpoints REST; gestiona JWT |

### Soporte de Navegadores

| Navegador | Soporte | Prioridad |
|-----------|---------|-----------|
| Google Chrome (últimas 2 versiones) | Obligatorio | Primario |
| Microsoft Edge (últimas 2 versiones) | Obligatorio | Primario |
| Mozilla Firefox (últimas 2 versiones) | Requerido | Secundario |
| Safari / IE / Edge Legacy | No soportado | Excluido |

### Decisiones de Implementación

- Bundle Blazor WASM comprimido con Brotli/gzip — reduce tiempo de carga inicial
- JWT almacenado en memoria del cliente (no localStorage) — mitiga XSS
- CORS restringido a orígenes internos autorizados
- Índices en columnas de filtro frecuente (fecha, empleado, proyecto) para consultas Dapper
- Comunicar a usuarios que la carga inicial es más lenta que las subsecuentes (runtime .NET descargado una vez)
- Actualización del dashboard: por navegación/recarga manual — sin polling en V1

---

## Requerimientos Funcionales

> Este listado es el contrato de capacidades. Toda funcionalidad no listada aquí no existirá en el producto final sin una adición explícita.

### Registro de Horas

- **FR1:** El Empleado puede registrar sus horas de entrada y salida para los turnos de mañana y tarde de un día específico
- **FR2:** El Empleado puede asociar cada registro a un cliente, proyecto, modalidad y recurso
- **FR3:** El Empleado puede ingresar descripción de tarea y lugar de trabajo en cada registro
- **FR4:** El sistema valida los campos del formulario en tiempo real antes de permitir el guardado
- **FR5:** El Empleado puede consultar el historial de sus propios registros
- **FR6:** El Empleado puede eliminar sus propios registros
- **FR7:** El Empleado puede registrar horas con retroactividad de hasta 3 días hábiles
- **FR8:** El Empleado puede solicitar autorización para registrar fuera de la ventana, adjuntando justificación
- **FR9:** El Supervisor puede registrar y eliminar horas propias con las mismas capacidades del Empleado
- **FR10:** El Supervisor puede editar únicamente el campo descripción de registros de su equipo
- **FR11:** El Supervisor puede eliminar registros de colaboradores bajo su supervisión
- **FR12:** El Admin puede autorizar o rechazar solicitudes de registro fuera de ventana
- **FR13:** El Admin puede eliminar cualquier registro del sistema
- **FR14:** El Admin puede editar únicamente el campo descripción de cualquier registro
- **FR15:** El sistema impide modificaciones a campos distintos a la descripción una vez guardado el registro, salvo autorización explícita del Admin

### Gestión de Acceso y Seguridad

- **FR16:** El usuario puede iniciar sesión con credenciales y recibir un JWT firmado
- **FR17:** El sistema renueva la sesión activa mediante refresh token antes de su expiración
- **FR18:** El usuario puede cerrar sesión revocando el token activo
- **FR19:** El sistema restringe acceso a funcionalidades y datos según el rol del usuario (Admin, Gerente, Supervisor, Empleado)
- **FR20:** El Admin puede crear, activar, desactivar y eliminar cuentas de usuario
- **FR21:** El Admin puede asignar y modificar el rol de cualquier usuario

### Administración de Catálogos

- **FR22:** El Admin puede crear, consultar, actualizar y desactivar registros de empleados
- **FR23:** El Admin puede crear, consultar, actualizar y desactivar registros de clientes
- **FR24:** El Admin puede crear, consultar, actualizar y desactivar proyectos asociados a un cliente
- **FR25:** El Admin puede crear, consultar, actualizar y desactivar parámetros del sistema (modalidades, recursos, lugares de trabajo)
- **FR26:** El Admin puede configurar la ventana de registro retroactivo como parámetro del sistema

### Reportes

- **FR27:** El Supervisor puede generar un reporte de horas filtrado por período (día, semana, mes o rango personalizado)
- **FR28:** El Supervisor puede filtrar reportes por empleado, cliente o proyecto
- **FR29:** El Supervisor puede exportar reportes en Excel (.xlsx) y PDF
- **FR30:** El Gerente puede generar y exportar los mismos reportes disponibles para el Supervisor
- **FR31:** El Admin puede generar y exportar reportes para cualquier combinación de filtros

### Dashboard Operativo

- **FR32:** El Supervisor puede visualizar el estado de registro del día de cada miembro de su equipo (registrado / pendiente)
- **FR33:** El Supervisor puede visualizar la distribución de horas de su equipo por consultor en el período seleccionado
- **FR34:** El Gerente puede visualizar la distribución de horas por cliente y por proyecto
- **FR35:** El Gerente puede visualizar indicadores de carga de trabajo por consultor
- **FR36:** El Admin puede visualizar métricas globales del sistema (totales, distribuciones, tendencias)

### Auditoría y Control

- **FR37:** El sistema registra en bitácora: inicios de sesión, cambios de rol, altas/bajas de usuarios, modificaciones autorizadas y aprobaciones de excepciones de ventana
- **FR38:** El Admin puede consultar y exportar la bitácora con filtros por fecha, usuario y tipo de acción
- **FR39:** El Supervisor puede consultar la bitácora de acciones de su equipo
- **FR40:** El Gerente puede consultar la bitácora del sistema

---

## Requerimientos No Funcionales

### Rendimiento

- **NFR1:** Carga inicial de la aplicación (bundle Blazor WASM) ≤ 5 segundos en red interna
- **NFR2:** Guardado del formulario de registro completado en < 2 segundos
- **NFR3:** Generación de cualquier reporte completada en < 10 segundos independientemente del rango
- **NFR4:** Carga del dashboard ≤ 3 segundos
- **NFR5:** Sistema soporta 30 usuarios concurrentes sin degradación perceptible del rendimiento

### Seguridad

- **NFR6:** Toda comunicación cliente-servidor sobre HTTPS
- **NFR7:** Tokens JWT almacenados en memoria del cliente — no en localStorage ni sessionStorage
- **NFR8:** JWT expira en máximo 60 minutos; refresh token no excede 8 horas
- **NFR9:** Cada endpoint de API verifica rol del usuario antes de procesar, independientemente de restricciones en frontend
- **NFR10:** Contraseñas almacenadas con hash bcrypt o argon2 — nunca en texto plano
- **NFR11:** CORS restringido a orígenes internos autorizados
- **NFR12:** Toda entrada validada y sanitizada en backend, independientemente de la validación frontend

### Confiabilidad

- **NFR13:** Disponibilidad ≥ 99% en horario laboral (lunes a viernes, 7am–8pm)
- **NFR14:** Respaldos automáticos diarios de base de datos; recuperación ante falla en ≤ 4 horas
- **NFR15:** Pérdida de conexión durante llenado del formulario no resulta en pérdida de datos ya guardados
- **NFR16:** Errores internos registrados en log con contexto de diagnóstico, sin exponer información sensible al usuario final

### Escalabilidad

- **NFR17:** Arquitectura soporta crecimiento hasta 100 usuarios sin cambios estructurales en backend ni base de datos
- **NFR18:** Consultas de reportes y dashboard con índices optimizados que no degraden con crecimiento del volumen de registros

### Usabilidad

- **NFR19:** Formulario de registro completable mediante teclado sin requerir mouse
- **NFR20:** Mensajes de error de validación indican exactamente qué campo requiere corrección y por qué
- **NFR21:** La V1 opera en Español como único idioma soportado; la interfaz, validaciones y mensajes visibles deben mantenerse consistentes en Español, dejando documentada la estrategia para incorporar Inglés como idioma futuro sin rediseñar flujos críticos
