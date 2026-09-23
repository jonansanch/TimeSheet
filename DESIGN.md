---
name: KPG Timesheet
description: Torre de control para el registro, aprobación y facturación de horas de una firma de consultoría.
colors:
  navy-kpg: "#0D3B5E"
  navy-hover: "#1B4F73"
  cielo-kpg: "#5BB8D4"
  verde-completo: "#2E7D32"
  ambar-pendiente: "#F9A825"
  amarillo-alerta: "#FFD300"
  naranja-info: "#F57C20"
  rojo-rechazo: "#C62828"
  morado-excepcion: "#7B1FA2"
  gris-vacio: "#E0E0E0"
  gris-inactivo: "#9E9E9E"
  fondo-app: "#F5F5F5"
  superficie: "#FFFFFF"
  texto-cuerpo: "#212121"
typography:
  display:
    fontFamily: "Inter, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif"
    fontSize: "24px"
    fontWeight: 600
    lineHeight: 1.2
  headline:
    fontFamily: "Inter, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif"
    fontSize: "20px"
    fontWeight: 600
    lineHeight: 1.3
  title:
    fontFamily: "Inter, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif"
    fontSize: "14px"
    fontWeight: 600
    lineHeight: 1.4
  body:
    fontFamily: "Inter, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif"
    fontSize: "16px"
    fontWeight: 400
    lineHeight: 1.5
  label:
    fontFamily: "Inter, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif"
    fontSize: "12px"
    fontWeight: 400
    letterSpacing: "0.3px"
rounded:
  none: "0px"
  sm: "4px"
  md: "12px"
  full: "50%"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
components:
  button-primary:
    backgroundColor: "{colors.navy-kpg}"
    textColor: "{colors.superficie}"
    rounded: "{rounded.sm}"
    padding: "6px 16px"
  button-secondary:
    backgroundColor: "{colors.superficie}"
    textColor: "{colors.navy-kpg}"
    rounded: "{rounded.sm}"
    padding: "6px 16px"
  card:
    backgroundColor: "{colors.superficie}"
    textColor: "{colors.texto-cuerpo}"
    rounded: "{rounded.md}"
    padding: "{spacing.lg}"
  chip-estado:
    backgroundColor: "{colors.verde-completo}"
    textColor: "{colors.superficie}"
    typography: "{typography.label}"
    padding: "0 10px"
    height: "24px"
  nav-link:
    backgroundColor: "{colors.navy-kpg}"
    textColor: "{colors.superficie}"
    rounded: "{rounded.none}"
    padding: "10px 20px"
  nav-link-active:
    backgroundColor: "{colors.navy-hover}"
    textColor: "{colors.superficie}"
    rounded: "{rounded.none}"
    padding: "10px 20px"
---

# Design System: KPG Timesheet

## Overview

**Creative North Star: "La Torre de Control"**

Esta no es una aplicación que se contempla: es una torre desde la que se despacha. Quien la
abre necesita saber, antes de leer una sola palabra, qué falta, qué espera aprobación y qué
está listo para facturar. Todo el sistema visual está subordinado a esa lectura instantánea:
el color codifica estado, la posición codifica urgencia, y la tipografía mantiene alineadas
columnas de horas que alguien va a convertir en una factura.

El carácter no viene de decorar. Viene de la precisión: un semáforo de estados que nunca
miente, cifras que alinean, y superficies que se distinguen sin gritar. La navegación es un
bloque navy sólido que ancla la pantalla por el costado izquierdo; el área de trabajo es gris
claro con tarjetas blancas flotando apenas sobre él. Nada compite con el dato.

El sistema rechaza dos cosas de forma explícita. Rechaza el color decorativo: si algo está
coloreado, significa algo, y el resto es neutro. Y rechaza el estado comunicado solo por
color: cada chip lleva su palabra, cada punto del calendario lleva su leyenda.

**Key Characteristics:**
- Navy institucional como marco permanente; el área de trabajo nunca se tiñe
- Semáforo de estados de cinco colores, cada uno con significado único y exclusivo
- Superficies blancas con esquinas de 12px, levantadas apenas (sombra suave)
- Contorno como voz por defecto de los botones; el relleno se reserva para guardar y confirmar
- Densidad alta tolerada: grillas de 50 filas, calendarios de 42 celdas, sin aire decorativo

## Colors

Una paleta institucional fría (navy y celeste) sobre la que se monta un semáforo semántico de
cinco estados. El navy es marco, el celeste es señal de navegación, y todo lo demás es estado.

### Primary
- **Navy KPG** (`#0D3B5E`): la identidad del sistema. Barra superior, sidebar completo, botones
  de acción primaria, anillo del día seleccionado en el calendario, y chip de los estados de
  aprobación intermedios (niveles 1/3 y 2/3). Contraste 9.4:1 sobre blanco.
- **Navy Hover** (`#1B4F73`): un solo paso más claro, exclusivo del hover y del estado activo
  en la navegación. Nunca aparece como color de fondo de una superficie.

### Secondary
- **Celeste KPG** (`#5BB8D4`): la señal dentro del navy. Íconos del sidebar, barra lateral de
  3px del ítem activo, flechas de expansión, barra de progreso de la carga inicial. Contraste
  3.0:1 sobre blanco — **solo elementos gráficos o texto ≥ 18px**, nunca texto de cuerpo.

### Tertiary — el semáforo de estados
Cada color tiene un único significado en todo el sistema y no se reutiliza para otra cosa.

- **Verde Completo** (`#2E7D32`): día completo en el calendario, registro aprobado en los tres
  niveles, catálogo activo. Contraste 5.1:1.
- **Ámbar Pendiente** (`#F9A825`): día con registro pero por debajo del umbral, y registro
  esperando la primera revisión. Es el color de "esto te espera a vos".
- **Rojo Rechazo** (`#C62828`): registro rechazado que volvió al empleado.
- **Morado Excepción** (`#7B1FA2`): excepción de ventana aprobada. **Siempre como anillo, nunca
  como relleno** — el fondo del día ya está diciendo si está completo.
- **Amarillo Alerta** (`#FFD300`) y **Naranja Info** (`#F57C20`): reservados a avisos del
  sistema (`Warning` e `Info` del tema). No participan del estado de un registro.
- **Ámbar Texto** (`#856500`): el amarillo de arriba no se puede usar como texto — sobre
  blanco da 1.4:1. Este es su par legible, y existe solo para eso: el día retroactivo del
  `KpgDatePicker` lo usa como color de texto sobre un fondo `rgba(255,211,0,0.25)`.

El **`KpgDatePicker` tiene su propio semáforo**, distinto del calendario y con otro
significado: verde `#2E7D32` sobre `rgba(46,125,50,0.15)` = fecha dentro de la ventana,
ámbar = fecha retroactiva que todavía se puede registrar, gris `#9E9E9E` = fecha bloqueada.
Aquí el color habla de **permiso**, no de completitud.

### Neutral
- **Fondo App** (`#F5F5F5`): el lienzo sobre el que flotan las tarjetas.
- **Superficie** (`#FFFFFF`): tarjetas, diálogos, grillas, popovers.
- **Texto Cuerpo** (`#212121`): todo el texto de lectura. Contraste 16:1.
- **Gris Vacío** (`#E0E0E0`): día sin registro en el calendario. Ausencia, no error.
- **Gris Inactivo** (`#9E9E9E`): elemento de catálogo desactivado. Se eligió explícito porque
  el `Color.Default` de MudBlazor se ve casi transparente y no se lee como inactivo.

### Named Rules

**La Regla del Color con Significado.** Si algo está coloreado, significa algo. Navy y celeste
son marco y navegación; los cinco colores del semáforo son estado. **No existe color
decorativo en este sistema** — ni en encabezados, ni en vacíos, ni en hitos. Un color nuevo
solo entra si trae un significado nuevo que ningún color existente ya ocupa.

**La Regla del Anillo.** El relleno de una celda comunica completitud; los anillos comunican
todo lo demás (excepción aprobada en morado, día seleccionado en navy). Dos anillos pueden
superponerse sin competir; dos rellenos, no.

**La Regla de la Palabra.** El color nunca es el único portador del estado. Todo chip lleva su
texto ("Aprobado 3/3", "Rechazado"), todo punto del calendario tiene su entrada en la leyenda,
y todo ícono de estado tiene su tooltip.

## Typography

**Display / Body / Label Font:** Inter (con fallback a Segoe UI, Roboto, Helvetica Neue, Arial)

**Character:** una sola voz para todo el sistema. Inter se elige por sus cifras tabulares: en
una aplicación donde la mayoría de la pantalla son columnas de horas, minutos y totales, que
los números alineen verticalmente no es estética, es legibilidad del dato que se va a
facturar. No hay fuente de display separada: la jerarquía se construye con peso y tamaño, no
con un segundo tipo.

Inter está **auto-hospedada** en `wwwroot/fonts/` como archivo variable (400–700) en dos
subsets: `inter-latin.woff2` (48 KB) e `inter-latin-ext.woff2` (85 KB), declarados con
`font-display: swap` y su `unicode-range`. El subset latin va con `<link rel="preload">`
porque la pantalla de carga ya muestra texto antes de que se parsee `app.css`. No se usa un
CDN externo: la red es corporativa interna y el presupuesto de carga inicial es de 5 s (NFR1).
El MIME `font/woff2` está declarado en `staticwebapp.config.json` y en `web.config` — con
`X-Content-Type-Options: nosniff` activo, un MIME incorrecto hace que el navegador descarte la
fuente en silencio.

### Hierarchy
- **Display / H4** (600, 24px): título de pantalla y cifra grande de las tarjetas de estadística.
- **Headline / H5** (600, 20px): título de sección y de diálogo.
- **Title / Subtitle1** (600, 14px): encabezado de tarjeta, mes del calendario, etiqueta de grupo.
- **Body / Body1** (400, 16px): texto de lectura y valores de formulario. Body2 (14px) para
  texto secundario dentro de una tarjeta.
- **Label / Caption** (400, 12px, +0.3px de tracking): leyendas, ayudas, días de la semana,
  metadatos de fila.

### Named Rules

**La Regla de Una Sola Voz.** Un único tipo para todo el sistema. Poppins estaba declarada en
31 lugares — el título de casi toda pantalla — y se eliminó por completo. Nunca se cargó, así
que esos títulos venían resolviendo al mismo fallback que el resto: la regla no cambió cómo se
ven, cerró la puerta a que divergieran. Un título de pantalla no lleva `font-family` propia.

**La Regla de la Cifra Alineada.** Toda columna de horas, minutos o totales se renderiza con
cifras tabulares (`font-variant-numeric: tabular-nums`) y alineada a la derecha. Un total que
no alinea con el de la fila de arriba obliga a leer dígito por dígito.

## Layout

El armazón es fijo: barra superior de 64px en navy, sidebar navy de ancho MudBlazor por
defecto que colapsa en el breakpoint `Md`, y área de contenido con padding de 24px
(`py-6 px-6`) sobre fondo gris.

**El ancho del contenido depende de la pantalla, y es una lista explícita, no una regla
automática.** Las pantallas de grilla ancha — `/mis-registros`, `/aprobaciones`, `/reportes`,
`/admin/bitacora`, `/admin/usuarios`, `/solicitudes-excepcion`, `/mis-solicitudes` — usan el
100% del ancho, porque a 1200px el renglón se corta y obliga a scroll horizontal. Todo lo
demás se topa en 1200px, porque un formulario o un párrafo a 2000px se lee mal. Agregar una
ruta a esa lista debe ser una decisión consciente.

**Ritmo de espaciado:** la escala de MudBlazor en pasos de 4px. Lo que el sistema realmente
usa es `pa-4` (16px) para tarjetas, `pa-3` (12px) para tarjetas densas como el calendario,
`mb-2` (8px) entre elementos apilados y `mb-4` (16px) entre bloques. El calendario usa una
grilla de 7 columnas con `gap: 2px`.

**Responsive:** el escritorio es el escenario real, pero el móvil ya ocurre. El sidebar colapsa
solo en `Md`; abajo de ese punto la barra superior cambia el menú de usuario por un botón de
salida directo (`d-none d-sm-block` / `d-sm-none`). Las grillas anchas son el punto débil
conocido en pantalla chica.

### Named Rules

**La Regla del Ancho Declarado.** Ninguna pantalla decide su ancho por accidente. O está en la
lista de rutas anchas de `MainLayout`, o se topa en 1200px.

## Elevation & Depth

Sistema de sombra suave, heredado de Material a través de MudBlazor y confirmado como la
doctrina del sistema. Las superficies blancas flotan apenas sobre el fondo gris: `Elevation="2"`
es el valor por defecto de una tarjeta de contenido (23 usos), `Elevation="1"` el de una
superficie de apoyo como el calendario o la barra superior (16 usos), y `Elevation="0"` se
reserva al sidebar, que no flota porque es estructura.

La profundidad no se usa para jerarquizar dentro de una pantalla: dos tarjetas hermanas tienen
la misma elevación siempre. Lo que sube de nivel es lo que realmente flota sobre el contenido:
diálogos, menús y popovers, donde MudBlazor aplica sus valores altos.

### Named Rules

**La Regla de la Elevación Plana entre Hermanos.** Dos superficies del mismo rango tienen la
misma sombra. Si una necesita destacarse, se destaca por posición, tamaño o color de estado —
nunca subiendo un escalón de sombra.

## Shapes

Lenguaje de esquina suave y consistente: **12px es el radio del sistema** (31 usos) y aplica a
toda superficie de contenido — tarjetas, paneles, diálogos. Los controles heredan el radio
pequeño de MudBlazor (4px): botones, campos, chips. El círculo completo (`50%`) queda para
puntos de leyenda, avatares y los días del calendario.

La excepción deliberada es la navegación: **los ítems del sidebar tienen radio 0**. El bloque
navy se lee como una superficie continua, no como una pila de píldoras, y la barra celeste de
3px del ítem activo necesita un borde recto contra el cual apoyarse.

Los bordes son escasos. La separación entre superficies la hace el contraste de fondo (blanco
sobre gris), no una línea.

## Components

### Buttons
- **Shape:** esquina suave de 4px, heredada del control base.
- **Voz por defecto: contorno.** El outline es la forma normal de un botón en este sistema
  (134 usos contra 58 llenos). No es timidez: es que en una pantalla de despacho casi toda
  acción es una opción, no *la* acción.
- **Primary (relleno navy `#0D3B5E`, texto blanco):** exclusivo de guardar y confirmar. Es el
  botón que cierra una transacción, y por eso el relleno se siente como una decisión.
- **Secondary (contorno navy):** todo lo demás — filtrar, exportar, abrir un diálogo, navegar.
- **Text:** acciones terciarias dentro de una fila o un diálogo (cancelar, limpiar).
- **Estado:** transición de fondo de 0.15s. Todo `MudIconButton` va envuelto en `MudTooltip`,
  reusando el texto de su `aria-label`.

### Chips (estado)
- **Style:** relleno sólido del color de estado, texto blanco, peso 600, tamaño `Small`.
- **Siempre con texto y con tooltip.** `KpgEstadoChip` para catálogos (verde activo / gris
  inactivo) y `KpgEstadoAprobacionChip` para la cadena, que además cuenta el avance:
  "Nivel 1 1/3", "Nivel 2 2/3", "Aprobado 3/3".
- Los colores van escritos explícitos y no por `Color.Default`, que se ve casi transparente
  sobre el fondo de la grilla.

### Cards / Containers
- **Corner Style:** 12px.
- **Background:** blanco sobre el gris de la aplicación.
- **Shadow Strategy:** `Elevation="2"` de contenido, `Elevation="1"` de apoyo (ver Elevation).
- **Internal Padding:** 16px (`pa-4`); 12px (`pa-3`) cuando la densidad lo exige.
- **Signature:** `KpgStatCard` — ícono + etiqueta en `body2` arriba, cifra en `h4` peso 700
  abajo. La cifra es el elemento, la etiqueta es el pie.

### Inputs / Fields
- **Style:** `Variant.Outlined` como norma absoluta en formularios (21 de 26 usos en el módulo
  de registro). Borde visible, fondo blanco, radio 4px.
- **Focus:** el anillo nativo de MudBlazor. El único foco custom del sistema es el del
  radiogroup de modalidad: `outline: 2px solid #0D3B5E` con `outline-offset: 3px`, porque un
  grupo de radios no tiene un borde propio sobre el cual dibujarse.
- **Autofill neutralizado:** el fondo azul del autocompletado del navegador se anula con
  `box-shadow: inset 0 0 0 9999px #ffffff`, para que un campo recordado no parezca un campo
  con error.
- **Horas:** `step="900"` (cuartos de hora) **más** validación de `Minutes % 15`, porque el
  atributo `step` no impide teclear un valor libre.

### Navigation
- **Sidebar navy sólido** (`#0D3B5E`), sin borde derecho, sin elevación. Ítems de 0.9rem con
  padding `10px 20px` y radio 0.
- **Hover:** fondo `#1B4F73`. **Activo:** fondo `rgba(255,255,255,0.12)` + barra izquierda de
  3px en celeste + peso 500 + ícono que pasa de celeste a blanco.
- **Hijos:** indent de 36px, 0.85rem, opacidad 0.88, sin ícono — el espacio reservado se oculta
  para que el texto arranque donde debe.
- **Logo:** `kpg-logo.jpg` en la cabecera del drawer con `mix-blend-mode: lighten`, que lo
  integra al navy sin necesitar una versión transparente del archivo.

### KpgMiniCalendario (signature)
El componente que define el sistema. Una grilla de 7 columnas donde **el relleno codifica
completitud y los anillos codifican todo lo demás**: verde `#2E7D32` día completo, ámbar
`#F9A825` incompleto, gris `#E0E0E0` sin registro, `#E3F2FD` para hoy sin registro; anillo
morado para excepción aprobada, anillo navy para el día seleccionado. Domingos y futuro
deshabilitados al 30% de opacidad. Leyenda siempre visible bajo la grilla, y tooltip con el
detalle en cada día con dato. El umbral de "completo" no es 8 horas: es el parámetro
`HorasDiaCompleto`.

### KpgSaveConfirmationBanner (signature)
La contracara de la inmutabilidad. Tras guardar, un panel `role="status"` con `aria-live="polite"`
repite lo que quedó grabado — fecha, cada bloque horario como `H1: 08:00–12:00`, cliente y
proyecto — porque el registro ya no se puede editar y la confirmación es la última oportunidad
de detectar el error.

## Do's and Don'ts

### Do:
- **Do** usar 12px de radio en toda superficie de contenido y 0px en los ítems del sidebar.
- **Do** acompañar todo color de estado con su palabra y su tooltip.
- **Do** poner `Variant.Outlined` como opción por defecto de un botón nuevo, y reservar el
  relleno navy para el botón que guarda o confirma.
- **Do** declarar el ancho de una pantalla nueva: o entra en la lista de rutas anchas de
  `MainLayout`, o vive dentro de los 1200px.
- **Do** renderizar toda columna de horas o totales con cifras tabulares y alineada a la derecha.
- **Do** pasar todo texto visible nuevo por el localizador, en `SharedResource.resx` **y** en
  `SharedResource.en.resx`. Una cadena hardcodeada rompe la paridad de 583 claves.
- **Do** escribir los colores de estado explícitos en vez de apoyarse en `Color.Default`.

### Don't:
- **Don't** introducir color decorativo. Un color nuevo solo entra con un significado nuevo.
- **Don't** usar el celeste `#5BB8D4` en texto menor a 18px: queda en 3.0:1 de contraste.
- **Don't** reutilizar un color del semáforo para otra cosa. El morado es excepción aprobada y
  nada más; el ámbar es "te espera a vos" y nada más.
- **Don't** comunicar la excepción con relleno: es anillo, porque el fondo ya dice completitud.
- **Don't** subir un escalón de sombra para destacar una tarjeta entre sus hermanas.
- **Don't** agregar una segunda familia tipográfica. Una sola voz, jerarquía por peso y tamaño.
- **Don't** cargar fuentes desde un CDN externo: red corporativa interna y 5 s de presupuesto
  de carga inicial.
- **Don't** escribir `Text="@L["Key"]"` en un `MudTooltip`: las comillas anidadas dan el error
  RZ9986. Va `Text="@(L["Key"].Value)"`.
