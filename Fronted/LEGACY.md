# LEGACY — Prototipo Angular (Descartado)

> **El subdirectorio `legacy-angular-prototype/` NO se utiliza en la versión 1 del producto.**

## Frontend oficial de V1

El frontend oficial de KPG Timesheet V1 es **Blazor WebAssembly**, ubicado en este mismo directorio:

```
Fronted/
├── KPG.Timesheet.WebUI.sln     ← solución Blazor WebAssembly
├── src/
│   └── WebUI/                  ← KPG.Timesheet.WebUI (Blazor WASM)
└── legacy-angular-prototype/   ← prototipo Angular descartado (no usar)
```

## Sobre el prototipo Angular

El subdirectorio `legacy-angular-prototype/` contiene un prototipo Angular generado durante la fase de exploración inicial del proyecto. Fue descartado como frontend oficial.

**No ejecutar ni desplegar** el prototipo Angular. La decisión de eliminar definitivamente ese directorio se tomará fuera del alcance de V1.

## Decisión de stack

Blazor WebAssembly fue elegido sobre Angular para:
- Mantener un único stack tecnológico (.NET / C# en todo el stack)
- Reducir la complejidad de mantenimiento (un solo lenguaje: C#)
- Integración nativa con la API backend via HttpClient typed
