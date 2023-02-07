using Abp.Modules;
using Chamran.Deed.Test.Base;

namespace Chamran.Deed.Tests
{
    [DependsOn(typeof(DeedTestBaseModule))]
    public class DeedTestModule : AbpModule
    {
       
    }
}
