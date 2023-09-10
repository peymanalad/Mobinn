using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.People.Dtos
{
    public class GetOrganizationForEditOutput
    {
        public CreateOrEditOrganizationDto Organization { get; set; }

        public string OrganizationLogoFileName { get; set; }

    }
}