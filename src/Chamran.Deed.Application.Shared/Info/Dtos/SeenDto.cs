using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class SeenDto : EntityDto
    {
        public DateTime SeenTime { get; set; }

        public int PostId { get; set; }

        public long? UserId { get; set; }

    }
}