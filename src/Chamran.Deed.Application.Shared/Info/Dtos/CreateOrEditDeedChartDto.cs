using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditDeedChartDto : EntityDto<int?>
    {

        [Required]
        [StringLength(DeedChartConsts.MaxCaptionLength, MinimumLength = DeedChartConsts.MinCaptionLength)]
        public string Caption { get; set; }

        [StringLength(DeedChartConsts.MaxLeafPathLength, MinimumLength = DeedChartConsts.MinLeafPathLength)]
        public string LeafPath { get; set; }

        public int? OrganizationId { get; set; }

        public int? ParentId { get; set; }

    }
}