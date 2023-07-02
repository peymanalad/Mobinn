using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditSeenDto : EntityDto<int?>
    {

        public DateTime SeenTime { get; set; }

        public int PostId { get; set; }

        public long? UserId { get; set; }

    }
}