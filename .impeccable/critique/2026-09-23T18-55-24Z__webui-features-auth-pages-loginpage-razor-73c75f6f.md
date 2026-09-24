---
target: login de KPG Timesheet
total_score: 28
max_score: 40
na_heuristics: 
p0_count: 0
p1_count: 2
target_identity: "file:C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Auth\\Pages\\LoginPage.razor"
target_fingerprint: "sha256:6674653d3242350a01d34dae47596aca36d32ed0ea4002395fe21d5fadc6a088"
target_path: "C:\\Personal\\TimeSheet\\Fronted\\src\\WebUI\\Features\\Auth\\Pages\\LoginPage.razor"
timestamp: 2026-09-23T18-55-24Z
slug: webui-features-auth-pages-loginpage-razor-73c75f6f
---
# Login — crítica de diseño

## Veredicto

La pantalla es funcional, limpia y coherente con el sistema KPG, pero se aparta del mockup en composición e identidad. Usa una división 40/60 en lugar de una presencia corporativa cercana a 45/55; el panel izquierdo combina degradado y geometría genérica, por lo que se siente más como plantilla SaaS que como una expresión inequívoca de KPG.

## Heurísticas

| Heurística | Puntuación |
|---|---:|
| Visibilidad del estado | 3/4 |
| Correspondencia con el mundo real | 4/4 |
| Control y libertad | 3/4 |
| Consistencia y estándares | 3/4 |
| Prevención de errores | 2/4 |
| Reconocimiento sobre recuerdo | 3/4 |
| Flexibilidad y eficiencia | 3/4 |
| Diseño estético y minimalista | 3/4 |
| Recuperación de errores | 2/4 |
| Ayuda y documentación | 2/4 |
| **Total** | **28/40** |

## Fortalezas

- Flujo enfocado con solo dos campos y un CTA principal.
- Jerarquía clara y uso consistente de MudBlazor y tokens KPG.
- Responsive source-level sólido mediante grid, `min-width:0` y formulario limitado a 440 px.

## Problemas prioritarios

1. **P1:** el toggle de contraseña no expone nombre accesible ni estado anunciado.
2. **P1:** ayuda y copyright usan `#9AAAB7` sobre blanco, contraste medido 2.38:1.
3. **P2:** mensajes required y error de servidor no indican una recuperación concreta.
4. **P2:** loading carece de `aria-busy` o nombre accesible para el progreso.
5. **P2:** en móvil, marca e idioma consumen espacio antes del formulario; el target del idioma no demuestra 44 px.
6. **P3:** degradado, figura rotada y proporción 40/60 difieren de la dirección visual del mockup.

## Personas

- **Jordan:** entiende el acceso, pero el contacto con administración no es accionable.
- **Sam:** labels y foco base ayudan; toggle, contraste muted y loading requieren corrección.
- **Casey:** formulario breve y autocomplete funcionan; la franja de marca móvil debe compactarse.

## Observaciones

- El panel corporativo tiene textos hardcodeados en español aunque existe selector de idioma.
- La espera fija de 800 ms tras autenticar agrega latencia artificial.
- El JPG del logo puede perder nitidez frente a un asset transparente de mayor calidad.
