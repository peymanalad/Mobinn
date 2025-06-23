using Chamran.Deed.Net.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Abp.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace SmsBehestan
{
    using System = System;

    public partial class Client
    {
        private readonly DeedSmsSenderConfiguration _config;

        public Client(DeedSmsSenderConfiguration configuration)
        {
            _config = configuration;
            _httpClient = new HttpClient();
            _settings = new Lazy<JsonSerializerSettings>();
            _baseUrl = "https://payamak.isikato.ir/";
            //_baseUrl = "http://www.mspos.ir:50001";
            //_baseUrl = "https://sms.mobinn.ir";
        }

        Task PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            var credentials = $"{_config.DeedSmsUsername}:{_config.DeedSmsPassword}";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
            return Task.CompletedTask;
        }


        public class IsikatoAuth
        {
            public string username { get; set; }
            public string password { get; set; }
            public string captcha { get; set; }
        }

        public class IsikatoToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public object refresh_token { get; set; }
            public int expires_in { get; set; }
            public object scope { get; set; }
        }

        public class IsikatoTunnelDto
        {
            public string phone_number { get; set; }
            public string token { get; set; }
            public string appkey { get; set; }
            public string appname { get; set; }
        }

    }
}
