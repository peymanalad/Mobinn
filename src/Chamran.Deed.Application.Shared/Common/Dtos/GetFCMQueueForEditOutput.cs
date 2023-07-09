using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Common.Dtos
{
    public class GetFCMQueueForEditOutput
    {
        public CreateOrEditFCMQueueDto FCMQueue { get; set; }

    }
}