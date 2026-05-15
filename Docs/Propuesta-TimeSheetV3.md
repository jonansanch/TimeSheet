# Resumen ejecutivo

El sistema actual de registro de horas (timesheet) de KPG opera sobre archivos de Excel con macros compartidos entre **30 colaboradores**, generando tres riesgos críticos: (1) **modificación no autorizada de registros sin trazabilidad** — cualquier usuario con acceso al archivo puede alterar datos históricos sin dejar rastro, comprometiendo la veracidad de la información que alimenta la facturación mensual; (2) **baja precisión por registro tardío** — los colaboradores completan el timesheet con días o semanas de retraso, reduciendo la confiabilidad de los datos; y (3) **proceso de facturación manual** — los reportes se imprimen y transfieren físicamente al área de facturación, con riesgo de errores de transcripción y retrasos en el ciclo de cobro.

Se propone migrar a una plataforma web empresarial con backend en .NET 10 y frontend en Blazor WebAssembly, incorporando administración de catálogos, validaciones y reportes/dashboards. La **inversión es cero** — el proyecto se ejecuta con equipo interno sobre infraestructura y licencias ya disponibles, convirtiendo cada mejora en valor neto directo para KPG.

- **Valor esperado:** eliminación del riesgo de manipulación de datos, registros con mayor precisión temporal, y generación de reportes digitales que reemplazan el proceso manual de facturación.

- **Impacto:** estandariza el proceso para 30 colaboradores, habilita trazabilidad completa con auditoría por usuario y rol, y reduce a minutos la generación de reportes mensuales.

- **Esfuerzo/Riesgo:** medio-bajo — modernización tecnológica con alcance acotado, equipo interno y entregables por etapas en 7 semanas.

- **Decisión requerida:** aprobación para ejecutar el proyecto en un ciclo de 7 semanas y autorizar la salida a producción.

**Proyecto:** Migración y evolución del sistema Timesheet KPG

### Alcance (alto nivel)

**Incluye:** registro de horas (AM/PM), validaciones de datos, administración de catálogos (empleados, clientes, proyectos y parámetros), reportes exportables (Excel/PDF), dashboard operativo y seguridad con roles/JWT.

**Excluye:** integración con sistemas de nómina, ERP o facturación; gestión de vacaciones, ausencias o permisos; cálculo automático de nómina; y funcionalidades de gestión de proyectos o CRM. Cualquier funcionalidad fuera de este alcance se tratará como solicitud de cambio con evaluación de impacto independiente.

**Política de registro tardío:** el sistema permitirá registrar horas con retroactividad de hasta [N] días calendario, configurable como parámetro del sistema. Registros fuera de esa ventana requerirán autorización explícita del Admin. La ventana exacta se definirá con el dueño del proceso en la Semana 1 de levantamiento.

### Supuestos y riesgos (alto nivel)

- Disponibilidad de dueños de proceso: se requiere validación rápida de reglas de negocio y catálogos.

- Migración de históricos: la calidad de los archivos Excel impacta el esfuerzo de depuración y carga.

- Adopción del cambio: se recomienda comunicación y capacitación corta para asegurar uso consistente.

- Seguridad y accesos: definición de roles/perfiles y lineamientos de autenticación corporativa (si aplica).

## 0. Situación actual y problemática

El proceso actual se basa en un archivo de Excel con macros distribuido entre los 30 colaboradores de KPG. Este modelo genera tres problemas estructurales que impactan tanto la operación interna como el ciclo de facturación:

| Problema | Descripción | Impacto |
| --- | --- | --- |
| **Integridad de datos comprometida** | El archivo Excel puede ser modificado por cualquier usuario con acceso, sin registro de cambios ni auditoría. | Registros alterados sin trazabilidad, con impacto directo en la veracidad de la información que soporta la facturación mensual. |
| **Registro tardío y baja precisión** | Los colaboradores registran sus horas con días o semanas de retraso, reconstruyendo la información de memoria. | Datos imprecisos que degradan la calidad de los reportes y la confiabilidad de los indicadores de capacidad. |
| **Proceso de facturación manual** | Los reportes mensuales se generan en Excel, se imprimen y se transfieren físicamente al área de facturación. | Riesgo de errores de transcripción, pérdida de información y retrasos en el ciclo de cobro. |

La migración a una plataforma web ataca los tres problemas de raíz: los registros quedan firmados por usuario y fecha, el sistema puede controlar ventanas de captura permitidas, y los reportes se generan y comparten digitalmente sin intermediación manual.

---

## 1. Introducción

Este documento presenta la propuesta técnica para transformar el actual sistema de registro de horas (basado en macros de Excel) en una plataforma web empresarial. La solución busca centralizar la gestión de consultores, mejorar la integridad y trazabilidad de la información, y habilitar análisis para la toma de decisiones mediante una arquitectura desacoplada, segura y escalable.

2. Descripción del sistema

La plataforma permitirá a los colaboradores registrar su jornada laboral diaria (entrada y salida en los bloques de mañana y tarde) y asociar cada registro a un cliente, proyecto y contexto de trabajo. A diferencia del modelo actual, la solución web incorporará un entorno integral para administrar catálogos, validar datos y visualizar información operativa y analítica.

Flujo principal de usuario

- Registrar dos turnos de entrada y salida (mañana/tarde).

- Seleccionar el contexto del registro (cliente, proyecto, modalidad y recurso).

- Describir la tarea realizada y el lugar de trabajo.

- Validar la información en tiempo real (reglas y consistencia de datos).

3. Arquitectura técnica

A. Backend (.NET 10)

Se implementará una Arquitectura Limpia (Clean Architecture) evolucionada con patrones de desacoplamiento avanzados para garantizar escalabilidad y mantenibilidad.

| Patrón / Herramienta | Descripción e Impacto |
| --- | --- |
| CQRS & MediatR | Separación de operaciones de escritura (Commands) y lectura (Queries). Se eliminan servicios masivos en favor de casos de uso independientes. |
| Read Stack con Dapper | Uso de un micro-ORM ligero para Reportes y Dashboards, permitiendo consultas SQL de alto rendimiento sin la sobrecarga de un ORM tradicional. |
| AutoMapper | Mapeo automatizado entre entidades de dominio y DTOs, simplificando la comunicación con Blazor y reduciendo código repetitivo. |
| Global Exception Handling | Middleware centralizado que captura errores, genera logs de auditoría y devuelve respuestas estandarizadas al usuario. |

B. Seguridad

- Autenticación: Implementación de JWT (JSON Web Tokens) para sesiones seguras y sin estado.

Autorización: Control de acceso basado en roles (Admin, Gerente, Supervisor, Empleado) y políticas específicas.

B. Frontend (Blazor WebAssembly)

Se implementará una arquitectura de 4 capas basada en el patrón MVVM para asegurar un desacoplamiento total entre la UI y la infraestructura.

- Views (UI): Componentes Razor encargados de la visualización y eventos de usuario, vinculados al ViewModel mediante Data Binding.

- ViewModels: Gestionan el estado de la vista y la lógica de presentación. Se comunican exclusivamente con la capa de Services.

- Services (Business Logic Client-Side): Orquestan la lógica de negocio en el cliente y coordinan los datos. Se comunican exclusivamente con la capa de Repositories.

- Repositories (Data Access Client-Side): Única capa con acceso al HttpClient y a los endpoints REST del API. Gestiona la autenticación JWT y la serialización de datos.

4. Módulos del sistema

I. Registro de horas (interfaz de usuario)

Formulario para colaboradores que digitaliza el “Timesheet KPG”. Incluye campos de entrada/salida (AM y PM), cliente, proyecto, modalidad, recurso, tarea diaria y lugar de trabajo, con validaciones para reducir errores de captura.

II. Administración (gestión de catálogos)

Pantallas completas de administración (CRUD) para personal autorizado, orientadas a mantener actualizados los catálogos y parámetros del sistema:

- Empleados: altas, bajas, actualización de datos y asignación de perfiles.

- Clientes y proyectos: creación y mantenimiento de la jerarquía de facturación.

- Parámetros: administración de modalidades, recursos y lugares de trabajo.

III. Reportes y análisis

- Reportes detallados: informes con filtros por mes, día, empleado o proyecto, con exportación a Excel y PDF.

- Dashboard operativo: panel con gráficas (barras, circular y tendencia) para visualizar distribución de horas, productividad por consultor y carga por cliente en tiempo cercano a tiempo real.

IV. Seguridad (JWT y roles)

Módulo transversal que garantiza que cada usuario acceda únicamente a las funciones y datos permitidos, usando autenticación basada en JWT y autorización por roles:

- Inicio de sesión y emisión de token: validación de credenciales, generación de JWT firmado y entrega segura al cliente.

- Renovación de sesión: flujo de refresh token y revocación para minimizar riesgo ante robo de sesión.

- Gestión de usuarios y roles: asignación de roles Admin, Gerente, Supervisor y Empleado (administrable por Admin).

- Autorización por endpoint y pantalla: control de acceso en API y UI para evitar exposición de funcionalidades no permitidas.

- Auditoría básica: bitácora de acciones sensibles (cambios de roles, altas/bajas, modificaciones críticas).

Matriz de roles y acciones (referencial). La acción Editar se limita exclusivamente a la actualización del campo Descripción del registro de horas; no permite cambiar fechas, horas, cliente, proyecto u otros campos.

| Acción / Módulo | Admin | Gerente | Supervisor | Empleado |
| --- | --- | --- | --- | --- |
| Iniciar sesión / obtener JWT | Sí | Sí | Sí | Sí |
| Registrar horas (Guardar) | Sí | No | Sí | Sí |
| Eliminar registro de horas | Sí | No | Sí | Sí |
| Editar solo Descripción del registro | Sí | No | Sí | No |
| Consultar reportes | Sí | Sí | Sí | No |
| Ver dashboard | Sí | Sí | Sí | No |
| Administrar catálogos (CRUD) | Sí | No | No | No |
| Gestionar usuarios y roles | Sí | No | No | No |
| Auditoría (ver bitácora) | Sí | Sí | Sí | No |

## 5. Beneficios de la solución

- **Escalabilidad:** preparada para crecer en usuarios y volumen de datos sin degradar el rendimiento.

- **Centralización:** elimina múltiples archivos de Excel y consolida una única fuente de información confiable.

- **Accesibilidad:** disponible desde cualquier navegador para registrar y consultar información desde distintas ubicaciones.

- **Analítica e inteligencia de negocio:** convierte registros operativos en indicadores y visualizaciones para gestionar recursos y facturación.

- **Eliminación del proceso manual de facturación:** los reportes mensuales se generan y comparten digitalmente, eliminando la impresión y transferencia física al área de facturación.

### Métricas de éxito (KPIs post go-live)

El éxito del proyecto se medirá a los 3 meses del despliegue en producción con los siguientes indicadores:

| KPI | Situación actual | Meta (3 meses post go-live) |
| --- | --- | --- |
| % de registros completados el mismo día del evento | No medido (estimado < 40% por registro tardío frecuente) | ≥ 85% |
| Modificaciones no autorizadas a registros históricos | No detectable (sin trazabilidad en Excel) | 0 (controlado por rol y auditoría) |
| Tiempo de generación del reporte mensual para facturación | Horas / días (proceso manual) | < 5 minutos |
| Errores de captura detectados en validación | No medido | Reducción ≥ 70% respecto a la línea base del primer mes |

6. Plan de implementación

Estimación ligera (referencial)

Estimación referencial sujeta a validación de alcance detallado y disponibilidad de usuarios clave. Como referencia, el proyecto puede ejecutarse en 7 semanas, incorporando holgura para pruebas (QA) y una salida a producción controlada.

- Semana 1 — Levantamiento de requerimientos: talleres con negocio, definición de reglas, catálogos, roles/perfiles y criterios de aceptación.

- Semanas 2–4 — Desarrollo: construcción de backend (APIs, seguridad) y frontend (formulario y administración), con entregas parciales para validación.

- Semanas 5–6 — Pruebas (QA): pruebas funcionales e integración, correcciones, regresión y preparación de datos (si aplica migración histórica).

- Semana 7 — Salida a producción: despliegue, verificación postproducción y acompañamiento inicial (hiper care).

Recursos mínimos sugeridos

- Líder funcional / Product Owner (negocio): 20–30% (definición de reglas, validación y priorización).

- Desarrollador Backend (.NET): 100% durante semanas 2–6 (y apoyo en semana 1 para definición técnica).

- Desarrollador Frontend (Blazor): 100% durante semanas 2–6.

- QA/Pruebas: 50–100% durante semanas 5–7.

- DevOps/Infraestructura: 10–20% para ambientes, despliegue y accesos (principalmente semanas 1 y 7).

- Etapa 1 (Semana 1): levantamiento de requerimientos, definición de reglas de negocio y validación de catálogos.

- Etapa 2 (Semanas 2–4): desarrollo de backend y frontend con validaciones y seguridad por roles.

- Etapa 3 (Semanas 5–6): QA (funcional/integración), correcciones, regresión y alistamiento de despliegue.

- Etapa 4 (Semana 7): salida a producción, verificación y acompañamiento inicial.

## 7. Decisión requerida y próximos pasos

### Para la dirección — Decisión de negocio

| Elemento | Detalle |
| --- | --- |
| **Aprobación solicitada** | Autorización para ejecutar el proyecto en un ciclo de 7 semanas, asignando responsable de negocio (Product Owner) y líder técnico. |
| **Inversión** | $0 costo adicional — equipo interno con infraestructura y licencias ya disponibles. |
| **Entregables clave** | Plataforma web operativa (registro, catálogos, reportes/dashboards), seguridad por roles y, si se acuerda, migración de datos históricos. |
| **Gobernanza** | Comité de seguimiento quincenal para priorización, validación y control de cambios. |
| **Definiciones pendientes** | Alcance de migración histórica, ventana de registro tardío permitida, roles/perfiles y lineamientos de despliegue. |

### Para el equipo técnico — Próximos pasos

1. **Semana 1:** talleres de levantamiento — confirmar reglas de negocio, catálogos iniciales, roles/perfiles y política de registro tardío.
2. **Semanas 2–4:** construcción iterativa con entregas parciales para validación temprana.
3. **Semanas 5–6:** ciclo QA — pruebas funcionales, integración y regresión; preparación y carga de datos si aplica migración.
4. **Semana 7:** despliegue a producción, verificación postproducción y acompañamiento inicial (hiper care).

### Go/No-Go para salida a producción

- **Go:** pruebas QA completadas (funcional e integración) con resultados satisfactorios y sin defectos críticos abiertos.
- **Go:** validación de negocio/UAT firmada sobre el flujo de registro, catálogos y reportes principales.
- **Go:** checklist de despliegue aprobado (ambientes, accesos, roles, backups y monitoreo básico).
- **No-Go:** defectos críticos o riesgos de seguridad pendientes, o datos/catálogos no validados que impacten la operación.
- **Contingencia:** ventana de reversión definida y soporte de hiper care durante la primera semana posterior al despliegue.

