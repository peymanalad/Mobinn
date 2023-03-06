using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.People.Dtos
{
    public class GetAllOrganizationsForExcelInput
    {
        public string Filter { get; set; }

        public string OrganizationNameFilter { get; set; }

    }
}