using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class ReportDto : EntityDto
    {
        public string ReportDescription { get; set; }

        public bool IsDashboard { get; set; }

        public int? OrganizationId { get; set; }

    }
}