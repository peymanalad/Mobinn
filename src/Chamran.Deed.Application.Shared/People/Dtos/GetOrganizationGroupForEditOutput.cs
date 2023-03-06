using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.People.Dtos
{
    public class GetOrganizationGroupForEditOutput
    {
        public CreateOrEditOrganizationGroupDto OrganizationGroup { get; set; }

        public string OrganizationOrganizationName { get; set; }

    }
}