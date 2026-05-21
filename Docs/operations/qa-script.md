# Script de QA Manual — Flujos J1–J4 KPG Timesheet

**Versión:** 1.0  
**Fecha:** 2026-05-20  
**Tester:** ___________________  
**Entorno:** ___________________  
**URL base:** `https://timesheet.kpg.com` (dev: `https://localhost:5201`)

---

## Credenciales de prueba

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| admin@kpg.com | Admin1234! | Admin |
| supervisor@kpg.com | Supervisor1234! | Supervisor |
| empleado@kpg.com | Empleado1234! | Empleado |
| ana.garcia@kpg.com | Empleado1234! | Empleado |
| carlos.ruiz@kpg.com | Empleado1234! | Empleado |

---

## Convenciones

- `[P]` — Paso a ejecutar manualmente
- `[V]` — Verificación (lo que debe observarse para marcar el paso como OK)
- Cada flujo debe ejecutarse en **Chrome**, **Edge** y **Firefox**
- Usar modo incógnito/privado entre cambios de usuario para limpiar la sesión
- Marcar `[x]` cuando el resultado es el esperado; `[!]` si hay desvío (documentar)

---

## Flujo J1 — Empleado: Registro diario exitoso

**Personaje:** Carlos/empleado@kpg.com — registra su jornada completa del día de hoy.  
**Rutas clave:** `/`, `/registro`, `/mis-registros`

### Preparación

- [ ] `[P]` Abrir el navegador en modo incógnito y navegar a la URL base
- [ ] `[V]` Se muestra la pantalla de login con logo KPG, campos usuario y contraseña

### Login

- [ ] `[P]` Ingresar `empleado@kpg.com` / `Empleado1234!` → clic en "Iniciar sesión"
- [ ] `[V]` Redirige a `/` (inicio). NavMenu muestra: Inicio, Registro, Mis Registros (sin Dashboard, sin Admin)

### Turno AM

- [ ] `[P]` Clic en "Registro" en el NavMenu → navega a `/registro`
- [ ] `[V]` Formulario muestra selector de fecha (default: hoy), selector de turno AM/PM
- [ ] `[P]` Seleccionar turno **AM**, fecha = hoy
- [ ] `[V]` Campos habilitados: Hora de entrada, Hora de salida, Cliente, Proyecto, Modalidad, Descripción
- [ ] `[P]` Completar: Entrada 08:00, Salida 13:00, seleccionar un cliente y proyecto disponibles, modalidad "Presencial" (u otra disponible), descripción "Test QA J1 AM"
- [ ] `[P]` Clic en "Guardar"
- [ ] `[V]` Aparece mensaje de confirmación de guardado exitoso (banner verde o similar). No hay navegación forzada.

### Turno PM

- [ ] `[P]` Cambiar selector a turno **PM** (o el formulario ya lo muestra para PM)
- [ ] `[P]` Completar: Entrada 14:00, Salida 18:00, mismo cliente/proyecto, descripción "Test QA J1 PM"
- [ ] `[P]` Clic en "Guardar"
- [ ] `[V]` Confirmación de guardado exitoso

### Verificar historial

- [ ] `[P]` Clic en "Mis Registros" en el NavMenu → navega a `/mis-registros`
- [ ] `[V]` Tabla muestra al menos 2 registros del día de hoy: uno AM (08:00–13:00) y uno PM (14:00–18:00)
- [ ] `[V]` Los datos guardados (cliente, proyecto, descripción) coinciden con lo ingresado

### Validaciones del formulario

- [ ] `[P]` Volver a `/registro`, seleccionar turno AM para hoy. Intentar guardar con campos obligatorios vacíos.
- [ ] `[V]` El formulario muestra mensajes de error de validación (campos requeridos marcados)

### Cross-browser

| Navegador | Login | AM | PM | Historial | Resultado |
|-----------|-------|----|----|-----------|-----------|
| Chrome | [ ] | [ ] | [ ] | [ ] | |
| Edge | [ ] | [ ] | [ ] | [ ] | |
| Firefox | [ ] | [ ] | [ ] | [ ] | |

---

## Flujo J2 — Empleado: Registro tardío con excepción

**Personaje:** ana.garcia@kpg.com — intenta registrar una fecha fuera de la ventana de 3 días.  
**Rutas clave:** `/registro`, `/solicitudes-excepcion` (Admin)

### Preparación

- [ ] `[P]` Identificar una fecha que esté fuera de la ventana de retroactividad (más de 3 días hábiles hacia atrás desde hoy)
- [ ] `[P]` Abrir modo incógnito, login con `ana.garcia@kpg.com` / `Empleado1234!`

### Intento de registro fuera de ventana

- [ ] `[P]` Ir a `/registro`, seleccionar turno AM, seleccionar la fecha identificada (fuera de ventana)
- [ ] `[V]` El sistema muestra un mensaje indicando que la fecha está fuera de la ventana permitida y que se requiere autorización del administrador
- [ ] `[V]` Los campos normales del registro pueden no estar habilitados / hay un flujo de excepción

### Solicitud de excepción

- [ ] `[P]` Completar la justificación requerida para la solicitud de excepción (texto libre)
- [ ] `[P]` Enviar la solicitud
- [ ] `[V]` Aparece confirmación de que la solicitud fue enviada

### Aprobación por Admin

- [ ] `[P]` Abrir nueva ventana incógnito, login con `admin@kpg.com` / `Admin1234!`
- [ ] `[P]` Navegar a `/solicitudes-excepcion`
- [ ] `[V]` La solicitud de ana.garcia aparece en la lista con estado "Pendiente"
- [ ] `[P]` Abrir la solicitud y hacer clic en "Aprobar"
- [ ] `[V]` La solicitud cambia a estado "Aprobada"

### Completar registro tras aprobación

- [ ] `[P]` Volver a la sesión de ana.garcia, ir a `/registro`
- [ ] `[P]` Seleccionar la misma fecha de la excepción aprobada
- [ ] `[V]` Ahora el formulario permite completar los campos normalmente (excepción activa)
- [ ] `[P]` Completar registro AM y guardar
- [ ] `[V]` Confirmación de guardado exitoso

### Verificar bitácora (Admin)

- [ ] `[P]` En la sesión de admin, navegar a `/admin/bitacora`
- [ ] `[V]` Aparecen eventos relacionados con la solicitud de excepción (tipo "SolicitudExcepcion" o similar)
- [ ] `[V]` Se muestra el actor (ana.garcia o admin), fecha/hora y descripción del evento

### Cross-browser

| Navegador | Mensaje fuera de ventana | Solicitud enviada | Aprobación Admin | Registro completo | Resultado |
|-----------|--------------------------|-------------------|------------------|-------------------|-----------|
| Chrome | [ ] | [ ] | [ ] | [ ] | |
| Edge | [ ] | [ ] | [ ] | [ ] | |
| Firefox | [ ] | [ ] | [ ] | [ ] | |

---

## Flujo J3 — Supervisor: Cierre mensual y reporte

**Personaje:** supervisor@kpg.com — revisa el estado del equipo y genera reportes exportables.  
**Rutas clave:** `/dashboard`, `/reportes`

### Login

- [ ] `[P]` Abrir modo incógnito, login con `supervisor@kpg.com` / `Supervisor1234!`
- [ ] `[V]` NavMenu muestra: Inicio, Registro, Mis Registros, Dashboard, Reportes, Notificaciones, Bitácora

### Dashboard — Estado del equipo

- [ ] `[P]` Navegar a `/dashboard`
- [ ] `[V]` Se muestra el panel de estado diario del equipo (qué empleados registraron hoy y cuáles no)
- [ ] `[V]` Si hay empleados sin registro, aparecen resaltados o en sección de "pendientes"
- [ ] `[V]` La sección de distribución de horas muestra datos (aunque pueden ser vacíos si no hay registros previos)

### Reportes con filtros

- [ ] `[P]` Navegar a `/reportes`
- [ ] `[V]` Se muestran filtros: período (desde/hasta), empleado, proyecto
- [ ] `[P]` Aplicar filtros: mes actual, todos los empleados, clic en "Buscar"
- [ ] `[V]` La tabla muestra registros (o estado vacío si no hay datos para el mes actual — verificar que el estado vacío se muestra correctamente)
- [ ] `[V]` Los datos en la tabla tienen: fecha, empleado, cliente, proyecto, horas AM/PM

### Exportar Excel

- [ ] `[P]` Con datos en la tabla, clic en "Exportar Excel" (o botón de descarga Excel)
- [ ] `[V]` Se descarga un archivo `.xlsx`
- [ ] `[P]` Abrir el archivo en Excel/LibreOffice
- [ ] `[V]` El archivo contiene columnas con los datos del reporte (fecha, empleado, horas, etc.)
- [ ] `[V]` No hay filas en blanco ni datos corruptos

### Exportar PDF

- [ ] `[P]` Con datos en la tabla, clic en "Exportar PDF" (o botón de descarga PDF)
- [ ] `[V]` Se descarga un archivo `.pdf`
- [ ] `[P]` Abrir el archivo en el visualizador de PDF
- [ ] `[V]` El archivo contiene los datos del reporte en formato legible

### Timesheet individual

- [ ] `[P]` Buscar la sección "Timesheet individual" en la página de reportes
- [ ] `[P]` Seleccionar un empleado (e.g. empleado@kpg.com), mes actual, año actual
- [ ] `[P]` Clic en "Descargar Timesheet"
- [ ] `[V]` Se descarga un `.xlsx` con formato de timesheet mensual KPG (nombre del empleado, mes, tabla de días)

### Cross-browser

| Navegador | Dashboard | Tabla reportes | Excel | PDF | Timesheet | Resultado |
|-----------|-----------|----------------|-------|-----|-----------|-----------|
| Chrome | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Edge | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Firefox | [ ] | [ ] | [ ] | [ ] | [ ] | |

---

## Flujo J4 — Admin: Catálogos y auditoría

**Personaje:** admin@kpg.com — alta de empleado, configuración y revisión de bitácora.  
**Rutas clave:** `/admin/empleados`, `/admin/clientes`, `/admin/parametros`, `/admin/bitacora`, `/solicitudes-excepcion`

### Login

- [ ] `[P]` Abrir modo incógnito, login con `admin@kpg.com` / `Admin1234!`
- [ ] `[V]` NavMenu Admin muestra: Administración, Empleados, Clientes, Solicitudes, Bitácora, Parámetros
- [ ] `[V]` El NavMenu Admin **NO** muestra el link "Bitácora" de Supervisor/Gerente — solo el de Admin (verificar que no hay duplicado)

### Gestión de empleados

- [ ] `[P]` Navegar a `/admin/empleados`
- [ ] `[V]` Lista de empleados existentes (al menos los del seed: empleado@kpg.com, ana.garcia@kpg.com, carlos.ruiz@kpg.com)
- [ ] `[P]` Crear un empleado de prueba: nombre "QA Test", correo `qa.test@kpg.com`, legajo `QA001`
- [ ] `[V]` El nuevo empleado aparece en la lista
- [ ] `[P]` Editar el empleado de prueba: cambiar el nombre a "QA Test Editado"
- [ ] `[V]` El cambio se refleja en la lista
- [ ] `[P]` Eliminar (o desactivar) el empleado de prueba
- [ ] `[V]` El empleado ya no aparece activo en la lista

### Gestión de clientes y proyectos

- [ ] `[P]` Navegar a `/admin/clientes`
- [ ] `[P]` Crear un cliente de prueba: "Cliente QA Test"
- [ ] `[V]` El cliente aparece en la lista
- [ ] `[P]` Agregar un proyecto al cliente: "Proyecto QA"
- [ ] `[V]` El proyecto aparece asociado al cliente
- [ ] `[P]` Desactivar (soft delete) el cliente de prueba
- [ ] `[V]` El cliente marcado como inactivo ya no aparece en el selector de cliente del formulario de registro (verificar en `/registro` con otro usuario)

### Parámetros del sistema

- [ ] `[P]` Navegar a `/admin/parametros`
- [ ] `[V]` Se muestra la ventana de retroactividad actual (default: 3 días) y el umbral de notificación
- [ ] `[P]` Cambiar la ventana de retroactividad a **5 días**, guardar
- [ ] `[V]` Mensaje de éxito. El valor mostrado actualiza a 5.
- [ ] `[P]` Restaurar la ventana a **3 días**, guardar
- [ ] `[V]` Mensaje de éxito. El valor vuelve a 3.

### Bitácora de auditoría

- [ ] `[P]` Navegar a `/admin/bitacora`
- [ ] `[V]` Tabla de eventos con columnas: FechaHora, TipoEvento, Actor, EntidadAfectada, etc.
- [ ] `[P]` Filtrar por el tipo de evento de creación de empleado (el que se creó en el paso anterior)
- [ ] `[V]` Aparecen los eventos de alta/modificación del empleado de prueba con el actor "admin@kpg.com" y la fecha/hora correctas
- [ ] `[P]` Limpiar filtros, filtrar por actor `admin@kpg.com`, clic en "Buscar"
- [ ] `[V]` Se muestran todos los eventos del admin de esta sesión de QA

### Exportar bitácora

- [ ] `[P]` Con filtros aplicados, clic en "Excel" (botón de exportación)
- [ ] `[V]` Se descarga un `.xlsx` con los eventos filtrados
- [ ] `[P]` Abrir el archivo y verificar que contiene columnas: FechaHora, TipoEvento, Actor, EntidadAfectada, EntidadId, Metadata

### Verificación de aislamiento de roles (seguridad)

- [ ] `[P]` En la sesión del Admin, intentar navegar a `/bitacora` (ruta de Supervisor/Gerente) directamente en la barra de direcciones
- [ ] `[V]` **Resultado aceptable**: página accesible para Admin (la autorización del endpoint es por rol — verificar comportamiento real) O redirige a inicio. Documentar el comportamiento observado.
- [ ] `[P]` Abrir nueva ventana incógnito, login con `empleado@kpg.com`, intentar navegar a `/admin/bitacora`
- [ ] `[V]` Redirige al login o muestra acceso denegado — el empleado NO puede acceder a rutas de Admin

### Cross-browser

| Navegador | Empleados | Clientes | Parámetros | Bitácora | Export Excel | Resultado |
|-----------|-----------|----------|------------|----------|--------------|-----------|
| Chrome | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Edge | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Firefox | [ ] | [ ] | [ ] | [ ] | [ ] | |

---

## Verificación de Accesibilidad (UX-DR41, UX-DR42)

### Navegación por teclado

- [ ] `[P]` En Chrome, cargar `/registro` con un usuario Empleado. Navegar SOLO con Tab/Shift+Tab/Enter/Espacio.
- [ ] `[V]` Todos los campos del formulario son accesibles por teclado
- [ ] `[V]` El focus visible es siempre visible (outline o indicador claro en cada elemento enfocado)
- [ ] `[V]` Los botones "Guardar" y acciones de menú son alcanzables por teclado

### Axe DevTools (Chrome)

- [ ] `[P]` Instalar extensión Axe DevTools en Chrome (gratuita). Abrir `/registro` y ejecutar el análisis.
- [ ] `[V]` No hay violaciones de nivel "Critical" o "Serious" relacionadas con formularios, contraste de color, o estructura semántica
- [ ] `[P]` Ejecutar Axe en `/admin/bitacora` (página con tabla compleja)
- [ ] `[V]` No hay violaciones críticas en la tabla de datos

### Resoluciones

- [ ] `[P]` Probar en 1280px de ancho (DevTools → Responsive, 1280×800)
- [ ] `[V]` El sistema es funcional: login, registro, menú navegable
- [ ] `[P]` Probar en 1440px
- [ ] `[V]` Experiencia completa sin scroll horizontal innecesario
- [ ] `[P]` Probar en 1920px
- [ ] `[V]` El contenido no queda demasiado disperso / centrado correctamente

---

## Resultado Final del QA

| Flujo | Chrome | Edge | Firefox | Bloqueantes |
|-------|--------|------|---------|-------------|
| J1 — Registro diario | [ ] | [ ] | [ ] | |
| J2 — Excepción | [ ] | [ ] | [ ] | |
| J3 — Reporte supervisor | [ ] | [ ] | [ ] | |
| J4 — Admin catálogos+bitácora | [ ] | [ ] | [ ] | |
| Accesibilidad | [ ] | n/a | n/a | |

**Defectos encontrados:**

| # | Flujo | Descripción | Severidad | Estado |
|---|-------|-------------|-----------|--------|
| 1 | | | | |
| 2 | | | | |

**Firma del tester:**  
Nombre: ___________________  
Fecha de ejecución: ___________________  
Resultado general: `[ ] APROBADO` / `[ ] CON OBSERVACIONES` / `[ ] BLOQUEADO`
