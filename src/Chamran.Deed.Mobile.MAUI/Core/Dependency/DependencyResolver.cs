using Abp.Dependency;

namespace Chamran.Deed.Core.Dependency
{
    public static class DependencyResolver
    {
        public static IIocManager IocManager => ApplicationBootstrapper.AbpBootstrapper.IocManager;

        public static T Resolve<T>()
        {
            try
            {
                return ApplicationBootstrapper.AbpBootstrapper.IocManager.Resolve<T>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static object Resolve(Type type)
        {
            try
            {
                return ApplicationBootstrapper.AbpBootstrapper.IocManager.Resolve(type);

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}