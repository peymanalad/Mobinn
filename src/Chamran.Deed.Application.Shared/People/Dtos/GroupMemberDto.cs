﻿using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.People.Dtos
{
    public class GroupMemberDto : EntityDto
    {
        public string MemberPosition { get; set; }

        public int MemberPos { get; set; }

        public long? UserId { get; set; }

        public string NationalId { get; set; }

        public int? OrganizationId { get; set; }

    }
}