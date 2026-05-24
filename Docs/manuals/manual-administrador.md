# Manual de Administrador — KPG Timesheet

**Versión:** 1.2  
**Fecha:** 2026-05-23  
**Rol requerido:** Admin  

---

## Tabla de Contenidos

1. [Responsabilidades del Administrador](#1-responsabilidades-del-administrador)
2. [Gestión de Usuarios](#2-gestión-de-usuarios)
3. [Gestión de Roles](#3-gestión-de-roles)
4. [Gestión de Recursos (Empleados)](#4-gestión-de-recursos-empleados)
5. [Gestión de Clientes y Proyectos](#5-gestión-de-clientes-y-proyectos)
6. [Parámetros del Sistema](#6-parámetros-del-sistema)
7. [Solicitudes de Excepción](#7-solicitudes-de-excepción)
8. [Bitácora de Auditoría](#8-bitácora-de-auditoría)
9. [Reportes y Exportaciones](#9-reportes-y-exportaciones)
10. [Tareas de Mantenimiento Periódico](#10-tareas-de-mantenimiento-periódico)

---

## 1. Responsabilidades del Administrador

El rol **Admin** tiene acceso completo al sistema. Sus responsabilidades incluyen:

- Crear y gestionar cuentas de usuario del equipo KPG.
- Asignar y modificar roles.
- Mantener los catálogos activos: empleados, clientes, proyectos, modalidades y lugares de trabajo.
- Configurar los parámetros operativos del sistema.
- Aprobar o rechazar solicitudes de excepción de registro.
- Monitorear la bitácora de auditoría.
- Generar reportes para cierres y análisis gerenciales.

### Selector de idioma

La interfaz del sistema está disponible en **español** e **inglés**. El selector de idioma se encuentra en la barra superior derecha (ícono ES/EN). El Admin puede cambiar el idioma en cualquier momento; la preferencia se guarda en el navegador.

### Acceso al panel de administración

El menú lateral muestra una sección **Administración** y una sección **Parámetros** exclusivas para el Admin:

| Sección | Ruta | Descripción |
|---------|------|-------------|
| Usuarios | `/admin/usuarios` | CRUD de cuentas de usuario |
| Recursos | `/admin/empleados` | Catálogo de empleados/recursos |
| Clientes | `/admin/clientes` | Catálogo de clientes y proyectos |
| Solicitudes excepción | `/solicitudes-excepcion` | Aprobar/rechazar excepciones |
| Bitácora Admin | `/admin/bitacora` | Bitácora completa de todos los eventos |
| Configuración General | `/admin/parametros/configuracion` | Ventana retroactividad y umbral notificaciones |
| Modalidades | `/admin/parametros/modalidades` | Catálogo de modalidades de trabajo |
| Lugares de Trabajo | `/admin/parametros/lugares` | Catálogo de lugares de trabajo |

---

## 2. Gestión de Usuarios

### Acceso

Menú lateral → **Administración → Usuarios** o navegar a `/admin/usuarios`.

### Ver lista de usuarios

La tabla muestra todos los usuarios del sistema con:
- **Email**
- **Nombre completo** (si fue ingresado)
- **Rol** asignado
- **Estado** (Activo / Inactivo)
- Acciones disponibles

### Crear un nuevo usuario

1. Hacer clic en **Nuevo Usuario**.
2. Completar el formulario:
   - **Email**: dirección de correo corporativa (será el nombre de usuario).
   - **Nombre completo**: nombre visible en reportes y dashboard (opcional pero recomendado).
   - **Contraseña**: mínimo 8 caracteres, debe incluir mayúscula, número y símbolo especial.
   - **Rol**: Empleado, Supervisor, Gerente o Admin.
3. Hacer clic en **Guardar**.

> La contraseña inicial debe ser comunicada al usuario de forma segura. El sistema no envía correo de bienvenida automático en V1.

### Editar un usuario

1. En la fila del usuario, hacer clic en el ícono **Editar** (lápiz).
2. Modificar los campos necesarios (email, nombre, contraseña).
3. Hacer clic en **Guardar**.

> Para cambiar el rol de un usuario, use la sección "Gestión de Roles" (ver sección 3).

### Desactivar / Activar un usuario

Cuando un colaborador deja la empresa o se da de baja temporalmente:

1. En la fila del usuario, hacer clic en el ícono **Desactivar** (toggle).
2. Confirmar la acción.
3. El usuario queda **Inactivo** y no puede iniciar sesión.

Para reactivarlo, repetir el proceso — el toggle activa el usuario nuevamente.

> Los registros de horas de un usuario desactivado se conservan en el sistema y siguen siendo visibles en reportes e historial.

### Restablecer contraseña

1. Hacer clic en el ícono **Editar** del usuario.
2. Ingresar la nueva contraseña en el campo correspondiente.
3. Hacer clic en **Guardar**.

Comunicar la nueva contraseña al usuario de forma segura y pedirle que la cambie en el próximo acceso.

---

## 3. Gestión de Roles

### Acceso

Los roles se asignan desde la misma pantalla de usuarios. Para cambiar el rol específicamente:

Menú lateral → **Administración → Usuarios** → fila del usuario → ícono **Cambiar Rol**.

### Roles disponibles

| Rol | Descripción |
|-----|-------------|
| **Empleado** | Acceso básico: registrar y ver su propio historial |
| **Supervisor** | Empleado + dashboard equipo, reportes, aprobar excepciones, editar descripciones |
| **Gerente** | Supervisor + dashboard gerencial por cliente/proyecto |
| **Admin** | Acceso completo al sistema |

### Cambiar el rol de un usuario

1. En la lista de usuarios, hacer clic en el ícono de **Cambiar Rol** de la fila correspondiente.
2. Seleccionar el nuevo rol en el diálogo.
3. Hacer clic en **Guardar**.

> El cambio de rol toma efecto en la próxima sesión del usuario. Si el usuario está actualmente logueado, el efecto aplica cuando vuelva a autenticarse.

> **Precaución:** Asignar rol Admin otorga acceso completo al sistema, incluyendo eliminación de registros de otros usuarios y acceso a toda la bitácora. Asignar con cuidado.

---

## 4. Gestión de Recursos (Empleados)

El catálogo de **Recursos** contiene los nombres de empleados/colaboradores que aparecen como opciones en el formulario de registro (campo "Recurso").

### Acceso

Menú lateral → **Administración → Recursos** o navegar a `/admin/empleados`.

### Ver lista de recursos

La tabla muestra nombre, estado (Activo/Inactivo) y acciones.

### Crear un recurso

1. Hacer clic en **Nuevo Recurso**.
2. Ingresar el **nombre** del recurso (máximo 100 caracteres).
3. Hacer clic en **Guardar**.

### Editar un recurso

1. Hacer clic en el ícono **Editar** de la fila.
2. Modificar el nombre.
3. Hacer clic en **Guardar**.

### Desactivar un recurso

1. Hacer clic en el ícono **Desactivar** (toggle).
2. Confirmar: _"¿Desactivar '[nombre]'? No aparecerá en nuevos registros."_

Los recursos desactivados dejan de aparecer en el dropdown del formulario de registro, pero los registros históricos que usaban ese recurso no se modifican.

> Un recurso desactivado puede reactivarse en cualquier momento.

---

## 5. Gestión de Clientes y Proyectos

El catálogo de **Clientes** contiene las empresas a las que se les factura, cada una con sus proyectos asociados.

### Acceso

Menú lateral → **Administración → Clientes** o navegar a `/admin/clientes`.

### Crear un cliente

1. Hacer clic en **Nuevo Cliente**.
2. Ingresar el **nombre** del cliente.
3. Hacer clic en **Guardar**.

### Agregar proyectos a un cliente

1. En la fila del cliente, hacer clic en **Gestionar proyectos**.
2. En el panel de proyectos, hacer clic en **Nuevo Proyecto**.
3. Ingresar el nombre del proyecto.
4. Hacer clic en **Guardar**.

### Editar cliente o proyecto

Hacer clic en el ícono **Editar** de la fila correspondiente y guardar los cambios.

### Desactivar cliente o proyecto

Hacer clic en el ícono **Desactivar**. Los clientes/proyectos inactivos no aparecen en el formulario de registro.

> Los registros históricos que usaron un cliente/proyecto desactivado no se modifican.

---

## 6. Parámetros del Sistema

Los parámetros controlan el comportamiento operativo del sistema. Acceso exclusivo para Admin.

### Configuración General

**Ruta:** `/admin/parametros/configuracion`

#### Ventana de retroactividad

Define cuántos **días hábiles hacia atrás** puede registrar un empleado sin necesitar aprobación de excepción.

- **Valor por defecto:** 3 días hábiles.
- **Rango:** 1 a 30 días.
- Cambiar el valor y hacer clic en **Guardar**.

> Si se aumenta la ventana, los empleados podrán registrar fechas más antiguas sin excepción. Si se reduce, más solicitudes de excepción serán necesarias.

#### Umbral de notificaciones

Define cuántos **días hábiles sin registro** activan el envío de correo automático al colaborador.

- **Valor por defecto:** 2 días.
- Cambiar el valor y hacer clic en **Guardar**.

El sistema envía los correos cada mañana a las 8:00 AM a todos los colaboradores que superen este umbral.

### Aviso de expiración de sesión (appsettings)

El tiempo de aviso previo a la expiración de sesión **no se configura desde la interfaz**, sino en el archivo `appsettings.json` del backend. Coordinar con el responsable técnico para modificarlo:

```json
"Jwt": {
  "SessionWarningMinutes": 5
}
```

- **Valor por defecto:** 5 minutos antes de que expire el JWT (que dura 60 minutos).
- Si se aumenta este valor, el modal de aviso aparecerá más temprano.
- Para que el cambio surta efecto, se requiere reiniciar el servicio de la API.

### Modalidades de Trabajo

**Ruta:** `/admin/parametros/modalidades`

Gestiona las opciones del campo "Modalidad" en el formulario de registro (ej. Presencial, Remoto, Híbrido).

**Crear modalidad:**
1. Clic en **Nueva Modalidad**.
2. Ingresar nombre (máximo 100 caracteres).
3. Clic en **Guardar**.

**Desactivar modalidad:** Clic en el ícono toggle. La modalidad deja de aparecer en el formulario de registro.

> Siempre debe haber al menos una modalidad activa.

### Lugares de Trabajo

**Ruta:** `/admin/parametros/lugares`

Gestiona las opciones del campo "Lugar" en el formulario de registro (ej. Presencial Oficina, Remoto, Presencial Cliente).

Mismo funcionamiento que las modalidades: crear, editar y desactivar.

---

## 7. Solicitudes de Excepción

Cuando un colaborador intenta registrar fuera de la ventana de retroactividad, debe enviar una solicitud de excepción que el Admin aprueba o rechaza.

### Acceso

Menú lateral → **Administración → Solicitudes Excepción** o navegar a `/solicitudes-excepcion`.

### Ver solicitudes pendientes

La tabla muestra todas las solicitudes con:
- **Empleado**: quien solicita
- **Fecha solicitada**: fecha para la que quiere registrar
- **Justificación**: motivo del registro tardío
- **Fecha de solicitud**: cuándo fue enviada
- **Estado**: Pendiente / Aprobada / Rechazada

### Aprobar una solicitud

1. En la fila de la solicitud, hacer clic en **Aprobar**.
2. El sistema cambia el estado a "Aprobada".
3. El colaborador puede ahora registrar en esa fecha específica.

### Rechazar una solicitud

1. En la fila de la solicitud, hacer clic en **Rechazar**.
2. El estado cambia a "Rechazada".
3. El colaborador no podrá registrar en esa fecha.

> Las acciones de aprobación y rechazo quedan registradas en la bitácora de auditoría.

---

## 8. Bitácora de Auditoría

La bitácora registra todos los eventos sensibles del sistema: creación/eliminación de registros, cambios de rol, aprobación de excepciones, modificaciones de catálogos, etc.

### Acceso Admin (completo)

Menú lateral → **Administración → Bitácora** o navegar a `/admin/bitacora`.

### Acceso Supervisor/Gerente (solo su equipo)

Menú lateral → **Bitácora** o navegar a `/bitacora`.

> El Admin ve la bitácora completa de todos los usuarios. Supervisores y Gerentes ven solo los eventos de su equipo.

### Filtros disponibles

| Filtro | Descripción |
|--------|-------------|
| **Período** | Rango de fechas de los eventos |
| **Usuario** | Filtrar por el usuario que generó el evento |
| **Tipo de evento** | Tipo de acción registrada |

### Tipos de eventos registrados

| Tipo | Descripción |
|------|-------------|
| `RegistroCreado` | Un colaborador guardó una jornada |
| `RegistroEliminado` | Un registro fue eliminado |
| `DescripcionEditada` | Se modificó la descripción de un registro |
| `ExcepcionSolicitada` | Un colaborador envió solicitud de excepción |
| `ExcepcionAprobada` | Un Admin aprobó una excepción |
| `ExcepcionRechazada` | Un Admin rechazó una excepción |
| `RolModificado` | Se cambió el rol de un usuario |
| `UsuarioCreado` | Se creó una nueva cuenta de usuario |
| `UsuarioDesactivado` | Se desactivó una cuenta |
| `EmpleadoCreado` | Se agregó un recurso al catálogo |
| `ClienteCreado` | Se agregó un cliente al catálogo |

### Exportar bitácora

Una vez aplicados los filtros deseados, hacer clic en **Exportar Excel** para descargar la bitácora filtrada en formato `.xlsx`.

---

## 9. Reportes y Exportaciones

El Admin tiene acceso a todos los reportes disponibles en el sistema.

### Reporte de horas (tabular)

Ruta: `/reportes`

Permite generar reportes con filtros por período, empleado, cliente y proyecto. Exportable a Excel y PDF. Ver detalle en el Manual de Usuario, sección 7.

### Timesheet individual

En la sección "Timesheet individual" de `/reportes`:

1. Seleccionar el empleado.
2. Seleccionar el mes y año.
3. Hacer clic en **Descargar Timesheet**.

Descarga un Excel con el formato oficial KPG: columnas Fecha, EntradaAM, SalidaAM, EntradaPM, SalidaPM, Cliente, Proyecto, Modalidad, Recurso, Descripción.

### Bitácora exportada

Desde `/admin/bitacora`, aplicar filtros y exportar a Excel para archivos de auditoría.

---

## 10. Tareas de Mantenimiento Periódico

### Diario
- Verificar que el backup nocturno de la base de datos se ejecutó correctamente.
- Revisar la carpeta de logs por errores críticos.
- Atender solicitudes de excepción pendientes.

### Semanal
- Revisar colaboradores en la sección de **Pendientes Críticos** del dashboard y hacer seguimiento.
- Verificar que los correos automáticos de notificación se están enviando (revisar historial en `/notificaciones`).

### Al incorporar un colaborador nuevo
1. Crear su cuenta de usuario en `/admin/usuarios`.
2. Asignarle el rol correspondiente.
3. Si es necesario, crearlo también como Recurso en `/admin/empleados`.
4. Comunicarle las credenciales y la URL del sistema.

### Al dar de baja a un colaborador
1. Desactivar su cuenta en `/admin/usuarios`.
2. Desactivar su entrada en `/admin/empleados` si aplica.
3. Sus registros históricos se conservan y siguen siendo visibles en reportes.

### Al inicio de cada mes
- Generar el Timesheet individual de cada colaborador para el mes anterior.
- Exportar el reporte mensual de horas desde `/reportes` para el cierre.
- Archivar los reportes según el proceso interno de KPG.
