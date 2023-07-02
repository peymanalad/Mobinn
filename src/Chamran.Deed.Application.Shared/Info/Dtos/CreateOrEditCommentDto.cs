using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditCommentDto : EntityDto<int?>
    {

        [Required]
        [StringLength(CommentConsts.MaxCommentCaptionLength, MinimumLength = CommentConsts.MinCommentCaptionLength)]
        public string CommentCaption { get; set; }

        public DateTime CommentDate { get; set; }

        public int PostId { get; set; }

        public long UserId { get; set; }

    }
}