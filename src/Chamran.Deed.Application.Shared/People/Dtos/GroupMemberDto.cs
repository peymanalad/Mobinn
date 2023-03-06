using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.People.Dtos
{
    public class GroupMemberDto : EntityDto
    {
        public string MemberPosition { get; set; }

        public long? UserId { get; set; }

        public int? OrganizationGroupId { get; set; }

    }
}