using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllTaskStatsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string CaptionFilter { get; set; }

        public short? MaxStatusFilter { get; set; }
        public short? MinStatusFilter { get; set; }

        public Guid? SharedTaskIdFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}