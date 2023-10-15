using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllDeedChartsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string CaptionFilter { get; set; }

        public string LeafPathFilter { get; set; }

        public string OrganizationOrganizationNameFilter { get; set; }

        public string DeedChartCaptionFilter { get; set; }

    }
}