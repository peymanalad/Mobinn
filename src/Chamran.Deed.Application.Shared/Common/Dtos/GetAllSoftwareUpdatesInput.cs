using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Common.Dtos
{
    public class GetAllSoftwareUpdatesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string SoftwareVersionFilter { get; set; }

        public int? ForceUpdateFilter { get; set; }

        public string UpdatePathFilter { get; set; }

        public string WhatsNewFilter { get; set; }

        public string PlatformFilter { get; set; }

        public int? MaxBuildNoFilter { get; set; }
        public int? MinBuildNoFilter { get; set; }

    }
}