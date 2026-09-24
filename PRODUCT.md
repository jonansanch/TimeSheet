# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- Empleados o colaboradores de KPG que registran diariamente sus horas por cliente y proyecto.
- Supervisores y gerentes que revisan cumplimiento, aprueban registros y analizan la distribución de horas del equipo.
- Administradores que mantienen usuarios, catálogos, estructura organizacional, parámetros y permisos.

El producto está orientado principalmente a uso de escritorio desde oficina o trabajo remoto. La base actual es de aproximadamente 30 colaboradores y debe soportar crecimiento sin alterar el flujo principal.

## Product Purpose

KPG Timesheet reemplaza el registro de horas basado en archivos Excel por una plataforma empresarial centralizada, trazable y gobernada por reglas. Debe permitir registrar una jornada en menos de dos minutos, dar visibilidad operativa a supervisores y producir información confiable para reportes y facturación.

El éxito significa registros oportunos y verificables, cero modificaciones no autorizadas, supervisión clara del cumplimiento y generación rápida de reportes digitales.

## Positioning

La plataforma adapta el registro de tiempo a las reglas internas de KPG: ventana de retroactividad configurable, jerarquía cliente/proyecto, roles y permisos propios, aprobación por niveles, trazabilidad completa y administración interna sin depender de un SaaS externo.

## Operating Context

- Uso cotidiano desde navegador de escritorio, principalmente al comenzar o cerrar la jornada.
- Registro de uno o varios tramos horarios asociados a fecha, cliente, proyecto, recurso, modalidad, lugar y descripción.
- Consulta del historial y estado de aprobación por parte del colaborador.
- Bandejas de aprobación, dashboard operativo y reportes para supervisores y gerentes.
- Administración de usuarios, recursos, supervisores, clientes, proyectos y parámetros por perfiles autorizados.
- Los datos de horas alimentan procesos de control operativo y facturación, por lo que exactitud, auditabilidad y estados explícitos son críticos.

## Capabilities and Constraints

- Stack existente: .NET 10, Blazor WebAssembly, MudBlazor, API REST y arquitectura Clean Architecture/CQRS.
- Conservar rutas, APIs, endpoints, modelos, validaciones, permisos, roles y reglas de negocio existentes.
- Roles vigentes: Admin, Gerente, Supervisor y Empleado.
- Mantener la ventana de retroactividad, las solicitudes de excepción, los niveles de aprobación y la bitácora append-only.
- Mantener navegación por teclado, foco visible, mensajes específicos por campo y contraste WCAG AA.
- No inventar métricas ni datos que no puedan derivarse del frontend o de las APIs existentes.
- No introducir otra librería UI grande ni dependencias pesadas para resolver la presentación.
- La aplicación es desktop-first; las vistas deben conservar un comportamiento responsive utilizable sin redefinir el alcance funcional móvil.

## Brand Commitments

- Nombre del producto: KPG Timesheet.
- Identidad corporativa KPG: azul marino, azul de acción, superficies blancas y estados semánticos sobrios.
- El mockup entregado por el usuario es la autoridad visual para esta iteración; la aplicación existente es la autoridad funcional.
- Voz clara, directa y profesional en español, conservando la internacionalización existente.
- Reutilizar los logotipos y activos corporativos disponibles en el repositorio; no incorporar imágenes externas arbitrarias.

## Evidence on Hand

- Mockup objetivo y capturas del estado actual aportados en la conversación.
- Brief funcional en `_bmad-output/planning-artifacts/product-brief-Timesheet.md`.
- Arquitectura en `_bmad-output/planning-artifacts/architecture.md`.
- Especificación UX previa en `_bmad-output/planning-artifacts/ux-design-specification.md`.
- Design System base en `Docs/design-system.md` y su especificación terminada en `_bmad-output/implementation-artifacts/spec-design-system-base.md`.
- Implementación activa en `Fronted/src/WebUI`.
- No se deben fabricar testimonios, clientes, métricas operativas ni combinaciones recientes que no existan en datos reales.

## Product Principles

1. La fuente de verdad funcional es la aplicación y sus reglas; la fuente de verdad visual es el mockup aprobado.
2. Registrar una jornada debe ser rápido, compacto, comprensible y verificable.
3. La información gerencial debe priorizar escaneo, comparación y atención a excepciones.
4. Los permisos, la trazabilidad y las validaciones nunca se subordinan a una mejora estética.
5. La interfaz debe sentirse como un único producto empresarial coherente, no como páginas aisladas con estilos locales.

## Accessibility & Inclusion

Objetivo WCAG 2.1 AA. Todos los flujos principales deben ser utilizables con teclado, conservar foco visible, asociar etiquetas y errores a sus controles y no comunicar estados únicamente mediante color.
