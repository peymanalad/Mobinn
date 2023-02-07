using Abp.Modules;
using Abp.Reflection.Extensions;

namespace Chamran.Deed
{
    [DependsOn(typeof(DeedXamarinSharedModule))]
    public class DeedXamarinIosModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedXamarinIosModule).GetAssembly());
        }
    }
}