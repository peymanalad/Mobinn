using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Chamran.Deed
{
    [DependsOn(typeof(DeedClientModule), typeof(AbpAutoMapperModule))]
    public class DeedXamarinSharedModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Localization.IsEnabled = false;
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedXamarinSharedModule).GetAssembly());
        }
    }
}