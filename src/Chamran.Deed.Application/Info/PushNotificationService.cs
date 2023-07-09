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

    
    public PushNotificationService(string firebaseJsonKeyPath, ILogger<PushNotificationService> logger)
    {
        _logger = logger;

        // Initialize Firebase Admin SDK
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromJson("")
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