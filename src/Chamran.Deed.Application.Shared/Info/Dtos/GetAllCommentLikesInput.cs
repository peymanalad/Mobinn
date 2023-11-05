using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllCommentLikesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int OrganizationId { get; set; }
        public DateTime? MaxLikeTimeFilter { get; set; }
        public DateTime? MinLikeTimeFilter { get; set; }

        public string CommentCommentCaptionFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}