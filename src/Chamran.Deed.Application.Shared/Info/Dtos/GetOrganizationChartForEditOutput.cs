using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetOrganizationChartForEditOutput
    {
        public CreateOrEditOrganizationChartDto OrganizationChart { get; set; }

        public string OrganizationChartCaption { get; set; }

    }
}