using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetOrganizationUserForEditOutput
    {
        public CreateOrEditOrganizationUserDto OrganizationUser { get; set; }

        public string UserName { get; set; }

        public string OrganizationChartCaption { get; set; }

    }
}