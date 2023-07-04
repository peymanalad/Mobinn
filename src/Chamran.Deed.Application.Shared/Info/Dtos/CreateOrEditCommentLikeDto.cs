using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditCommentLikeDto : EntityDto<int?>
    {

        public DateTime LikeTime { get; set; }

        public int CommentId { get; set; }

        public long UserId { get; set; }

    }
}