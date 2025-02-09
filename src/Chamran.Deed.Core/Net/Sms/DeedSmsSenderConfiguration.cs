using System;
using Abp.Dependency;
using Abp.Extensions;
using Microsoft.Extensions.Configuration;
using Chamran.Deed.Configuration;

namespace Chamran.Deed.Net.Sms
{
    public class DeedSmsSenderConfiguration : ITransientDependency
    {
        private readonly IConfigurationRoot _appConfiguration;

        public string DeedSmsUsername =>
            Environment.GetEnvironmentVariable("DEED_SMS_USERNAME")
            ?? _appConfiguration["DeedSmsSender:Username"];

        public string DeedSmsPassword =>
            Environment.GetEnvironmentVariable("DEED_SMS_PASSWORD")
            ?? _appConfiguration["DeedSmsSender:Password"];


        public DeedSmsSenderConfiguration(IAppConfigurationAccessor configurationAccessor)
        {
            _appConfiguration = configurationAccessor.Configuration;
        }
    }
}