using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllReportsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string ReportDescriptionFilter { get; set; }

        public int? IsDashboardFilter { get; set; }

        public string OrganizationOrganizationNameFilter { get; set; }

    }
}