using System;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;

namespace Chamran.Deed.Web.IsWorkaround
{
    using Autofac.Core;
    using System.Collections.Generic;

    public class DummyAutofacContainer : Autofac.IContainer
    {
      
        // Implement other members of the IContainer interface as needed
        // You may leave them as empty methods for this dummy implementation
        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return null;

        }

        public IComponentRegistry ComponentRegistry { get; }
        public void Dispose()
        {
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            return null;
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return null;
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return null;
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return null;
        }

        public IDisposer Disposer { get; }
        public object Tag { get; }
        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;
    }

}
