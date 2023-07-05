using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetReportForEditOutput
    {
        public CreateOrEditReportDto Report { get; set; }

        public string OrganizationOrganizationName { get; set; }

    }
}