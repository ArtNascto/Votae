using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using SQLitePCL;
using Votae.Interfaces.Services;
using Votae.Services;

namespace Votae;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Batteries_V2.Init();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        var appDataPath = FileSystem.AppDataDirectory;
        builder.Services.AddSingleton<IDataService>(sp =>
            new DataService(appDataPath));

        return builder.Build();
    }
}