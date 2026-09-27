using MudBlazor;

namespace PruebaOben.Shared.Services;

public static class AppTheme
{
    public static MudTheme Current { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#173B57",
            Secondary = "#1D8A8A",
            Tertiary = "#E7A33E",
            Background = "#F3F6F8",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#173B57",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#334155",
            TextPrimary = "#192B3A",
            TextSecondary = "#64748B",
            Divider = "#E4EAF0",
            LinesDefault = "#E4EAF0",
            Success = "#23856D",
            Warning = "#B7791F",
            Error = "#C2414B",
            Info = "#2878A5"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "260px"
        }
    };
}
