using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllPostGroupsForExcelInput
    {
        public string Filter { get; set; }

        public string PostGroupDescriptionFilter { get; set; }

        public int? MaxOrderingFilter { get; set; }
        public int? MinOrderingFilter { get; set; }

        public string OrganizationGroupGroupNameFilter { get; set; }

    }

    public class GetAllPostSubGroupsForExcelInput
    {
        public string Filter { get; set; }

        public string PostSubGroupDescriptionFilter { get; set; }


    }
}