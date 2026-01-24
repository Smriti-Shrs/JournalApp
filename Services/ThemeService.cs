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

    public static Task SetThemeAsync(bool isDark)
    {
        SetTheme(isDark);
        return Task.CompletedTask;
    }

    public static void Initialize()
    {
        ApplyTheme();
    }

    public static Task InitializeAsync()
    {
        ApplyTheme();
        return Task.CompletedTask;
    }

    private static void ApplyTheme()
    {
        var resources = Application.Current?.Resources;
        if (resources is null)
            return;

        if (_isDark)
        {
            resources["PageBackgroundColor"] = Color.FromArgb("#0f172a");
            resources["CardBackgroundColor"] = Color.FromArgb("#1e293b");
            resources["PrimaryTextColor"] = Color.FromArgb("#f8fafc");
            resources["SecondaryTextColor"] = Color.FromArgb("#94a3b8");
            resources["PrimaryColor"] = Color.FromArgb("#6366f1");
            resources["BorderColor"] = Color.FromArgb("#334155");
        }
        else
        {
            resources["PageBackgroundColor"] = Color.FromArgb("#f8fafc");
            resources["CardBackgroundColor"] = Colors.White;
            resources["PrimaryTextColor"] = Color.FromArgb("#0f172a");
            resources["SecondaryTextColor"] = Color.FromArgb("#6b7280");
            resources["PrimaryColor"] = Color.FromArgb("#6366f1");
            resources["BorderColor"] = Color.FromArgb("#e2e8f0");
        }
    }
}
