﻿using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.People.Dtos
{
    public class GetAllGroupMembersForExcelInput
    {
        public string Filter { get; set; }

        public string MemberPositionFilter { get; set; }

        public string UserNameFilter { get; set; }

        public string OrganizationGroupGroupNameFilter { get; set; }

    }
}