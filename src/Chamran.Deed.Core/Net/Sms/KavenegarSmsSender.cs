using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Chamran.Deed.Net.Sms;
using System.Net.Http;


public class KavenegarSmsSender : ISmsSender
{
    private readonly ILogger<KavenegarSmsSender> _logger;


    public KavenegarSmsSender(ILogger<KavenegarSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string number, string message)
    {
        //return SendAsyncResult(number, message);
        return SendInternalAsync(number, message, false);
    }

    //public async Task<bool> SendAsyncResult(string number, string message)
    public Task<bool> SendAsyncResult(string number, string message)
    {
        return SendInternalAsync(number, message, true);
    }

    private async Task<bool> SendInternalAsync(string number, string message, bool asOtp)
    {
        try
        {
            //string token = message;
            string finalMessage = message;
            string appkey = "RVwFEFi4EJE";//APPKEY
            //var apiKey = Environment.GetEnvironmentVariable("KAVENEGAR_API_KEY");
            var apiKey = "6A596B434E3764674E57737079706F32306F34714F59417532734E416B4949575261636750646B654C70513D";
            var sender = "20005209";
            //var tag = "otp";
            //string appname = "سامانه مبین";
            //string otpTemplate = "« {appName} »\nکد ورود به سیستم:\n{otp}\n{apiKey}\nلغو11";
            //string finalMessage = otpTemplate
            //    .Replace("{appName}", appname)
            //    .Replace("{otp}", token)
            //    .Replace("{apiKey}", "RVwFEFi4EJE");
            var tag = asOtp ? "otp" : "sms";
            if (asOtp)
            {
                string appname = "سامانه مبین";
                string otpTemplate = "« {appName} »\nکد ورود به سیستم:\n{otp}\n{apiKey}\nلغو11";
                finalMessage = otpTemplate
                    .Replace("{appName}", appname)
                    .Replace("{otp}", message)
                    .Replace("{apiKey}", appkey);
            }

            var url = $"https://api.kavenegar.com/v1/{apiKey}/sms/send.json?receptor={number}&sender={sender}&message={finalMessage}&tag={tag}";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Kavenegar success: {Response}", content);
                return true;
            }
            else
            {
                _logger.LogError("Kavenegar failed: Status={StatusCode}, Body={Response}",
                    response.StatusCode, content);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending SMS to Kavenegar");
            return false;
        }
    }
}