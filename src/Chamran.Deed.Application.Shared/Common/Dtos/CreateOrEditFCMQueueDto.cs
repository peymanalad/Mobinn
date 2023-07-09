using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Common.Dtos
{
    public class CreateOrEditFCMQueueDto : EntityDto<int?>
    {

        [Required]
        [StringLength(FCMQueueConsts.MaxDeviceTokenLength, MinimumLength = FCMQueueConsts.MinDeviceTokenLength)]
        public string DeviceToken { get; set; }

        [Required]
        [StringLength(FCMQueueConsts.MaxPushTitleLength, MinimumLength = FCMQueueConsts.MinPushTitleLength)]
        public string PushTitle { get; set; }

        [Required]
        [StringLength(FCMQueueConsts.MaxPushBodyLength, MinimumLength = FCMQueueConsts.MinPushBodyLength)]
        public string PushBody { get; set; }

        public bool IsSent { get; set; }

        public DateTime SentTime { get; set; }

    }
}