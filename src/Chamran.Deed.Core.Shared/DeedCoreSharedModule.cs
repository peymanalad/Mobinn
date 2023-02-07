using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Chamran.Deed
{
    public class DeedCoreSharedModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedCoreSharedModule).GetAssembly());
        }
    }
}