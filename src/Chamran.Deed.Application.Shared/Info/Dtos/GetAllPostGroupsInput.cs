using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllPostGroupsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string PostGroupDescriptionFilter { get; set; }

        public string OrganizationGroupGroupNameFilter { get; set; }

    }
}