using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Chamran.Deed.Startup
{
    [DependsOn(typeof(DeedCoreModule))]
    public class DeedGraphQLModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedGraphQLModule).GetAssembly());
        }

        public override void PreInitialize()
        {
            base.PreInitialize();

            //Adding custom AutoMapper configuration
            Configuration.Modules.AbpAutoMapper().Configurators.Add(CustomDtoMapper.CreateMappings);
        }
    }
}