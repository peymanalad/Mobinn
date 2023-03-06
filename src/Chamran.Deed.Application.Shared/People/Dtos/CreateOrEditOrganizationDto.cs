using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.People.Dtos
{
    public class CreateOrEditOrganizationDto : EntityDto<int?>
    {

        [Required]
        [StringLength(OrganizationConsts.MaxOrganizationNameLength, MinimumLength = OrganizationConsts.MinOrganizationNameLength)]
        public string OrganizationName { get; set; }

    }
}