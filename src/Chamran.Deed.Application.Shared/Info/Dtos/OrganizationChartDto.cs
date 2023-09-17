using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class OrganizationChartDto : EntityDto
    {
        public string Caption { get; set; }

        public string LeafPath { get; set; }

        public int? ParentId { get; set; }
        public Guid? OrganizationLogo { get; set; }

    }
}