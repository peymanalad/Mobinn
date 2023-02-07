using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Chamran.Deed.Authorization;

namespace Chamran.Deed
{
    /// <summary>
    /// Application layer module of the application.
    /// </summary>
    [DependsOn(
        typeof(DeedApplicationSharedModule),
        typeof(DeedCoreModule)
        )]
    public class DeedApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Adding authorization providers
            Configuration.Authorization.Providers.Add<AppAuthorizationProvider>();

            //Adding custom AutoMapper configuration
            Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedApplicationModule).GetAssembly());
        }
    }
}