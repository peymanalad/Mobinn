using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Common.Dtos
{
    public class GetAllFCMQueuesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string PushTitleFilter { get; set; }

        public int? IsSentFilter { get; set; }

        public DateTime? MaxSentTimeFilter { get; set; }
        public DateTime? MinSentTimeFilter { get; set; }

    }
}