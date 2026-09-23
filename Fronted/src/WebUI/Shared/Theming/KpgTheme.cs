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
            Primary = "#075985",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#0284C7",
            SecondaryContrastText = "#FFFFFF",
            Tertiary = "#0EA5E9",
            Info = "#1D4ED8",
            Success = "#15803D",
            Warning = "#B45309",
            Error = "#B91C1C",
            Background = "#F4F7FA",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#16324A",
            DrawerBackground = "#073B5C",
            DrawerText = "#E7F2F8",
            DrawerIcon = "#8ED8F8",
            TextPrimary = "#16324A",
            TextSecondary = "#5D7285",
            Divider = "#DCE5EC",
            LinesDefault = "#DCE5EC",
            ActionDefault = "#5D7285",
            ActionDisabled = "#9AAAB7",
            ActionDisabledBackground = "#E8EEF3"
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
            DrawerWidthLeft = "256px"
        }
    };
}
