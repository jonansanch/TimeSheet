# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

**Primarios — 30 colaboradores de KPG (rol Empleado).** Consultores y analistas de una firma
de consultoría colombiana. Registran las horas que trabajaron para un cliente y proyecto
concretos, al final de la jornada o al día siguiente, desde su computadora en oficina o
home office. El registro es una obligación administrativa que compite con su trabajo real:
cada segundo de fricción se paga en registros tardíos o reconstruidos de memoria.

**Supervisores (tres figuras distintas, no un solo rol).** La cadena de aprobación tiene tres
niveles y cada uno es una persona diferente: el *supervisor del puesto* (regla por puesto y
cliente), el *jefe directo* (`AspNetUsers.SupervisorUserId`) y el *responsable del proyecto*
(`Proyectos.SupervisorUserId`). Revisan colas de registros ajenos y necesitan decidir rápido
sobre muchos ítems, no contemplar uno.

**Gerente.** Mira horas por cliente, proyecto y consultor para gestión de capacidad y para
saber qué se puede facturar.

**Admin.** Mantiene catálogos, usuarios, organigrama, parámetros y bitácora. Es también quien
destraba los casos fuera de regla: excepciones de ventana, cargas masivas, importaciones.

## Product Purpose

Reemplazar el Excel con macros que circulaba entre los 30 colaboradores por un sistema donde
cada hora registrada queda firmada, fechada y sujeta a una cadena de aprobación. KPG **factura
a sus clientes por horas trabajadas**: la exactitud del timesheet es literalmente la exactitud
del ingreso. El éxito es que el cierre mensual para facturación pase de horas o días de
consolidación manual a un reporte digital en menos de 5 minutos, y que los registros se
completen el mismo día (meta ≥ 85%, línea base estimada < 40%).

En producción desde el 2026-06-02.

## Positioning

Clockify, Harvest o Rocketlane resuelven el registro de horas genérico. Ninguno modela sin
personalización costosa las tres reglas que a KPG le importan: la cadena de aprobación de tres
niveles con figuras distintas (puesto, jefe directo, proyecto), la ventana de retroactividad
parametrizable por persona y por rol, y la inmutabilidad del registro firmado. Construirlo
internamente costó $0 adicionales sobre infraestructura y licencias ya pagadas, frente a
~$3.600–5.000 anuales de un SaaS equivalente para 30 usuarios, y deja los datos de facturación
en infraestructura propia. La trazabilidad completa es además un argumento contractual: ante un
cliente que audite horas facturadas, KPG muestra una bitácora, no un archivo modificable.

## Operating Context

- **Escritorio primero, pero el móvil ya ocurre.** El uso real y esperado es Chrome o Edge en
  computadora (Firefox secundario), en red corporativa interna, sin requerimiento offline. Aun
  así hay colaboradores que abren la aplicación desde el celular y hoy sufren. El trabajo de
  diseño futuro debe dejar usables en pantalla chica al menos el registro y la consulta propia;
  optimizar el resto para pantalla grande sigue siendo correcto.
- **El mes es la unidad real.** Todo el sistema gira alrededor del cierre mensual: el empleado
  registra a diario, el supervisor aprueba durante el mes, y a fin de mes alguien exporta para
  facturación. Una cola de aprobaciones que se deja acumular cae entera sobre la primera
  persona que abra la pantalla.
- **La jornada se parte en hasta tres bloques horarios** (`HoraEntrada1/2/3` + `HoraSalida1/2/3`),
  no en un AM/PM. Los minutos se capturan en cuartos de hora.
- **Los datos anteriores al go-live no se migraron.** Los Excel viejos quedan fuera del sistema
  por decisión explícita del cliente.
- La capacitación fue de menos de una hora, con una semana de acompañamiento (hypercare). No hay
  presupuesto de entrenamiento para una interfaz que haya que aprenderse.

## Capabilities and Constraints

**Lo que el sistema hace hoy**

- Registro diario de horas con hasta tres bloques horarios, asociado a cliente, proyecto,
  modalidad, lugar y descripción; y registro por rango de días para cargas masivas.
- Ventana de retroactividad **parametrizable**: valor global más reglas que la amplían o
  reducen por persona o por rol (`/api/sistema/ventana-retroactividad`, `/api/reglas-ventana`).
  Fuera de la ventana, el empleado abre una solicitud de excepción que un Admin resuelve.
- **Restricciones de día**: días de la semana en que una persona o un rol no puede registrar
  (`ParametroRestriccionDia`). Se acumulan y cualquiera que coincida bloquea — a diferencia de
  la ventana retroactiva, donde una regla gana sobre otra. La excepción aprobada es la válvula
  de escape de ambas.
- **Reportes de usuario**: el colaborador reporta una falla o una mejora desde `/reportar`; el
  Admin las gestiona en `/admin/reportes-usuario` con estados `Nuevo → EnRevision → Resuelto`
  o `Rechazado`. Es el canal de soporte dentro del propio producto.
- Cadena de aprobación de tres niveles con estados `Pendiente → AprobadoNivel1 → AprobadoNivel2
  → Aprobado`, más `Rechazado`. El rechazo vuelve siempre al empleado y, al reenviar, la cadena
  **recomienza desde el primer nivel**.
- Estructura organizacional: puesto y jefe directo por usuario, organigrama navegable, y reglas
  de supervisor por puesto y cliente.
- Catálogos: clientes, proyectos (con `ClienteId`), empleados, lugares, modalidades y parámetros
  del sistema.
- Dashboard, reporte de horas, reporte de timesheet con exportación a Excel y PDF, e importación
  de timesheet pre-aprobado desde la plantilla del consultor. El **logo de los reportes es
  configurable** por el Admin y viaja como data URI dentro del Excel y el PDF.
- Bitácora de auditoría, notificaciones internas, perfil, recuperación de contraseña.
- Captura de horas por voz, interpretada por IA (Anthropic), con caída a un parser de reglas en
  el navegador cuando no hay API key configurada.

**Restricciones que el diseño no puede romper**

- **Un tramo horario guardado es inmutable.** Corregir un `08:00` mal digitado obliga hoy a
  borrar el registro y rehacerlo. *Decisión abierta:* permitir editarlo mientras esté Pendiente.
- **Un nivel de aprobación sin aprobador asignado no lo aprueba nadie.** No existe override de
  Admin. *Decisión abierta:* si el cliente pide un escape (supervisor de vacaciones, por
  ejemplo), hay que diseñarlo.
- 4 roles fijos: `Admin`, `Gerente`, `Supervisor`, `Empleado`. El rol decide qué se ve y qué se
  puede hacer, y el backend lo verifica en cada endpoint sin confiar en el frontend.
- El JWT vive **en memoria del cliente**, nunca en `localStorage` ni `sessionStorage` (NFR7).
  El diseño no puede asumir persistencia de sesión en el navegador.
- **Bilingüe español/inglés, con el español como idioma base.** La decisión del 2026-05-14
  (NFR21) difería el inglés a trabajo futuro, pero ya está implementado: 583 claves en
  `SharedResource.resx` y otras 583 en `SharedResource.en.resx`, con un selector que alterna la
  cultura (`KpgLanguageSelector`). **Todo texto nuevo visible entra por el localizador en los
  dos archivos; una cadena hardcodeada rompe la paridad.** Fechas `dd/MM/yyyy`, horas `HH:mm`,
  decimales con punto e `InvariantCulture` vía `KpgFormat`.
- Presupuestos de rendimiento vigentes: carga inicial del bundle WASM ≤ 5 s en red interna,
  guardado de registro < 2 s, dashboard ≤ 3 s, cualquier reporte < 10 s, 30 usuarios
  concurrentes sin degradación (NFR1–NFR5).
- El formulario de registro debe completarse **solo con teclado** (NFR19).
- Crecimiento previsto hasta 100 usuarios sin cambios estructurales (NFR17).

**Vocabulario del dominio (usarlo exactamente así en la interfaz)**

- El catálogo **"Empleados" contiene puestos, no personas** (`Consultor`, `Analista`, …). Las
  personas viven únicamente en `AspNetUsers`. Es la trampa de nombres más cara del proyecto.
- **"Recurso"** es la persona imputada en un registro, tomada del catálogo de puestos.
- **"Modalidad"** es cómo se trabajó: `Cliente` (antes "Presencial"), `Remoto` y otras activas.
  `Hibrido` está desactivada pero no borrada — los registros históricos la conservan.
- **"Día completo"** es un umbral parametrizable (`ParametrosSistema.HorasDiaCompleto`, default
  8 horas), no un valor fijo.
- **"Solicitud de excepción"** es el pedido de registrar fuera de ventana.

**Fuera del alcance de V1** (confirmado en el brief, no reabierto): integración con nómina, ERP
o facturación; módulo de vacaciones o ausencias; cálculo automático de nómina; modo offline.

## Brand Commitments

- **El logo es real.** `Fronted/src/WebUI/wwwroot/images/kpg-logo.jpg` es el logo oficial de KPG
  y se preserva.
- **La paleta no es identidad corporativa.** El navy `#0D3B5E`, el celeste `#5BB8D4` y el resto
  de la escala se eligieron durante este proyecto, no salen de un manual de marca. Son el estado
  actual y pesan como incumbente, pero pueden reemplazarse si hay una razón de diseño. No existe
  manual de marca de KPG que consultar.
- El producto se llama **KPG Timesheet**. El cliente es la propia firma: audiencia interna,
  ningún usuario externo ve estas pantallas.

## Evidence on Hand

- `_bmad-output/planning-artifacts/` — product brief, PRD (21 NFRs), épicas, arquitectura y una
  UX design specification de 844 líneas con la dirección visual original y las jornadas J1–J4.
- `Docs/manuals/` — manuales de usuario, administrador y técnico, ya entregados al cliente.
- `Docs/operations/` — checklist de go-live, guion de QA, checklist de UAT, plan de hypercare, y
  los SQL de cierre mensual y de carga de estructura inicial.
- `Docs/handoff/` y `Docs/session-logs/` — historia de decisiones sesión por sesión.
- `Docs/diagrams/` — diagrama de clases y MER.
- Datos reales en producción desde junio de 2026; el diagnóstico sobre 141 registros dio 0
  parejas cliente/proyecto inválidas.
- **Lo que NO existe y no debe inventarse:** testimonios, casos de éxito, métricas de adopción
  posteriores al go-live, benchmarks, precios y cualquier cifra de resultado. Los indicadores de
  éxito del brief son *metas*, no resultados medidos. El organigrama cargado hoy en Azure es
  data de demostración (Laura, Miguel, Juan y clientes ficticios), no la estructura real de KPG.

## Product Principles

1. **El registro es peaje, no destino.** Nadie abre esta aplicación porque quiera. Cada pantalla
   del flujo de registro se juzga por cuánto tiempo del colaborador devuelve, no por cuánto
   muestra.
2. **Exactitud antes que velocidad, pero sin elegir entre las dos.** El dato alimenta la factura:
   una validación que evita una hora mal cobrada vale la fricción; una que solo protege al
   sistema de sí mismo, no.
3. **Lo inmutable se anuncia antes de guardar, no después.** Como el registro no se puede editar,
   el momento de confirmar es el único punto donde el diseño puede evitar el error.
4. **Aprobar es decidir sobre una cola, no contemplar un ítem.** Las pantallas de supervisor se
   diseñan para despachar volumen con contexto suficiente, no para el caso de un solo registro.
5. **El estado se dice, no se colorea.** El color nunca es el único indicador: siempre lo
   acompaña un ícono o una palabra.

## Accessibility & Inclusion

No hay estándar exigido a KPG por contrato ni política: no se audita WCAG y nadie del equipo ha
declarado una necesidad específica. Lo que existe hoy es práctica propia del proyecto y se
mantiene como piso, no como obligación externa: navegación completa por teclado en el formulario
de registro (NFR19), indicadores de foco visibles, mensajes de validación que nombran el campo y
la razón (NFR20), y color nunca como único portador de estado. Los contrastes verificados en la
UX spec — navy `#0D3B5E` 9.4:1, cuerpo `#212121` 16:1, verde `#2E7D32` 5.1:1 — dan AA con
holgura; el celeste `#5BB8D4` queda en 3.0:1 y por eso se restringe a elementos gráficos o texto
≥ 18px.
