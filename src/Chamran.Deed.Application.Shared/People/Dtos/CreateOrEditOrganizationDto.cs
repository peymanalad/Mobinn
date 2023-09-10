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

        public bool IsGovernmental { get; set; }

        [StringLength(OrganizationConsts.MaxNationalIdLength, MinimumLength = OrganizationConsts.MinNationalIdLength)]
        public string NationalId { get; set; }

        [StringLength(OrganizationConsts.MaxOrganizationLocationLength, MinimumLength = OrganizationConsts.MinOrganizationLocationLength)]
        public string OrganizationLocation { get; set; }

        [StringLength(OrganizationConsts.MaxOrganizationPhoneLength, MinimumLength = OrganizationConsts.MinOrganizationPhoneLength)]
        public string OrganizationPhone { get; set; }

        [StringLength(OrganizationConsts.MaxOrganizationContactPersonLength, MinimumLength = OrganizationConsts.MinOrganizationContactPersonLength)]
        public string OrganizationContactPerson { get; set; }

        [StringLength(OrganizationConsts.MaxCommentLength, MinimumLength = OrganizationConsts.MinCommentLength)]
        public string Comment { get; set; }

        public Guid? OrganizationLogo { get; set; }

        public string OrganizationLogoToken { get; set; }

    }
}