using Microsoft.Extensions.Logging;
using JournalApp.Services;
using QuestPDF.Infrastructure;

namespace JournalApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register service
        builder.Services.AddSingleton<JournalService>();

        return builder.Build();
    }
}
