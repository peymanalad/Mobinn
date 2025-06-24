using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Chamran.Deed.Net.Sms;
using Kavenegar;
using SmsBehestan;
using System.Collections.Generic;
using Kavenegar.Core.Exceptions;
using ApiException = Kavenegar.Core.Exceptions.ApiException;


public class KavenegarSmsSender : ISmsSender
{
    private readonly ILogger<KavenegarSmsSender> _logger;
    private readonly string _apiKey = "کلید API شما"; // TODO: از appsettings.json بخوانید
    private readonly string _templateName = "verify"; // نام تمپلیت ساخته‌شده در کاوه‌نگار

    public KavenegarSmsSender(ILogger<KavenegarSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string number, string message)
    {
        return SendAsyncResult(number, message);
    }

    public async Task<bool> SendAsyncResult(string number, string message)
    {
        try
        {
            string token = message;
            string appkey = "RVwFEFi4EJE";//APPKEY
            var apiKey = Environment.GetEnvironmentVariable("KAVENEGAR_API_KEY");
            string appname = "سامانه مبین";
            string otpTemplate = "« {appName} »\nکد ورود به سیستم:\n{otp}\n{apiKey}\nلغو11";
            string finalMessage = otpTemplate
                .Replace("{appName}",appname)
                .Replace("{otp}", token)
                .Replace("{apiKey}", "RVwFEFi4EJE");
            Kavenegar.KavenegarApi api = new Kavenegar.KavenegarApi(apiKey);
            var result = api.Send("2000660110", number, finalMessage);
            return true;
        }
        catch (ApiException ex)
        {
            Console.Write("Message : " + ex.Message);
        }
        catch (KavenegarException ex)
        {
            Console.Write("Message : " + ex.Message);
        }
        return false; 
    }
}