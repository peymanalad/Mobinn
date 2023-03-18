using Chamran.Deed.Net.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmsBehestan
{
    using System = global::System;
    public partial class Client
    {
        private readonly DeedSmsSenderConfiguration _config;

        public Client(DeedSmsSenderConfiguration configuration)
        {
            _config = configuration;
            _httpClient = new HttpClient();
            _settings = new Lazy<JsonSerializerSettings>();
            _baseUrl = "https://payamak.isikato.ir/";
        }
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {
            var authenticationString = $"{_config.DeedSmsUsername}:{_config.DeedSmsPassword}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", base64EncodedAuthenticationString);

        }

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            var authenticationString = $"{_config.DeedSmsUsername}:{_config.DeedSmsPassword}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", base64EncodedAuthenticationString);
        }
    }

}
