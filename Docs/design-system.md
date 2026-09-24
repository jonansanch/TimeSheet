# KPG Design System

Esta capa visual amplía MudBlazor sin sustituirlo. Los componentes de negocio, rutas y contratos continúan usando MudBlazor directamente; las primitivas `Kpg*` codifican solamente patrones visuales repetibles.

## Fundamentos

Los tokens viven en `Fronted/src/WebUI/wwwroot/css/kpg-tokens.css` y se dividen en:

- Primitivos: escalas `navy`, `blue`, `slate`, `green`, `amber`, `red` e `indigo`.
- Semánticos: `--kpg-color-primary`, `--kpg-text-*`, `--kpg-surface-*`, `--kpg-border-*` y `--kpg-status-*`.
- Escalas: `--kpg-space-1` a `--kpg-space-10`, radios de 8/10/12 px y sombras `xs/sm/md`.

El tema equivalente de MudBlazor se centraliza en `Shared/Theming/KpgTheme.cs`. Si cambia un color base, ambos archivos deben mantenerse alineados.

## Principios de uso

- Fondo de aplicación gris claro, superficies de contenido blancas y sidebar navy.
- Usar el espaciado de tokens; evitar nuevos valores arbitrarios o estilos inline.
- Preferir `KpgCard` para nuevas superficies. `Outlined` es el valor predeterminado.
- Los estados siempre incluyen texto y, cuando aporte claridad, icono. Nunca comunicar estado solo mediante color.
- Mantener áreas táctiles de al menos 40 px y foco visible.
- Respetar `prefers-reduced-motion`; la hoja global desactiva animaciones no esenciales.

## Componentes base

| Componente | Uso recomendado |
|---|---|
| `KpgPageHeader` | Título `h1`, subtítulo, breadcrumb y acciones de página. |
| `KpgCard` | Superficie estándar o compacta. |
| `KpgButton` | Acciones KPG con defaults de tamaño, radio y peso. |
| `KpgInput<T>` | Campo de texto outlined y compacto; usar MudTextField directo para capacidades avanzadas. |
| `KpgSelect<T>` | Select outlined; sus opciones siguen siendo `MudSelectItem<T>`. |
| `KpgStatusBadge` | Estados `success`, `warning`, `error`, `info` o neutral. Variantes desconocidas caen a neutral. |
| `KpgStatCard` | KPI numérico; mantiene su contrato previo. |
| `KpgDataTable<T>` | Tabla cliente sencilla con loading y empty state integrados. Para virtualización o datos de servidor usar MudDataGrid directamente. |
| `KpgEmptyState` | Ausencia de contenido con título, descripción y acción opcional. |
| `KpgTableSkeleton` | Estado de carga accesible para tablas. |

## Ejemplo

```razor
<KpgPageHeader Title="Mis registros" Subtitle="Consulta tu historial">
    <Actions>
        <KpgButton StartIcon="@Icons.Material.Filled.Add">Registrar jornada</KpgButton>
    </Actions>
</KpgPageHeader>

<KpgCard>
    <KpgStatusBadge Text="Aprobado" Status="success" Icon="@Icons.Material.Filled.CheckCircle" />
</KpgCard>
```

Los wrappers no pretenden reflejar toda la API de MudBlazor. Si una pantalla requiere una capacidad que no constituye un patrón transversal, debe utilizar el componente MudBlazor original y los tokens KPG.
