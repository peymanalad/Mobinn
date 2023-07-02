using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllCommentsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string CommentCaptionFilter { get; set; }

        public DateTime? MaxCommentDateFilter { get; set; }
        public DateTime? MinCommentDateFilter { get; set; }

        public string PostPostTitleFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}