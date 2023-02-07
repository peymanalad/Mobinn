using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Chamran.Deed
{
    [DependsOn(typeof(DeedCoreSharedModule))]
    public class DeedApplicationSharedModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedApplicationSharedModule).GetAssembly());
        }
    }
}