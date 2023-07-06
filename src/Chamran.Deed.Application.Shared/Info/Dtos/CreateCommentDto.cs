using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateCommentDto : EntityDto<int?>
    {

        [Required]
        [StringLength(CommentConsts.MaxCommentCaptionLength, MinimumLength = CommentConsts.MinCommentCaptionLength)]
        public string CommentCaption { get; set; }

        public int PostId { get; set; }

        public int? CommentId { get; set; }

    }
}