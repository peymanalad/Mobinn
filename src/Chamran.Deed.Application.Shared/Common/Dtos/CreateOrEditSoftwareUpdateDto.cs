using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Common.Dtos
{
    public class CreateOrEditSoftwareUpdateDto : EntityDto<int?>
    {

        [Required]
        [StringLength(SoftwareUpdateConsts.MaxSoftwareVersionLength, MinimumLength = SoftwareUpdateConsts.MinSoftwareVersionLength)]
        public string SoftwareVersion { get; set; }

        public bool ForceUpdate { get; set; }

        [StringLength(SoftwareUpdateConsts.MaxUpdatePathLength, MinimumLength = SoftwareUpdateConsts.MinUpdatePathLength)]
        public string UpdatePath { get; set; }

        [StringLength(SoftwareUpdateConsts.MaxWhatsNewLength, MinimumLength = SoftwareUpdateConsts.MinWhatsNewLength)]
        public string WhatsNew { get; set; }

        [StringLength(SoftwareUpdateConsts.MaxPlatformLength, MinimumLength = SoftwareUpdateConsts.MinPlatformLength)]
        public string Platform { get; set; }

        public int BuildNo { get; set; }

    }
}