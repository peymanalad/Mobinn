using Microsoft.Extensions.Configuration;
using System.Reflection;
using Chamran.Deed.Core;
using Syncfusion.Blazor;
using System.Net;
using Chamran.Deed.Mobile.MAUI.Services.Deed;

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
            // Set path to the SQLite database (it will be created if it does not exist)
            var dbPath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"deed.db");
            builder.Services.AddSingleton<GetExploreDataService>(
                s => ActivatorUtilities.CreateInstance<GetExploreDataService>(s, dbPath));
            builder.Services.AddSingleton<GetPostsDataService>(
                s => ActivatorUtilities.CreateInstance<GetPostsDataService>(s, dbPath));
            var app = builder.Build();
            return app;
        }
    }
}