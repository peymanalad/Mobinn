using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chamran.Deed.Info
{
    public interface IPushNotificationService : IApplicationService
    {
        Task SendPushNotifications(List<string> deviceTokens, string title, string body);
    }
}
