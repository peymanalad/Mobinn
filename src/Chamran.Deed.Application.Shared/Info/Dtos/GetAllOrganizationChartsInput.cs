using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllOrganizationChartsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string CaptionFilter { get; set; }

        public string LeafPathFilter { get; set; }

        public string OrganizationChartCaptionFilter { get; set; }

    }
}