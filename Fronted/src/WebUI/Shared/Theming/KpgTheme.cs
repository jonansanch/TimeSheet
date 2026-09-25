using MudBlazor;

namespace KPG.Timesheet.WebUI.Shared.Theming;

/// <summary>
/// Tema visual global de KPG. Los valores deben mantenerse alineados con
/// wwwroot/css/kpg-tokens.css; MudBlazor consume esta clase y el CSS consume
/// los tokens semánticos.
/// </summary>
public static class KpgTheme
{
    private static readonly string[] SansFontStack =
    [
        "Inter",
        "Segoe UI",
        "Roboto",
        "Helvetica Neue",
        "Arial",
        "sans-serif"
    ];

    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#0B466A",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#087EA4",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#3B82F6",
            Info = "#2563EB",
            Success = "#15803D",
            Warning = "#F59E0B",
            WarningContrastText = "#172B3A",
            Error = "#DC2626",
            Background = "#F6F8FA",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#172B3A",
            DrawerBackground = "#073B5C",
            DrawerText = "#E6EEF4",
            DrawerIcon = "#B9CCDA",
            TextPrimary = "#172B3A",
            TextSecondary = "#64748B",
            Divider = "#E5EAF0",
            LinesDefault = "#E5EAF0",
            ActionDefault = "#64748B",
            ActionDisabled = "#8492A6",
            ActionDisabledBackground = "#EEF2F6"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = SansFontStack, FontSize = "1rem", LineHeight = "1.5" },
            H1 = new H1Typography { FontFamily = SansFontStack, FontSize = "2rem", FontWeight = "700", LineHeight = "1.2" },
            H2 = new H2Typography { FontFamily = SansFontStack, FontSize = "1.75rem", FontWeight = "700", LineHeight = "1.25" },
            H3 = new H3Typography { FontFamily = SansFontStack, FontSize = "1.5rem", FontWeight = "700", LineHeight = "1.3" },
            H4 = new H4Typography { FontFamily = SansFontStack, FontSize = "1.25rem", FontWeight = "700", LineHeight = "1.35" },
            H5 = new H5Typography { FontFamily = SansFontStack, FontSize = "1.125rem", FontWeight = "650", LineHeight = "1.4" },
            H6 = new H6Typography { FontFamily = SansFontStack, FontSize = "1rem", FontWeight = "650", LineHeight = "1.4" },
            Body1 = new Body1Typography { FontFamily = SansFontStack, FontSize = "1rem", LineHeight = "1.5" },
            Body2 = new Body2Typography { FontFamily = SansFontStack, FontSize = "0.875rem", LineHeight = "1.5" },
            Button = new ButtonTypography { FontFamily = SansFontStack, FontSize = "0.875rem", FontWeight = "600", TextTransform = "none" },
            Caption = new CaptionTypography { FontFamily = SansFontStack, FontSize = "0.75rem", LineHeight = "1.4" },
            Subtitle1 = new Subtitle1Typography { FontFamily = SansFontStack, FontSize = "0.9375rem", FontWeight = "600" },
            Subtitle2 = new Subtitle2Typography { FontFamily = SansFontStack, FontSize = "0.8125rem", FontWeight = "600" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "280px"
        }
    };
}
