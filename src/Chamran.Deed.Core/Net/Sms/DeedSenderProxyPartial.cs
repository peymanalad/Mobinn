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
            //_baseUrl = "https://payamak.isikato.ir/";
            _baseUrl = "http://www.mspos.ir:50001";
        }


        async Task PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            //try
            //{
            //    CancellationToken cancellationToken = default;
            //    using var request_ = new HttpRequestMessage();
            //    var loginAuth = new IsikatoAuth()
            //    {
            //        captcha = "",
            //        password = _config.DeedSmsPassword,
            //        username = _config.DeedSmsUsername
            //    };

            //    var content_ =
            //        new StringContent(
            //            JsonConvert.SerializeObject(loginAuth, _settings.Value));
            //    content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            //    request_.Content = content_;
            //    request_.Method = new HttpMethod("POST");
            //    request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("*/*"));
            //    var urlBuilder_ = new StringBuilder();
            //    urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/services/auth/login");
            //    var url_ = urlBuilder_.ToString();
            //    request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);
            //    var response_ = await client.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            //    var result = await response_.Content.ReadAsStringAsync(cancellationToken);
            //    var token = JsonConvert.DeserializeObject<IsikatoToken>(result);
            //    //var authenticationString = $"{_config.DeedSmsUsername}:{_config.DeedSmsPassword}";
            //    //var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));
            //    //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            //    request.Headers.Add("Authorization", $"Bearer {token.access_token}");
            //}
            //catch (Exception)
            //{
            //    //ignored
            //}
            try
            {
                request.Headers.Add("Authorization", $"YasnaSystem");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
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
