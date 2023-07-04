using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class PostLikeDto : EntityDto
    {
        public DateTime LikeTime { get; set; }

        public int PostId { get; set; }

        public long UserId { get; set; }

    }
}