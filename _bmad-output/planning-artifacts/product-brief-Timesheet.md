---
title: "Product Brief: KPG Timesheet Platform"
status: "final"
created: "2026-05-12"
updated: "2026-05-12"
inputs:
  - "Docs/Propuesta-TimeSheetV3.md"
---

# Product Brief: KPG Timesheet Platform

## Resumen Ejecutivo

KPG opera con 30 colaboradores que registran sus horas de trabajo en archivos de Excel con macros. Este modelo expone a la organización a tres riesgos críticos: registros históricos que cualquier usuario puede modificar sin dejar rastro, horas capturadas con días o semanas de retraso basándose en memoria, y un proceso de facturación mensual que depende de imprimir reportes y transferirlos físicamente. El resultado es información de dudosa confiabilidad que alimenta decisiones de capacidad y cobro.

La solución es una plataforma web empresarial construida con .NET 10 y Blazor WebAssembly, que centraliza el registro de horas, impone validaciones en tiempo real, controla quién puede modificar qué y cuándo, y genera reportes digitales listos para facturación en menos de 5 minutos. El proyecto se ejecuta en 7 semanas con equipo interno sobre infraestructura y licencias ya disponibles — **inversión neta adicional: $0**, frente a un costo recurrente de ~$3,600–5,000/año que implicaría una solución SaaS equivalente (Harvest, Clockify o similar a ~$10–14/usuario/mes para 30 usuarios).

---

## El Problema

Cada mes, el proceso de timesheet en KPG produce datos que nadie puede garantizar como correctos.

**Integridad comprometida:** El archivo Excel circula entre los 30 colaboradores sin control de versiones ni auditoría. Cualquier persona con acceso puede alterar un registro anterior — de forma accidental o deliberada — sin que quede ningún rastro. Esos mismos datos son la base de la facturación mensual a clientes.

**Registro tardío:** Los colaboradores completan el timesheet cuando pueden, no cuando ocurrieron las horas. Es común que pasen varios días o semanas entre el evento y el registro. El resultado son estimaciones disfrazadas de datos precisos.

**Facturación manual:** Al cierre de cada mes, alguien consolida los Excel, genera un reporte, lo imprime y lo entrega al área de facturación. Cada paso manual es una oportunidad de error, retraso o pérdida de información.

El costo del statu quo no es solo operativo — es directamente financiero: **KPG factura a sus clientes por horas trabajadas**. Datos imprecisos o manipulados significan horas no cobradas, horas cobradas de más, o ciclos de facturación retrasados. En una firma de consultoría, la exactitud del timesheet es exactamente igual a la exactitud del ingreso.

---

## La Solución

Una plataforma web accesible desde cualquier navegador de escritorio donde cada colaborador registra sus horas diarias (turnos mañana/tarde) asociadas a un cliente, proyecto y contexto de trabajo. El sistema valida la información en tiempo real, impide modificaciones no autorizadas y genera reportes digitales con un clic.

**Experiencia del colaborador:** Abre el sistema, selecciona la fecha (dentro de los últimos 3 días hábiles), completa el formulario con cliente, proyecto, modalidad y descripción, y guarda. El registro queda firmado con su usuario y marca de tiempo — inmutable salvo autorización de supervisor o admin.

**Experiencia del supervisor/gerente:** Accede a un dashboard con la distribución de horas del equipo, identifica colaboradores con registros pendientes, y genera el reporte mensual para facturación en formato digital (Excel/PDF) sin intervención manual.

**Control total:** Cada acción queda en bitácora. Los catálogos de empleados, clientes y proyectos se administran desde una pantalla centralizada. Los roles determinan exactamente qué puede ver y hacer cada usuario.

---

## Por Qué Este Enfoque

Existen herramientas de mercado como Clockify, Harvest o Rocketlane — algunas con versiones gratuitas. KPG elige construir internamente por razones de peso:

- **Costo cero:** equipo interno disponible + infraestructura y licencias ya adquiridas. Cualquier herramienta SaaS implicaría un costo recurrente por 30 usuarios.
- **Control total sobre reglas de negocio:** la lógica de KPG (roles específicos, ventana de 3 días, jerarquía cliente-proyecto, flujo de aprobación) no encaja limpiamente en soluciones genéricas sin personalización costosa.
- **Sin dependencia de terceros:** los datos de horas y facturación quedan en infraestructura propia, sin riesgo de cambios de precio, discontinuación del servicio o condiciones de privacidad de un proveedor externo.
- **Base para evolución propia:** la arquitectura interna permite agregar módulos futuros (notificaciones, integración de facturación, vacaciones) sin negociar con un proveedor.
- **Trazabilidad como ventaja contractual:** cada registro queda firmado con usuario, fecha y hora — inmutable sin autorización. Frente a clientes que auditen horas facturadas, KPG tendrá un sistema de registro con bitácora completa, no un archivo Excel modificable.

---

## A Quién Sirve

| Perfil | Necesidad principal | Éxito para ellos |
|--------|--------------------|--------------------|
| **Empleado** (colaborador) | Registrar sus horas rápido y sin errores, desde su computadora | Formulario en menos de 2 minutos, sin llamadas de corrección posterior |
| **Supervisor** | Verificar que su equipo registre a tiempo y generar reportes | Dashboard claro, reporte mensual listo sin consolidación manual |
| **Gerente** | Visibilidad de horas por cliente/proyecto para gestión de capacidad | Dashboard analítico con distribución de carga y tendencias |
| **Admin** | Mantener catálogos actualizados y gestionar accesos | CRUD completo, auditoría de acciones sensibles |

**Usuarios directos:** 30 colaboradores activos. Plataforma de escritorio — los patrones de uso de KPG son desde oficina o home office con computadora; el acceso móvil queda como módulo de evolución para la versión 2.

**Adopción:** se prevé una sesión de capacitación corta (< 1 hora) antes del go-live y un período de acompañamiento durante la primera semana en producción (hiper care), para asegurar que los 30 colaboradores abandonen el Excel y adopten el nuevo flujo sin fricción.

---

## Criterios de Éxito

Medición a los 3 meses del despliegue en producción:

| Indicador | Línea base | Meta | Cómo se mide |
|-----------|-----------|------|--------------|
| % de registros completados el mismo día | < 40% (estimado) | ≥ 85% | Reporte de admin + panel del dashboard |
| Modificaciones no autorizadas a registros | No detectable | 0 | Bitácora de auditoría en tiempo real |
| Tiempo de generación del reporte mensual | Horas / días | < 5 minutos | Medición directa por el supervisor |
| Errores de captura detectados por validación | No medido | Reducción ≥ 70% vs. primer mes | Conteo de validaciones fallidas en el sistema |

---

## Alcance — Versión 1 (7 semanas)

**Incluye:**
- Registro de horas diario (AM/PM) con validaciones en tiempo real
- Ventana de registro: hasta 3 días hábiles de retroactividad; fuera de ventana requiere autorización Admin
- Administración de catálogos: empleados, clientes, proyectos y parámetros
- Reportes exportables (Excel/PDF) con filtros por período, empleado y proyecto
- Dashboard operativo con distribución de horas, carga por consultor y por cliente
- Seguridad: autenticación JWT, 4 roles (Admin, Gerente, Supervisor, Empleado), auditoría de acciones sensibles
- Datos históricos: sistema arranca desde cero — sin migración de Excel. Los archivos Excel anteriores al go-live quedan fuera del alcance del sistema; no representan un riesgo operativo para KPG

**Excluye (versión 1):**
- Integración con sistemas de nómina, ERP o herramienta de facturación
- Módulo de vacaciones o ausencias
- Notificaciones automáticas
- Acceso móvil
- Cálculo de nómina o facturación automática

**Contingencia de cronograma:** si al cierre de la semana 6 (QA) existen defectos que impidan el go-live, el proyecto puede extenderse **máximo una semana adicional** antes de reevaluar el alcance de la versión 1.

---

## Visión a 2 años

Si la plataforma cumple su promesa, KPG cuenta con una fuente de datos de horas confiable y completa. Eso abre cuatro líneas naturales de evolución:

1. **Notificaciones automáticas:** alertas cuando un colaborador no ha registrado el día — ataca el problema de registro tardío sin depender de disciplina individual.
2. **Integración directa con facturación:** exportación automatizada al sistema de facturación, eliminando el último paso manual del ciclo de cobro.
3. **Módulo de vacaciones y ausencias:** gestión de permisos integrada al mismo sistema, con visibilidad de disponibilidad del equipo.
4. **Versión móvil:** acceso desde smartphone para colaboradores en campo o con clientes, completando la cobertura de uso.

La plataforma pasa de ser un registro operativo a convertirse en el sistema de inteligencia de recursos y facturación de KPG.
