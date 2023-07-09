using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Common.Dtos
{
    public class FCMQueueDto : EntityDto
    {
        public string DeviceToken { get; set; }

        public string PushTitle { get; set; }

        public string PushBody { get; set; }

        public bool IsSent { get; set; }

        public DateTime SentTime { get; set; }

    }
}