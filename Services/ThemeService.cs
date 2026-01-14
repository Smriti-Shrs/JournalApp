using Microsoft.Maui.Controls;

namespace JournalApp.Services;

public static class ThemeService
{
    private static bool _isDark;

    public static bool IsDark => _isDark;

    public static void ToggleTheme()
    {
        _isDark = !_isDark;
        ApplyTheme();
    }

    public static void SetTheme(bool isDark)
    {
        _isDark = isDark;
        ApplyTheme();
    }

    private static void ApplyTheme()
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
            return;

        if (_isDark)
        {
            resources["PageBackgroundColor"] = Color.FromArgb("#121212");
            resources["CardBackgroundColor"] = Color.FromArgb("#1E1E1E");
            resources["PrimaryTextColor"] = Colors.White;
        }
        else
        {
            resources["PageBackgroundColor"] = Color.FromArgb("#F5F5F5");
            resources["CardBackgroundColor"] = Colors.White;
            resources["PrimaryTextColor"] = Color.FromArgb("#222222");
        }
    }
}
