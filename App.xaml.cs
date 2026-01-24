namespace JournalApp;

using JournalApp.Services;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Initialize theme resources before creating window
        ThemeService.Initialize();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Title = "JournalApp"
        };
    }
}
