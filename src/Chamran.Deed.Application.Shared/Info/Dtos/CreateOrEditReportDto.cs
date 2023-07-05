using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditReportDto : EntityDto<int?>
    {

        [Required]
        [StringLength(ReportConsts.MaxReportDescriptionLength, MinimumLength = ReportConsts.MinReportDescriptionLength)]
        public string ReportDescription { get; set; }

        public bool IsDashboard { get; set; }

        [Required]
        public string ReportContent { get; set; }

        public int? OrganizationId { get; set; }

    }
}