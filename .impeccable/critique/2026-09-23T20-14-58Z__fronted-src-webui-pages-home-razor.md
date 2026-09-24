---
target: Inicio (Home) + AppShell de KPG Timesheet
total_score: 23
max_score: 40
na_heuristics: 
p0_count: 1
p1_count: 2
target_identity: "file:C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Pages\\Home.razor"
target_fingerprint: "sha256:b96bf6f3116ec71beb73b18d6b4f05d591748d5e94d3d3ba9b8fdd8477c52fba"
target_path: "C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Pages\\Home.razor"
timestamp: 2026-09-23T20-14-58Z
slug: fronted-src-webui-pages-home-razor
---
# Inicio (Home) + AppShell — crítica de diseño

## Veredicto
Shell sólido y consistente en tokens, pero el Home —primera pantalla del usuario— no cumple lo que la "segunda pasada" prometía frente al mockup: dos tercios del panel de resumen son estados vacíos hardcodeados sin datos reales, hay un CTA duplicado y media página sin localizar.

## Heurísticas
| Heurística | Puntuación |
|---|---:|
| Visibilidad del estado | 2/4 |
| Correspondencia con el mundo real | 3/4 |
| Control y libertad | 3/4 |
| Consistencia y estándares | 2/4 |
| Prevención de errores | 3/4 |
| Reconocimiento sobre recuerdo | 3/4 |
| Flexibilidad y eficiencia | 2/4 |
| Diseño estético y minimalista | 2/4 |
| Recuperación de errores | 2/4 |
| Ayuda y documentación | 1/4 |
| **Total** | **23/40** |

## Fortalezas
- Sistema de tokens consistente (spacing, radio, sombra) en cards y chrome del layout.
- Accesibilidad base: foco visible, skip link, prefers-reduced-motion.
- IA consciente de roles vía AuthorizeView.

## Problemas prioritarios
1. **P0:** Home.razor:17-18 — dos de tres widgets de resumen ("Mi semana", "Mis horas este mes") son KpgEmptyState hardcodeado sin binding de datos; es el único estado que renderiza siempre.
2. **P1:** Home.razor:7 vs Home.razor:9 — CTA "Registrar horas" duplicado (botón de header + primera card del grid), mismo destino /registro.
3. **P1:** Home.razor:17-19 — literales en español hardcodeados pese a IStringLocalizer usado en el resto del archivo y selector de idioma activo en el layout.
4. **P2:** Home.razor:9-13 — tonos (info/success/warning) de HomeLink asignados por posición, no por significado; choca con su uso real como estado de jornada.
5. **P2:** Home.razor.css — falta regla `.home-month` (existe `.home-week` con min-height:210px pero no su hermana), posicionamiento implícito sin control de altura.
6. **P3:** MainLayout.razor:19 — texto "KPG Timesheet" del topbar con aria-hidden="true", sin verificar contra lector de pantalla real.

## Personas
- **Alex (power user):** cero valor de eficiencia; sin atajos; 2/3 widgets siempre vacíos; tras una visita empieza a navegar por URL directa.
- **Sam (accesibilidad):** 6+ h2 hermanos sin landmark que distinga "acciones" de "información".

## Observaciones
- Home.razor:6-21 escrito como markup de una sola línea por elemento; afecta mantenibilidad.
- Home.razor:7 — fecha vía DateTime.Today.ToString() sin CultureInfo explícito.
- .home-quick (caja con borde) y .home-action-card (card con icono) son dos lenguajes visuales de "tile" distintos coexistiendo.

## Detector mecánico
`impeccable detect --json` sobre Home.razor + Layout/ devolvió `[]` (cero hallazgos). Evidencia complementaria por grep: un hex sin token (#8ed8f8 en NavMenu.razor.css:18) y valores px de dimensión de componente en su mayoría legítimos (avatar/logo/bordes), no violaciones de escala de espaciado. Sin navegador disponible esta sesión — sin evidencia visual renderizada.
