using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Chamran.Deed
{
    public class DeedClientModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedClientModule).GetAssembly());
        }
    }
}
