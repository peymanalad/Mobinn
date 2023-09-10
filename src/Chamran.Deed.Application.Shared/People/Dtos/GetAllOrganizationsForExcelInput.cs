using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.People.Dtos
{
    public class GetAllOrganizationsForExcelInput
    {
        public string Filter { get; set; }

        public string OrganizationNameFilter { get; set; }

        public int? IsGovernmentalFilter { get; set; }

        public string NationalIdFilter { get; set; }

        public string OrganizationLocationFilter { get; set; }

        public string OrganizationPhoneFilter { get; set; }

        public string OrganizationContactPersonFilter { get; set; }

        public string CommentFilter { get; set; }

    }
}