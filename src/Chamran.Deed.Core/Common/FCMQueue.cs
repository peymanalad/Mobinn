using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Common
{
    [Table("FCMQueues")]
    public class FCMQueue : CreationAuditedEntity
    {

        [Required]
        [StringLength(FCMQueueConsts.MaxDeviceTokenLength, MinimumLength = FCMQueueConsts.MinDeviceTokenLength)]
        public virtual string DeviceToken { get; set; }

        [Required]
        [StringLength(FCMQueueConsts.MaxPushTitleLength, MinimumLength = FCMQueueConsts.MinPushTitleLength)]
        public virtual string PushTitle { get; set; }

        [Required]
        [StringLength(FCMQueueConsts.MaxPushBodyLength, MinimumLength = FCMQueueConsts.MinPushBodyLength)]
        public virtual string PushBody { get; set; }

        public virtual bool IsSent { get; set; }

        public virtual DateTime SentTime { get; set; }

    }
}