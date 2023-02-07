using Microsoft.Extensions.Configuration;
using System.Reflection;
using Chamran.Deed.Core;

namespace Chamran.Deed.Mobile.MAUI
{
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
                    fonts.AddFont("IRANSans.ttf", "IRANSans");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif
            ApplicationBootstrapper.InitializeIfNeeds<DeedMobileMAUIModule>();

            var app = builder.Build();
            return app;
        }
    }
}