using Abp.Modules;
using Abp.Reflection.Extensions;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Chamran.Deed.Configure;
using Chamran.Deed.Startup;
using Chamran.Deed.Test.Base;

namespace Chamran.Deed.GraphQL.Tests
{
    [DependsOn(
        typeof(DeedGraphQLModule),
        typeof(DeedTestBaseModule))]
    public class DeedGraphQLTestModule : AbpModule
    {
        public override void PreInitialize()
        {
            IServiceCollection services = new ServiceCollection();
            
            services.AddAndConfigureGraphQL();

            WindsorRegistrationHelper.CreateServiceProvider(IocManager.IocContainer, services);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DeedGraphQLTestModule).GetAssembly());
        }
    }
}