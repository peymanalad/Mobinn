using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Notifications;

namespace Chamran.Deed.Notifications.Dto
{
    public class GetNotificationsOutput : PagedResultDto<UserNotification>
    {
        public int UnreadCount { get; set; }

        public GetNotificationsOutput(int totalCount, int unreadCount, List<UserNotification> notifications)
            : base(totalCount, notifications)
        {
            UnreadCount = unreadCount;
        }
    }
    //public class GetNotificationsOutput : PagedResultDto<UserNotificationDto>
    //{
    //    public int UnreadCount { get; set; }

    //    public GetNotificationsOutput(int totalCount, int unreadCount, List<UserNotificationDto> notifications)
    //        : base(totalCount, notifications)
    //    {
    //        UnreadCount = unreadCount;
    //    }
    //}
}