using Abp.Dependency;
using GraphQL.Types;
using GraphQL.Utilities;
using Chamran.Deed.Queries.Container;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Chamran.Deed.Schemas
{
    public class MainSchema : Schema, ITransientDependency
    {
        public MainSchema(IServiceProvider provider) :
            base(provider)
        {
            Query = provider.GetRequiredService<QueryContainer>();
        }
    }
}