using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Authorization;
using Chamran.Deed.Authorization;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace Chamran.Deed.Info;

[AbpAuthorize(AppPermissions.Pages_Administration)]
public class PushNotificationService : DeedAppServiceBase, IPushNotificationService
{
    private readonly ILogger<PushNotificationService> _logger;


    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;

        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromJson(@"{
  ""type"": ""service_account"",
  ""project_id"": ""ideed-994b0"",
  ""private_key_id"": ""a997ebd58f37bbfe1b75cf741f62cf453754d47c"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCi0OSf+sYtafGW\ndYo/O4I7ZGEnHTtzJ1yosjMQxZ3FPVL6dcxCbAdJPqVKHzn0rklvIzxjiK6BrxGY\nD61KmvVEBQElC6SMANbqGC67RNHJw2focLYkzXVaqMoIVkmoZDkdhO+j9yUInQ5f\njVLpBKrPS+iJ+LhProihrlRhcPl9s9kRZb6gOcHYE+4jhqc1wUEbtt/exV2V+IuW\npCckFLeEE82+hygDBviYGNUD1A2Dn5aXwt9FtC8Bl2OtfXiSisJSxX31c88ntXKz\nBGHlVmEONSTZWpK/5SJOxCXSUvLiXqlyg4avU4u/Tvj6x7qtdofsBxlqzJ2rAsAP\nppxiXZELAgMBAAECggEAB3B5CdBJDtko50GOpA4BiNow0Ds3NK9HPhQKXklRSKph\nKL2KLDzIZnHc1pKvsLzn8RKQl1Gu+5D2wlZxChgjfcymvT0Xcyg0qbSaKgdeeJQb\notTHlc3nf1F4DcJ3kiCmNXGWC41m3Upj1jzcXXZPobIruWYqccWdS5nuFBe35qTm\n7XzsBFK1mkVNWYYEb51HNwIj5MAJy2kHe5sYQQhmTyFjVgB8IU4JGjjAcQvCWCwc\ngviY/MX/5630W3Qn5p2uhM/e2TfKhXsxLpWi41sYEJvIpz5Nt2d/JeNaWLR77TXU\nC9ke4Xs/3E7x6PmN2WQY1kTYFNUgLgyZiLXWY4Du8QKBgQDMhnRVi9PBgGBrTCgR\n5v8qpo8/efFjgNiqNkP9+4wDZV893QQ0oxa9Pc4dUSfbeF3jOBEPcgQRjnDeM1qB\nR7C6aa02oUU+z8fxJmYNwc2blqCz8J+C6eLPP/iKeicUfVK6164bQDFqirJDjeK3\nxc5jriw4z4NyzoyFqOaXlXC58QKBgQDLyx28lSCMCPkW970jglVGAiPqheskEAKb\n8m3EAz9H+PHRi/OHoewt0sKEl5kOGewGHwMS3LVQjrun2hkatGzDWRimTP0hT7PU\nPL5Y12gp5o8TVoDumvXAwFuuJbQ20UxIvS+4aMDQEgxMG8R/VyP0Vf2Ob39IFXW1\nRE7+D3OeuwKBgGysmM4H0UnFM/Zvbe8tRbJc9EqvOeM8SGQRF4myCpCXYccWVDC/\n48pEN+v9/mawi3q7hN0nISBPBowaz2FYPYCfvEkF4ixg7YWmeJ2gt+aPX+6c0bUX\nH8wt2puCEpfhi376MWoLbvaEofohPzGU6niuyJKbOOZc9+/hcj1jS5WRAoGBAIJ8\nGRwoN+Cx1ht4oGfeCY97y1dcS8SZY7JKNTHuClSg9uR+wX00li545gdTsbIvssnw\njV1EZ+uemFTO9of8wi8KGxhP7zum9rOL+SyVL6K0dyOgnYkpOvUhtRH6HjuDI17n\nk2h12g7fn3QTy2rldqX5cRIEHfEfnHBlvlY5uoQFAoGAECkrx7KC2r/Sec50Ghkc\nTExNuER4qvcdnYhhU+hYJRi5lLyEPTiiwcVAxM4FJl5RDQHCIhRXyf6fi6rObwVI\nAMirszy9dYNLTTiikF7Qz4FarKmoAqZ7uw3kLsfwbtZhpjCQAaGRQhNpwcEF38ss\nKc4VCs+9sJbVI1uMPOFHqUI=\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""firebase-adminsdk-8m60q@ideed-994b0.iam.gserviceaccount.com"",
  ""client_id"": ""115342475745479403949"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-8m60q%40ideed-994b0.iam.gserviceaccount.com"",
  ""universe_domain"": ""googleapis.com""
}
")
        });
    }

    public async Task SendPushNotifications(List<string> deviceTokens, string title, string body)
    {
        var message = new MulticastMessage()
        {
            Tokens = deviceTokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

            if (response.FailureCount > 0)
            {
                // Handle failed notifications
                foreach (var error in response.Responses)
                {
                    _logger.LogError($"Failed to send notification to token: {error.Exception.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending push notification: {ex.Message}");
            throw;
        }
    }
}