using Abp.Application.Services.Dto;
using Abp.Notifications;
using Abp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abp.Extensions;

namespace Chamran.Deed.Notifications.Dto
{
    [Serializable]
    public class UserNotificationDto : EntityDto<Guid>, IUserIdentifier
    {
        /// <summary>TenantId.</summary>
        public int? TenantId { get; set; }

        /// <summary>User Id.</summary>
        public long UserId { get; set; }

        /// <summary>Current state of the user notification.</summary>
        public UserNotificationState State { get; set; }

        /// <summary>The notification.</summary>
        public TenantNotificationDto Notification { get; set; }

        /// <summary>
        /// which realtime notifiers should handle this notification
        /// </summary>
        public string TargetNotifiers { get; set; }

        public List<string> TargetNotifiersList
        {
            get
            {
                if (this.TargetNotifiers.IsNullOrWhiteSpace())
                    return new List<string>();
                return ((IEnumerable<string>)this.TargetNotifiers.Split(',')).ToList<string>();
            }
        }
    }

}
