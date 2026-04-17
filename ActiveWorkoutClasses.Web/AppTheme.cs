using MudBlazor;

namespace ActiveWorkoutClasses.Web;

public static class AppTheme
{
    public static MudTheme FitnessTheme => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#FF6B35",
            Secondary = "#00BCD4",
            Background = "#F5F5F5",
            Surface = "#FFFFFF",
            AppbarBackground = "#1A1C23",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#1A1C23",
            DrawerText = "#E0E0E0",
            DrawerIcon = "#FF6B35",
            TextPrimary = "#212121",
            TextSecondary = "#757575",
            ActionDefault = "#757575",
            Success = "#4CAF50",
            Warning = "#FF9800",
            Error = "#F44336",
            Info = "#00BCD4",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#FF6B35",
            Secondary = "#00BCD4",
            Background = "#0D0F14",
            Surface = "#1A1C23",
            AppbarBackground = "#111318",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#111318",
            DrawerText = "#E0E0E0",
            DrawerIcon = "#FF6B35",
            TextPrimary = "#E0E0E0",
            TextSecondary = "#90A4AE",
            ActionDefault = "#90A4AE",
            Success = "#4CAF50",
            Warning = "#FF9800",
            Error = "#F44336",
            Info = "#00BCD4",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "sans-serif"] },
            H1 = new H1Typography { FontFamily = ["Barlow Condensed", "sans-serif"], FontWeight = "700" },
            H2 = new H2Typography { FontFamily = ["Barlow Condensed", "sans-serif"], FontWeight = "700" },
            H3 = new H3Typography { FontFamily = ["Barlow Condensed", "sans-serif"], FontWeight = "600" },
            H4 = new H4Typography { FontFamily = ["Barlow Condensed", "sans-serif"], FontWeight = "600" },
            H5 = new H5Typography { FontFamily = ["Barlow Condensed", "sans-serif"], FontWeight = "600" },
            H6 = new H6Typography { FontFamily = ["Barlow Condensed", "sans-serif"], FontWeight = "600" },
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "8px" }
    };
}
