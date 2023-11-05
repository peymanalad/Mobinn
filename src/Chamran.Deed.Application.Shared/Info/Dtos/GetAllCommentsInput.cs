using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllCommentsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int OrganizationId { get; set; }
        public string CommentCaptionFilter { get; set; }

        public DateTime? MaxInsertDateFilter { get; set; }
        public DateTime? MinInsertDateFilter { get; set; }

        public string PostPostTitleFilter { get; set; }

        public string UserNameFilter { get; set; }

        public string CommentCommentCaptionFilter { get; set; }

    }
}