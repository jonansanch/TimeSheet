# Checklist de UAT (User Acceptance Testing) — KPG Timesheet

**Versión:** 1.0  
**Fecha:** 2026-05-20  
**Product Owner:** Jonathan  
**URL del sistema:** `https://timesheet.kpg.com` (dev: `https://localhost:5201`)

---

## Instrucciones para el Product Owner

Este checklist es la validación final del sistema antes del go-live. Usted, como Product Owner, confirma que el sistema cumple las necesidades operativas de KPG. No es necesario ejecutar todos los pasos técnicos — concéntrese en la experiencia de usuario y en que los flujos de negocio funcionan como se acordaron.

**Credenciales para UAT:**

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| admin@kpg.com | Admin1234! | Admin |
| supervisor@kpg.com | Supervisor1234! | Supervisor |
| empleado@kpg.com | Empleado1234! | Empleado |

---

## Sección 1 — Acceso al Sistema

- [ ] Puedo acceder al sistema desde el navegador sin instalación adicional
- [ ] La pantalla de login muestra la identidad visual de KPG (logo, colores corporativos)
- [ ] Login con mi usuario y contraseña funciona correctamente
- [ ] Logout cierra la sesión y vuelve al login
- [ ] Si intento acceder a una URL protegida sin login, me redirige al login

**Observaciones del PO:**

---

## Sección 2 — Flujo J1: Empleado registra su jornada (camino exitoso)

*Usar: empleado@kpg.com / Empleado1234!*

- [ ] Puedo navegar a "Registro" desde el menú lateral
- [ ] El formulario de registro muestra fecha, turno AM/PM, horas, cliente, proyecto, modalidad y descripción
- [ ] Puedo completar el formulario de turno AM (mañana) sin confusión
- [ ] Puedo completar el formulario de turno PM (tarde) sin confusión
- [ ] Al guardar, el sistema confirma que el registro fue exitoso
- [ ] En "Mis Registros" puedo ver los registros que ingresé
- [ ] Los registros guardados no pueden modificarse (aparecen como solo lectura o sin botón de edición de horas)
- [ ] El menú lateral solo muestra las secciones que corresponden a mi rol (Inicio, Registro, Mis Registros)

**Calificación del flujo:** `[ ] Aprobado` / `[ ] Aprobado con observaciones` / `[ ] No aprobado`  
**Observaciones del PO:**

---

## Sección 3 — Flujo J2: Registro tardío con excepción

*Usar: empleado@kpg.com para la solicitud, admin@kpg.com para la aprobación*

- [ ] Si intento registrar una fecha de más de 3 días hábiles atrás, el sistema me informa que necesito solicitar una excepción
- [ ] Puedo enviar una solicitud de excepción con una justificación
- [ ] Como Admin, puedo ver las solicitudes pendientes en "Solicitudes"
- [ ] Puedo aprobar la solicitud y el empleado puede completar el registro
- [ ] El flujo es claro para el empleado (sabe qué hacer en cada paso)

**Calificación del flujo:** `[ ] Aprobado` / `[ ] Aprobado con observaciones` / `[ ] No aprobado`  
**Observaciones del PO:**

---

## Sección 4 — Flujo J3: Supervisor genera reporte mensual

*Usar: supervisor@kpg.com / Supervisor1234!*

- [ ] El Dashboard muestra quiénes del equipo registraron hoy y quiénes no
- [ ] En "Reportes" puedo filtrar por período y empleado
- [ ] La tabla de reportes muestra los datos con claridad (fecha, empleado, horas, cliente, proyecto)
- [ ] Puedo exportar el reporte a **Excel** y el archivo descargado contiene los datos correctos
- [ ] Puedo exportar el reporte a **PDF** y el archivo es legible
- [ ] Puedo descargar el timesheet mensual de un empleado específico (sección "Timesheet individual")
- [ ] El formato del timesheet individual es adecuado para enviarlo a facturación

**Calificación del flujo:** `[ ] Aprobado` / `[ ] Aprobado con observaciones` / `[ ] No aprobado`  
**Observaciones del PO:**

---

## Sección 5 — Flujo J4: Admin gestiona catálogos

*Usar: admin@kpg.com / Admin1234!*

- [ ] Puedo crear un nuevo empleado desde "/admin/empleados" con nombre, correo y legajo
- [ ] Puedo crear un nuevo cliente y agregarle proyectos desde "/admin/clientes"
- [ ] Los nuevos clientes/proyectos aparecen en el formulario de registro de horas
- [ ] Puedo desactivar un cliente sin perder el historial de registros asociados
- [ ] En "/admin/parámetros" puedo cambiar la ventana de retroactividad y el umbral de notificación
- [ ] La bitácora en "/admin/bitacora" muestra los cambios que realicé (alta de empleado, cambio de parámetros)
- [ ] Puedo exportar la bitácora filtrada a Excel

**Calificación del flujo:** `[ ] Aprobado` / `[ ] Aprobado con observaciones` / `[ ] No aprobado`  
**Observaciones del PO:**

---

## Sección 6 — Experiencia General y Textos

- [ ] Todos los textos visibles al usuario están en **Español** correcto (sin mezcla inglés/español en navegación, botones, mensajes)
- [ ] Los mensajes de error son claros y orientativos (no muestran códigos técnicos al usuario)
- [ ] Los estados de carga (spinners) aparecen cuando la aplicación está procesando datos
- [ ] La aplicación responde con fluidez en condiciones normales de red interna
- [ ] Los íconos y botones tienen tamaño adecuado (no son demasiado pequeños para hacer clic)
- [ ] El sistema funciona en el navegador que usa el equipo KPG habitualmente

**Observaciones del PO:**

---

## Sección 7 — Observaciones Finales y Defectos

**¿Hay algo que no funciona como esperaba?**

| # | Pantalla | Descripción del problema | Bloqueante para go-live |
|---|----------|--------------------------|------------------------|
| 1 | | | [ ] Sí / [ ] No |
| 2 | | | [ ] Sí / [ ] No |
| 3 | | | [ ] Sí / [ ] No |

**¿Hay algo que falta para que el sistema sea útil en producción?**

_______________________________________________  
_______________________________________________

---

## Sección 8 — Autorización de Go-Live

Con base en la validación realizada:

**DECISIÓN:**

`[ ] AUTORIZO EL GO-LIVE` — El sistema cumple los requisitos de negocio de KPG. Los flujos principales funcionan correctamente y no hay defectos bloqueantes.

`[ ] CONDICIONAL` — Autorizo el go-live con la condición de que los siguientes puntos se resuelvan en los próximos _____ días hábiles:

_______________________________________________  
_______________________________________________

`[ ] NO AUTORIZO` — Hay defectos bloqueantes que deben resolverse antes del go-live:

_______________________________________________  
_______________________________________________

---

**Firma del Product Owner:**  
Nombre: Jonathan  
Fecha: ___________________  
Firma: ___________________

---

> **Próximo paso tras la aprobación:** Ver [`hypercare-plan.md`](hypercare-plan.md) para las actividades de soporte activo de la primera semana.
