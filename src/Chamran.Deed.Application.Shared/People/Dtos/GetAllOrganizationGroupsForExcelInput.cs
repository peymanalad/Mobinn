using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.People.Dtos
{
    public class GetAllOrganizationGroupsForExcelInput
    {
        public string Filter { get; set; }

        public string GroupNameFilter { get; set; }

        public string OrganizationOrganizationNameFilter { get; set; }

    }
}