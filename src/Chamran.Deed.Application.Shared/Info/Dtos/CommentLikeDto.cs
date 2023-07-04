using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class CommentLikeDto : EntityDto
    {
        public DateTime LikeTime { get; set; }

        public int CommentId { get; set; }

        public long UserId { get; set; }

    }
}