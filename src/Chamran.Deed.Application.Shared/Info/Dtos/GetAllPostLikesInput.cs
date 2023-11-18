using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllPostLikesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int OrganizationId { get; set; }
        public int? UserId { get; set; }

        public DateTime? MaxLikeTimeFilter { get; set; }
        public DateTime? MinLikeTimeFilter { get; set; }

        public string PostPostTitleFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}