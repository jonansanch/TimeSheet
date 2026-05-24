# Manual de Usuario — KPG Timesheet

**Versión:** 1.2  
**Fecha:** 2026-05-23  
**Sistema:** KPG Timesheet — Registro de Horas  

---

## Tabla de Contenidos

1. [Introducción](#1-introducción)
2. [Acceso al Sistema](#2-acceso-al-sistema)
3. [Pantalla de Inicio](#3-pantalla-de-inicio)
4. [Registro de Jornada (Empleado)](#4-registro-de-jornada-empleado)
5. [Mis Registros — Historial](#5-mis-registros--historial)
6. [Dashboard (Supervisor / Gerente / Admin)](#6-dashboard-supervisor--gerente--admin)
7. [Reportes](#7-reportes)
8. [Notificaciones](#8-notificaciones)
9. [Solicitar Excepción de Registro](#9-solicitar-excepción-de-registro)
10. [Cerrar Sesión](#10-cerrar-sesión)
11. [Preguntas Frecuentes](#11-preguntas-frecuentes)

---

## 1. Introducción

**KPG Timesheet** es el sistema web de registro de horas de KPG. Reemplaza el control manual en hojas de Excel y permite que cada colaborador registre su jornada diaria de forma rápida y segura.

### Roles del sistema

| Rol | Qué puede hacer |
|-----|----------------|
| **Empleado** | Registrar su jornada, ver su historial, eliminar sus propios registros |
| **Supervisor** | Todo lo del Empleado + ver dashboard del equipo, reportes, aprobar excepciones, editar descripciones |
| **Gerente** | Todo lo del Supervisor + dashboard gerencial con análisis por cliente/proyecto |
| **Admin** | Acceso completo + gestión de usuarios, catálogos, parámetros y bitácora |

### Navegadores soportados

Google Chrome, Microsoft Edge y Mozilla Firefox (versiones actuales). Resolución mínima recomendada: 1280 × 720 px.

---

## 2. Acceso al Sistema

### Iniciar sesión

1. Abrir el navegador y navegar a la URL del sistema (por ejemplo: `https://timesheet.kpg.com`).
2. Ingresar el **correo electrónico** y la **contraseña** asignados.
3. Hacer clic en **Iniciar sesión**.

### Olvidé mi contraseña

Si no recuerda su contraseña, puede restablecerla sin contactar al administrador:

1. En la pantalla de login, hacer clic en **¿Olvidó su contraseña?**.
2. Ingresar su **correo electrónico** y hacer clic en **Enviar**.
3. Revise su bandeja de entrada — recibirá un correo con un enlace de restablecimiento (válido por un tiempo limitado).
4. Hacer clic en el enlace del correo. El sistema lo lleva a la página de restablecimiento.
5. Ingresar su **nueva contraseña** (mínimo 8 caracteres, debe incluir mayúscula, número y símbolo especial).
6. Confirmar la contraseña y hacer clic en **Restablecer contraseña**.
7. Al completarse, puede iniciar sesión con la nueva contraseña.

> Si no recibe el correo en los próximos minutos, revise la carpeta de spam. Si el problema persiste, contacte al administrador del sistema.

### Cambiar contraseña (sesión activa)

Si desea cambiar su contraseña mientras está logueado:

1. Hacer clic en el ícono de usuario en la barra superior derecha.
2. Seleccionar **Cambiar contraseña**.
3. Ingresar la contraseña actual y la nueva contraseña.
4. Hacer clic en **Guardar**.

### Primer acceso

Al ingresar por primera vez, se recomienda:
- Verificar que su nombre aparece correctamente en la barra superior.
- Confirmar que el menú lateral muestra las opciones correspondientes a su rol.

---

## 3. Pantalla de Inicio

Al iniciar sesión, el sistema muestra la pantalla de inicio (`/`). Desde aquí puede:
- Navegar a cualquier sección usando el **menú lateral izquierdo**.
- Ver el nombre de usuario y rol activo en la parte superior.

### Selector de idioma

La aplicación está disponible en **español** e **inglés**. Para cambiar el idioma:

1. Hacer clic en el selector de idioma en la barra superior derecha (muestra la bandera o código del idioma activo: **ES** / **EN**).
2. Seleccionar el idioma deseado.
3. La interfaz cambia al instante y la preferencia se guarda en el navegador para futuras sesiones.

### Menú lateral por rol

**Empleado:**
- Inicio
- Registro _(registrar jornada del día)_
- Mis Registros _(historial propio)_

**Supervisor / Gerente:**
- Todo lo anterior
- Dashboard
- Reportes
- Notificaciones
- Bitácora _(historial de auditoría)_

**Admin:**
- Todo lo anterior
- Sección **Administración** con acceso a usuarios, empleados, clientes y solicitudes de excepción
- Sección **Parámetros** con configuración general, modalidades y lugares de trabajo

---

## 4. Registro de Jornada (Empleado)

### Acceso

Menú lateral → **Registro** o navegar a `/registro`.

### Vista de calendario

La página de Registro incluye un **calendario mensual** que muestra el estado de cada día:

- **Punto verde**: día con registro guardado.
- **Sin punto**: día sin registro.
- Hacer clic en cualquier día del calendario navega directamente a ese día en el formulario de registro.

El calendario es útil para identificar rápidamente qué días de la semana o mes le faltan registrar, sin necesidad de ir al historial.

### Completar el formulario

El formulario de registro permite capturar la jornada del día en un solo paso.

#### Fecha

- Por defecto aparece la **fecha de hoy**.
- Para cambiar la fecha, hacer clic en el campo de fecha y seleccionar el día deseado.
- Solo se permiten fechas dentro de la **ventana de retroactividad** configurada (normalmente 3 días hábiles). Si intenta registrar una fecha anterior, el sistema mostrará una advertencia y podrá solicitar una excepción.

#### Turno AM

El formulario muestra directamente los campos de hora del turno AM:
- **Entrada**: hora de inicio del turno (formato HH:MM, ej. `08:00`).
- **Salida**: hora de fin del turno (ej. `13:00`).

> La hora de salida debe ser mayor que la hora de entrada.

#### Turno PM

De igual forma, los campos de hora del turno PM:
- **Entrada**: hora de inicio del turno (ej. `14:00`).
- **Salida**: hora de fin del turno (ej. `18:00`).

> Debe completar al menos un turno (AM o PM) para poder guardar. Deje vacíos los campos del turno que no aplique.

#### Campos comunes (compartidos entre AM y PM)

| Campo | Descripción | Obligatorio |
|-------|-------------|-------------|
| **Cliente** | Empresa o cliente al que se factura el trabajo | Sí |
| **Proyecto** | Proyecto específico del cliente | Sí |
| **Modalidad** | Presencial, Remoto o Híbrido | Sí |
| **Recurso** | Tipo de recurso (Consultor, Analista, Líder técnico, etc.) | Sí |
| **Descripción** | Resumen de las actividades realizadas | Sí |
| **Lugar** | Lugar físico de trabajo (Oficina, Cliente, Remoto, etc.) | Sí |

#### Sugerencias recientes

Debajo del campo de fecha, el sistema muestra tarjetas de **registros recientes** del usuario. Al hacer clic en una tarjeta, los campos de cliente, proyecto, modalidad, recurso, descripción y lugar se autocompletarán con esos valores.

### Guardar el registro

1. Completar todos los campos requeridos.
2. Hacer clic en **Guardar jornada**.
3. El sistema muestra un mensaje de confirmación con la fecha y el total de horas calculadas.

> Si ya existe un registro para esa fecha, el sistema actualiza el registro existente agregando el turno nuevo (upsert).

### Errores comunes

| Error | Causa | Solución |
|-------|-------|----------|
| "La salida debe ser mayor que la entrada" | Hora de salida ≤ hora de entrada | Corregir los campos de hora |
| "Debe ingresar al menos una hora de entrada en el turno AM o PM" | Ningún turno tiene horas | Activar y completar al menos un turno |
| "El campo Cliente es requerido" | Campo vacío | Ingresar el nombre del cliente |
| "Fecha fuera de la ventana de registro" | Fecha demasiado antigua | Solicitar excepción (ver sección 9) |

---

## 5. Mis Registros — Historial

### Acceso

Menú lateral → **Mis Registros** o navegar a `/mis-registros`.

### Ver el historial

La tabla muestra todos sus registros ordenados del más reciente al más antiguo, con las columnas:

| Columna | Descripción |
|---------|-------------|
| **Fecha** | Fecha del registro (dd/MM/yyyy) |
| **AM** | Horario del turno AM (entrada – salida) o `—` si no hay |
| **PM** | Horario del turno PM (entrada – salida) o `—` si no hay |
| **Cliente** | Cliente del registro |
| **Proyecto** | Proyecto del registro |
| **Modalidad** | Modalidad de trabajo |
| **Descripción** | Descripción de actividades |

### Paginación

Usar el selector **Mostrar:** en la parte inferior para ver 5, 10, 20, 50 o 100 registros por página. Navegar entre páginas con las flechas de paginación.

### Ordenar registros

Hacer clic en el encabezado de la columna **Fecha** para cambiar el orden ascendente/descendente.

### Eliminar un registro

1. En la fila del registro a eliminar, hacer clic en el ícono **Eliminar** (papelera roja).
2. Confirmar la acción en el diálogo que aparece.
3. El registro desaparece de la tabla y se muestra un mensaje de confirmación.

> La eliminación es permanente. Solo puede eliminar sus propios registros.

### Estado vacío

Si no tiene registros, el sistema muestra un mensaje con un botón **Registrar horas** que lo lleva directamente al formulario de registro.

---

## 6. Dashboard (Supervisor / Gerente / Admin)

### Acceso

Menú lateral → **Dashboard** o navegar a `/dashboard`.

El dashboard muestra información del equipo en tiempo real. La información varía según el rol:

### Vista Supervisor

**Estado diario del equipo:**
- Tarjetas de resumen: total de empleados, completos, parciales y pendientes del día.
- Lista de colaboradores con su estado AM/PM y badge de estado:
  - **Completo** (verde): registró AM y PM.
  - **Parcial** (amarillo): registró solo AM o solo PM.
  - **Pendiente** (rojo): no tiene registro del día.
- Selector de fecha para consultar días anteriores.

**Distribución de horas:**
- Gráfico/tabla con el total de horas por colaborador en un período seleccionable.

### Vista Gerente

Incluye todo lo del Supervisor más:

**Dashboard gerencial:**
- Horas totales por cliente en el período.
- Horas totales por proyecto en el período.
- Selector de período: este mes, esta semana, hoy, o rango personalizado.

### Vista Admin

Incluye todo lo anterior más:

**Métricas globales:**
- Total de registros en el sistema.
- Empleados activos vs. inactivos.
- Tendencia de horas en los últimos 30 días.

**Pendientes críticos:**
- Lista de colaboradores con 3 o más días hábiles sin registrar.

---

## 7. Reportes

### Acceso

Menú lateral → **Reportes** o navegar a `/reportes`.

Disponible para: Supervisor, Gerente y Admin.

### Filtros disponibles

| Filtro | Descripción |
|--------|-------------|
| **Período** | Este mes, esta semana, hoy, o rango personalizado |
| **Empleado** | Filtrar por un colaborador específico |
| **Cliente** | Filtrar por nombre de cliente (búsqueda parcial) |
| **Proyecto** | Filtrar por nombre de proyecto (búsqueda parcial) |

Al cargar la página, el período por defecto es el **mes actual**. El reporte no se genera automáticamente — debe presionar el botón **Generar Reporte**.

### Generar el reporte

1. Seleccionar los filtros deseados.
2. Hacer clic en **Generar Reporte**.
3. La tabla muestra los resultados con columnas: Fecha, Empleado, Turno, Entrada, Salida, Horas, Cliente, Proyecto, Descripción.

### Exportar

Una vez generado el reporte, puede exportarlo:
- **Excel (.xlsx)**: botón **Exportar Excel** — descarga el reporte en formato de hoja de cálculo.
- **PDF**: botón **Exportar PDF** — descarga el reporte en formato PDF.

### Timesheet individual

En la sección **Timesheet individual**:
1. Seleccionar un empleado.
2. Seleccionar el mes y año.
3. Hacer clic en **Descargar Timesheet**.
4. Se descarga un archivo Excel con el formato oficial KPG para ese empleado y mes.

### Estado vacío

Si los filtros no arrojan resultados, el sistema muestra un mensaje informativo. Puede ajustar los filtros sin perder la selección actual.

---

## 8. Notificaciones

### Acceso

Menú lateral → **Notificaciones** o navegar a `/notificaciones`.

Disponible para: Supervisor, Gerente y Admin.

### Historial de correos

Muestra la lista de correos automáticos enviados por el sistema a colaboradores con registros pendientes. Cada fila incluye:
- Fecha y hora del envío
- Destinatario
- Tipo de notificación
- Estado del envío

El sistema envía correos automáticamente cada mañana a los colaboradores que superan el umbral de días sin registrar (configurable en Parámetros).

---

## 9. Solicitar Excepción de Registro

Cuando intenta registrar una fecha fuera de la ventana de retroactividad permitida, el sistema muestra un aviso.

### Pasos para solicitar una excepción

1. En el formulario de registro, seleccionar la fecha deseada.
2. El sistema muestra: _"Esta fecha está fuera de la ventana de registro permitida. Solicitar excepción."_
3. Hacer clic en **Solicitar excepción**.
4. En el diálogo que aparece, escribir la **justificación** del registro tardío.
5. Hacer clic en **Enviar solicitud**.
6. El sistema muestra confirmación y la solicitud queda pendiente de aprobación.

### Estado de la solicitud

Una vez enviada, la solicitud puede estar en los siguientes estados:
- **Pendiente**: esperando revisión por Admin.
- **Aprobada**: puede realizar el registro en esa fecha.
- **Rechazada**: no se permite el registro en esa fecha.

> Una vez aprobada la solicitud, vuelva al formulario de registro y seleccione la misma fecha para completar el registro.

---

## 10. Cerrar Sesión

1. Hacer clic en el ícono de **Cerrar sesión** (puerta de salida) en la barra superior derecha.
2. El sistema lo redirige a la pantalla de login.

### Aviso de expiración de sesión

Por seguridad, la sesión tiene una duración limitada (60 minutos). **Antes de que expire**, el sistema muestra automáticamente un modal de aviso:

- El modal aparece con una **cuenta regresiva** (MM:SS) mostrando el tiempo restante.
- **Continuar**: extiende la sesión por otro período completo sin necesidad de volver a ingresar credenciales.
- **Cerrar sesión**: cierra la sesión inmediatamente.

Si no responde al aviso antes de que el contador llegue a cero, la sesión expira y el sistema lo redirige automáticamente al login.

> El tiempo de aviso previo (por defecto 5 minutos antes de la expiración) es configurable por el administrador técnico del sistema.

---

## 11. Preguntas Frecuentes

**¿Puedo registrar varios días de una sola vez?**  
No. El formulario de registro es para un día a la vez. Para registrar días anteriores, use el campo de fecha y seleccione cada día por separado (dentro de la ventana permitida).

**¿Qué pasa si necesito corregir la descripción de un registro ya guardado?**  
Si tiene rol Supervisor o Admin, puede editar la descripción desde la tabla de Mis Registros usando el ícono de edición (lápiz). Los Empleados no pueden editar registros ya guardados.

**¿Puedo registrar en fin de semana?**  
Los domingos están deshabilitados en el selector de fecha. Los sábados sí se pueden registrar.

**¿El sistema envía recordatorios?**  
Sí. Si no ha registrado durante el número de días configurado por el Admin, el sistema enviará un correo automático de recordatorio.

**¿Qué significa el badge "Parcial" en el dashboard?**  
Significa que el colaborador registró solo uno de los dos turnos (AM o PM) para ese día.

**¿Cómo sé cuántas horas registré este mes?**  
Vaya a **Reportes**, seleccione el período "Este mes", filtre por su usuario y haga clic en **Generar Reporte**.

**¿A quién contacto si tengo un problema técnico?**  
Contacte al administrador del sistema de su organización.
