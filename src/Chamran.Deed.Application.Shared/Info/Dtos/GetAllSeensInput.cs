using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllSeensInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public DateTime? MaxSeenTimeFilter { get; set; }
        public DateTime? MinSeenTimeFilter { get; set; }

        public string PostPostTitleFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}