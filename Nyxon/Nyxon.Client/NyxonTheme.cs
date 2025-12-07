using MudBlazor;

public static class NyxonTheme
{
    private static readonly MudTheme _defaultTheme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#BB86FC",
            Secondary = "#03DAC6",
            Background = "#0F0F0F",
            Surface = "#1A1A1A",
            AppbarBackground = "#0F0F0F",
            TextPrimary = "#E0E0E0",
            TextSecondary = "#A0A0A0",
            ActionDefault = "#BB86FC",
            Error = "#CF6679"
        }
    };
    static NyxonTheme()
    {
        // Default Font (Inter)
        _defaultTheme.Typography.Default.FontFamily = new[] { "Inter", "sans-serif" };
        
        // Headers (Space Grotesk)
        _defaultTheme.Typography.H1.FontFamily = new[] { "Space Grotesk", "sans-serif" };
        _defaultTheme.Typography.H1.FontWeight = "700"; // FIX: Must be a String ("700"), not int (700)
        
        // Buttons (Inter, Semi-Bold)
        _defaultTheme.Typography.Button.FontFamily = new[] { "Inter", "sans-serif" };
        _defaultTheme.Typography.Button.FontWeight = "600"; // FIX: String
    }

    // 3. Expose it
    public static MudTheme Default => _defaultTheme;
}
