using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Chamran.Deed.Web.Helpers;
using Sentry;
using System;

namespace Chamran.Deed.Web.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.TraversePath().Load();
            CurrentDirectoryHelpers.SetCurrentDirectory();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN") ?? string.Empty;

            var builder = new WebHostBuilder()
                .UseKestrel(opt =>
                {

                    opt.AddServerHeader = false;
                    opt.Limits.MaxRequestLineSize = 524288000;
                    opt.Limits.MaxRequestBufferSize = 524288000;

                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging((context, logging) =>
                {
                    //logging.AddFilter("Microsoft.EntityFrameworkCore.Database", LogLevel.Debug);
                    //logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                    if (!string.IsNullOrWhiteSpace(sentryDsn))
                    {
                        //    options.Dsn = sentryDsn;
                        //    options.MinimumEventLevel = LogLevel.Debug;
                        //    options.MinimumBreadcrumbLevel = LogLevel.Debug;
                        //});
                        logging.AddSentry(options =>
                        {
                            options.Dsn = sentryDsn;
                            options.MinimumEventLevel = LogLevel.Warning;
                            options.MinimumBreadcrumbLevel = LogLevel.Information;
                        });
                    }
                })
                .UseIIS();
            if (!string.IsNullOrWhiteSpace(sentryDsn))
            {
                builder = builder.UseSentry(o =>
                {
                    o.Dsn = sentryDsn;
                    o.Debug = false;
                    o.DiagnosticLevel = SentryLevel.Info;
                    o.TracesSampleRate = 1.0;
                    o.ProfilesSampleRate = 1.0;
                    o.AutoSessionTracking = false;
                    //})
                });
            }
            return builder


                .UseIISIntegration()
                .UseStartup<Startup>();
        }
    }
}