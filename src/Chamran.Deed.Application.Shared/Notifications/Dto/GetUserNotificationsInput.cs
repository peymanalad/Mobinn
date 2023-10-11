using System;
using Abp.Notifications;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Notifications.Dto
{
    public class GetUserNotificationsInput : PagedInputDto
    {
        public UserNotificationState? State { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}