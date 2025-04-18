﻿using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllPostGroupsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int? OrganizationId { get; set; }
        public string PostGroupDescriptionFilter { get; set; }

        public int? MaxOrderingFilter { get; set; }
        public int? MinOrderingFilter { get; set; }

        public string OrganizationGroupGroupNameFilter { get; set; }

    }

    public class GetAllPostSubGroupsInput : PagedAndSortedResultRequestDto
    {
        public int PostGroupId { get; set; }
        public string Filter { get; set; }
        public string PostSubGroupDescriptionFilter { get; set; }



    }
}