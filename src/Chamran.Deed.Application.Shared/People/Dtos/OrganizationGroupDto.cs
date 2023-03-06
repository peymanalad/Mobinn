using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.People.Dtos
{
    public class OrganizationGroupDto : EntityDto
    {
        public string GroupName { get; set; }

        public int? OrganizationId { get; set; }

    }
}