using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetDeedChartForEditOutput
    {
        public CreateOrEditDeedChartDto DeedChart { get; set; }

        public string OrganizationOrganizationName { get; set; }

        public string DeedChartCaption { get; set; }

    }
}