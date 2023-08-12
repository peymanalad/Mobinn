using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllOrganizationUsersForExcelInput
    {
        public string Filter { get; set; }

        public string UserNameFilter { get; set; }

        public string OrganizationChartCaptionFilter { get; set; }

    }
}