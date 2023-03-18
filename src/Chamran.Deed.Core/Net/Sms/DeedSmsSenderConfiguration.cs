using Abp.Dependency;
using Abp.Extensions;
using Microsoft.Extensions.Configuration;
using Chamran.Deed.Configuration;

namespace Chamran.Deed.Net.Sms
{
    public class DeedSmsSenderConfiguration : ITransientDependency
    {
        private readonly IConfigurationRoot _appConfiguration;

        public string DeedSmsUsername => _appConfiguration["DeedSmsSender:Username"];

        public string DeedSmsPassword => _appConfiguration["DeedSmsSender:Password"];


        public DeedSmsSenderConfiguration(IAppConfigurationAccessor configurationAccessor)
        {
            _appConfiguration = configurationAccessor.Configuration;
        }
    }
}