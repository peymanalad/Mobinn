using Microsoft.Extensions.Configuration;
using System.Reflection;
using Chamran.Deed.Core;
using Syncfusion.Blazor;
using System.Net;

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
            builder.Services.AddSyncfusionBlazor();
              Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzUyOEAzMjMwMkUzNDJFMzBQZDNEQ1hxUXE1OG9qUTdWMmQ4K05jU2c2MnozbnlNVzJJQ24yOVk3cGxZPQ==");
            ApplicationBootstrapper.InitializeIfNeeds<DeedMobileMAUIModule>();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var app = builder.Build();
            return app;
        }
    }
}