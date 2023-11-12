using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.People.Dtos
{
    public class GetAllGroupMembersInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string MemberPositionFilter { get; set; }

        public string UserNameFilter { get; set; }

        public string OrganizationGroupGroupNameFilter { get; set; }

        public int? OrganizationId { get; set; }

    }
}