using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Chamran.Deed.Web.IsWorkaround
{

    using Microsoft.Extensions.DependencyInjection;

    public class CustomServiceProviderIsService : IServiceProviderIsService
    {
        private readonly IContainer _container;

        public CustomServiceProviderIsService(IContainer container)
        {
            _container = container;
        }

        public bool IsService(Type serviceType)
        {
            // Check if the service type is registered in Castle Windsor
            bool isCastleService = _container.IsRegistered(serviceType);

            // For Autofac.IContainer, return true to indicate it's a service
            // This is just a dummy implementation to satisfy the dependency
            if (serviceType == typeof(Autofac.IContainer))
            {
                return true;
            }

            return isCastleService;
        }
    }
}
