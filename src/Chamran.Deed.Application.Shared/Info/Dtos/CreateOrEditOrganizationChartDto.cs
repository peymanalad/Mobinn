using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditOrganizationChartDto : EntityDto<int?>
    {
        [Required]
        public int OrganizationId { get; set; }

        [Required]
        [StringLength(OrganizationChartConsts.MaxCaptionLength, MinimumLength = OrganizationChartConsts.MinCaptionLength)]
        public string Caption { get; set; }

        [StringLength(OrganizationChartConsts.MaxLeafPathLength, MinimumLength = OrganizationChartConsts.MinLeafPathLength)]
        public string LeafPath { get; set; }

        public int? ParentId { get; set; }

    }

    public class CreateCompanyChartDto 
    {
        [Required]
        public int OrganizationId { get; set; }

        [Required]
        [StringLength(OrganizationChartConsts.MaxCaptionLength, MinimumLength = OrganizationChartConsts.MinCaptionLength)]
        public string Caption { get; set; }


    }
}