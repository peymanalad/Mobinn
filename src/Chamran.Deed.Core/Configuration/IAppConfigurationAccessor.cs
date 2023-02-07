using Microsoft.Extensions.Configuration;

namespace Chamran.Deed.Configuration
{
    public interface IAppConfigurationAccessor
    {
        IConfigurationRoot Configuration { get; }
    }
}
