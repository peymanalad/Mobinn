using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllCommentLikesForExcelInput
    {
        public string Filter { get; set; }

        public DateTime? MaxLikeTimeFilter { get; set; }
        public DateTime? MinLikeTimeFilter { get; set; }

        public string CommentCommentCaptionFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}