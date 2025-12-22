using Microsoft.Extensions.Logging;
using XmlLibraryLab2.Services;
using XmlLibraryLab2.Strategies;
using XmlLibraryLab2.ViewModels;
using XmlLibraryLab2.Views;

namespace XmlLibraryLab2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<IXmlMetadataService, XmlMetadataService>();
        builder.Services.AddSingleton<IXmlTransformService, XsltTransformService>();
        builder.Services.AddSingleton<IStrategyFactory, StrategyFactory>();

        // Strategies
        builder.Services.AddSingleton<IXmlSearchStrategy, SaxXmlSearchStrategy>();
        builder.Services.AddSingleton<IXmlSearchStrategy, DomXmlSearchStrategy>();
        builder.Services.AddSingleton<IXmlSearchStrategy, LinqXmlSearchStrategy>();

        // VM + View
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}