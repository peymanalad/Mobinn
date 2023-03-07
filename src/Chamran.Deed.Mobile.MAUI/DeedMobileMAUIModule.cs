using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Chamran.Deed.ApiClient;
using Chamran.Deed.Info;
using Chamran.Deed.Mobile.MAUI.Core.ApiClient;
using System.ComponentModel;

namespace Chamran.Deed
{
    [DependsOn(typeof(DeedClientModule), typeof(AbpAutoMapperModule))]

    public class DeedMobileMAUIModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Localization.IsEnabled = false;
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;

            Configuration.ReplaceService<IApplicationContext, MAUIApplicationContext>();
        }

        public override void Initialize()
        {
            try
            {

                IocManager.RegisterAssemblyByConvention(typeof(DeedMobileMAUIModule).GetAssembly());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}