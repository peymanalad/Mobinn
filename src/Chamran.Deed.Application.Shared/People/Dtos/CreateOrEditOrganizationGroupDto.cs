using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.People.Dtos
{
    public class CreateOrEditOrganizationGroupDto : EntityDto<int?>
    {

        [Required]
        [StringLength(OrganizationGroupConsts.MaxGroupNameLength, MinimumLength = OrganizationGroupConsts.MinGroupNameLength)]
        public string GroupName { get; set; }

        public int? OrganizationId { get; set; }

    }
}