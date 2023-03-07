using System.IO;
using Abp.Application.Services;
using Abp.Extensions;
using Chamran.Deed.ApiClient;

namespace Chamran.Deed
{
    public abstract class ProxyAppServiceBase : IApplicationService
    {
        public AbpApiClient ApiClient { get; set; }

        public const string ApiBaseUrl = "api/services/app/";

        private readonly string _serviceUrlSegment;

        protected ProxyAppServiceBase()
        {
            _serviceUrlSegment = GetServiceUrlSegmentByConvention();
        }

        protected string GetEndpoint(string methodName)
        {
            return ApiBaseUrl + _serviceUrlSegment + "/" + methodName;
        }

        protected string GetDirectEndpoint(string directPath)
        {
            return ApiBaseUrl + directPath;
        }


        private string GetServiceUrlSegmentByConvention()
        {
            return GetType()
                .Name
                .RemovePreFix("Proxy")
                .RemovePostFix("AppServiceProxy", "AppService");
        }
    }
}