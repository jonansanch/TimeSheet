# Segunda pasada de fidelidad visual

## Autoridad y criterio

El mockup entregado es la autoridad visual. La aplicación activa, sus APIs y sus modelos son la autoridad funcional. La segunda pasada reemplaza composiciones heredadas sin alterar rutas, permisos, validaciones, estados ni contratos.

## Brechas transversales

**CURRENT:** shell correcto pero espacioso; contenido variable, controles altos, varias páginas construidas como formularios o tablas heredadas con nueva paleta. Sidebar de 256 px, topbar de 64 px y abundante CSS local/inline.

**TARGET:** producto SaaS corporativo denso, sidebar de aproximadamente 220–240 px, topbar de 60–64 px, contenido centrado de 1280–1440 px, títulos de 22–26 px, controles de 38–42 px, cards de radio 10–12 px, bordes claros y sombras mínimas.

**GAP:** faltan reglas de composición compartidas; el Design System cubre apariencia base pero no organiza toolbars, KPIs, bandejas, formularios compactos ni jerarquías de datos.

**CHANGES REQUIRED:** ajustar tokens y AppShell; introducir clases/patrones reutilizables para page container, KPI strip, filter bar, card section, datatable y acciones compactas; eliminar estilos inline repetidos; mantener foco, teclado, estados semánticos e i18n.

## Login

**CURRENT:** panel 45/55, logo dentro de un cuadro oscuro, formulario centrado pero rodeado de espacio excesivo y pie de ayuda adicional.

**TARGET:** panel de marca 34–40%, logo integrado, beneficios compactos, formulario de 360–420 px centrado verticalmente y selector de idioma discreto.

**GAP:** proporción, escala del logo, densidad y alineación no corresponden al mockup. No existe asset arquitectónico limpio ni funcionalidad “Recordarme”.

**CHANGES REQUIRED:** recomponer columnas y bloque de marca usando solo assets existentes; eliminar el recuadro visual del logo; compactar campos y textos; conservar recuperación, errores, visibilidad de contraseña e idioma. No añadir checkbox falso ni imagen externa.

## Inicio

**CURRENT:** saludo genérico y cuatro cards grandes dependientes del rol; no existe resumen operativo.

**TARGET:** header con nombre, fecha y CTA; cuatro action cards compactas; segunda fila con semana, horas del mes y accesos recientes.

**GAP:** la composición sigue siendo el home anterior. El claim `name` está disponible, pero Home no carga aún datos de jornada o dashboard.

**CHANGES REQUIRED:** crear cabecera y cuadrícula del target; derivar acciones por permisos; cargar únicamente resúmenes disponibles mediante repositorios existentes y mostrar estados vacíos honestos cuando no haya datos; no hardcodear cifras o clientes.

## Registro

**CURRENT:** calendario mensual ocupa la columna izquierda; tres bloques horarios grandes en un formulario vertical.

**TARGET:** fecha compacta arriba; card de tramos en filas Entrada/Salida/Duración/Acción; sugerencias recientes secundarias; información del registro en grid compacto.

**GAP:** estructura, densidad y prioridad visual difieren sustancialmente. La lógica existente admite hasta tres tramos, retroactividad, voz, sugerencias y upsert.

**CHANGES REQUIRED:** retirar el calendario grande del layout principal sin perder selección/semáforo; representar tramos como filas y calcular duración/total; conservar reglas de 15 minutos, ventanas, excepciones, máximo de bloques, catálogos, voz y guardado; organizar datos y acciones según el mockup.

## Mis Registros

**CURRENT:** filtros mínimos y tabla ancha con tres columnas de horario, recurso y firmas visibles bajo el estado.

**TARGET:** cuatro KPI compactos, filter bar completa y tabla con Fecha/Horario/Horas/Cliente/Proyecto/Modalidad/Estado/Acciones.

**GAP:** exceso de columnas y ruido de aprobadores; falta lectura agregada.

**CHANGES REQUIRED:** calcular KPIs y duración desde datos cargados cuando sea posible; agrupar rangos horarios en una celda; mover firmas a tooltip/popover accesible; preservar paginación servidor, edición permitida, reenvío, eliminación y estados finales.

## Reportar falla o mejora

**CURRENT:** formulario y tabla extendidos a casi todo el ancho; campos y botón sobredimensionados.

**TARGET:** contenedor de 900–1000 px, card compacta, selector claro y tabla de reportes debajo.

**GAP:** anchura, densidad y alineación de acciones.

**CHANGES REQUIRED:** limitar ancho, agrupar controles, alinear CTA y normalizar tabla/badges; mantener validaciones 200/2000, API, estados y respuesta accesible.

## Dashboard

**CURRENT:** KPI superiores seguidos por ocho cards de empleado y gráficos gerenciales extensos.

**TARGET:** KPI strip y primera vista en tres columnas: lista del equipo, distribución de horas y pendientes críticos.

**GAP:** las cards individuales dominan el viewport y desplazan la información comparativa.

**CHANGES REQUIRED:** convertir miembros en lista compacta con avatar, puesto, horas y estado; reutilizar ApexCharts en un panel compacto; mostrar pendientes como lista de atención; mantener selector de fecha, permisos y secciones gerenciales/admin debajo.

## Aprobaciones

**CURRENT:** datos agrupados por empleado/día con controles por grupo y toggle de revisados.

**TARGET:** bandeja con tabs de estado, toolbar de filtros, selección múltiple y tabla operativa.

**GAP:** falta jerarquía de bandeja y densidad del mockup. El backend sigue aprobando/rechazando por registro y nivel.

**CHANGES REQUIRED:** presentar tabs derivados de estados disponibles; compactar grupos en filas seleccionables sin perder el contexto del día; habilitar acciones por selección solo para registros accionables; conservar niveles, comentarios obligatorios, revertir, reenviar y permisos.

## Organigrama

**CURRENT:** árbol recursivo como lista indentada con iconos y texto.

**TARGET:** nodos visuales conectados con avatar/iniciales, nombre, puesto, rol y cantidad de reportes.

**GAP:** no comunica jerarquía espacialmente y no permite plegar ramas.

**CHANGES REQUIRED:** reutilizar el árbol construido en frontend y renderizar nodos/cards con conectores CSS; añadir expand/collapse accesible; conservar raíces auto-supervisadas y huérfanos, y proteger el render ante ciclos.

## Usuarios

**CURRENT:** tabla genérica muy ancha, búsqueda local y numerosas acciones icon-only.

**TARGET:** page header con CTA, toolbar dentro de card, filas compactas, badges y acciones agrupadas.

**GAP:** densidad, descubribilidad y consistencia de acciones.

**CHANGES REQUIRED:** adoptar patrón administrativo común; priorizar nombre/email/rol/estructura/estado y agrupar acciones secundarias; preservar creación, estructura, rol, estado, eliminación y reset de contraseña.

## Recursos

**CURRENT:** tabla funcional de nombre/estado/acciones con filtro sencillo, pero visualmente heredada.

**TARGET:** composición exacta de administración: header, CTA, buscador centrado, tabla compacta y paginación sobria.

**GAP:** toolbar, padding, headers, estados y acciones no alcanzan la fidelidad objetivo.

**CHANGES REQUIRED:** convertir esta página en referencia del patrón admin y propagarlo; conservar CRUD/toggle y la semántica actual del catálogo.

## Supervisores

**CURRENT:** tabla de reglas puesto/cliente/aprobador/estado con diálogo propio.

**TARGET:** mismo lenguaje visual de Recursos y Usuarios.

**GAP:** ausencia de patrón compartido y acciones poco jerarquizadas.

**CHANGES REQUIRED:** reutilizar header/toolbar/card/table/empty state administrativos; mantener la regla general por cliente nulo, edición restringida, catálogos y endpoints.

## Validación prevista

- Build de la solución WebUI tras cada grupo lógico.
- Smoke visual y funcional con perfiles Empleado, Supervisor/Gerente y Admin.
- Capturas de 1440 × 900 y 390 px para la ronda final; comprobación adicional en 1280 y 1920 px.
- Verificación de loading, error, vacío, textos largos, ES/EN, teclado, foco y permisos.
- Detector mecánico Impeccable una sola vez al finalizar los cambios visuales.
