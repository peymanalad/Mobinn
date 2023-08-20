using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Chamran.Deed.Web.Helpers;

namespace Chamran.Deed.Web.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CurrentDirectoryHelpers.SetCurrentDirectory();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
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
                    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                })
                .UseIIS()
                .UseIISIntegration()
                .UseStartup<Startup>();
        }
    }
}
