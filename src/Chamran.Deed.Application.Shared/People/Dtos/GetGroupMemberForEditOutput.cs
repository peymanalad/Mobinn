using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.People.Dtos
{
    public class GetGroupMemberForEditOutput
    {
        public CreateOrEditGroupMemberDto GroupMember { get; set; }

        public string UserName { get; set; }

        public string OrganizationGroupGroupName { get; set; }

    }
}