using System;
using Abp.Extensions;

namespace Chamran.Deed.ApiClient
{
    public static class ApiUrlConfig
    {
#if DEBUG
        private const string DefaultHostUrl = "https://localhost:44301/";
#endif
#if !DEBUG
        private const string DefaultHostUrl = "https://api.ideed.ir/"; 
#endif

        public static string BaseUrl { get; private set; }

        static ApiUrlConfig()
        {
            ResetBaseUrl();
        }

        public static void ChangeBaseUrl(string baseUrl)
        {
            BaseUrl = ReplaceLocalhost(NormalizeUrl(baseUrl));
        }

        public static void ResetBaseUrl()
        {
            BaseUrl = ReplaceLocalhost(DefaultHostUrl);
        }

        public static bool IsLocal => DefaultHostUrl.Contains("localhost");

        private static string NormalizeUrl(string baseUrl)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != "http" && uriResult.Scheme != "https"))
            {
                throw new ArgumentException("Unexpected base URL: " + baseUrl);
            }

            return uriResult.ToString().EnsureEndsWith('/');
        }

        private static string ReplaceLocalhost(string url)
        {
            return url.Replace("localhost:44301", DebugServerIpAddresses.Current);
        }
    }
}