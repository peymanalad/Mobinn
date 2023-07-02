using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.People.Dtos
{
    public class CreateOrEditGroupMemberDto : EntityDto<int?>
    {
        [StringLength(GroupMemberConsts.MaxMemberPositionLength, MinimumLength = GroupMemberConsts.MinMemberPositionLength)]
        public string MemberPosition { get; set; }

        public int MemberPos { get; set; }


        public long? UserId { get; set; }

        public int? OrganizationGroupId { get; set; }

    }
}