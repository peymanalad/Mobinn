using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.People.Dtos
{
    public class OrganizationDto : EntityDto
    {
        public string OrganizationName { get; set; }
        public bool IsGovernmental { get; set; }
        
                public string NationalId { get; set; }
        
                public string OrganizationLocation { get; set; }
        
                public string OrganizationPhone { get; set; }
        
                public string OrganizationContactPerson { get; set; }
        
                public string Comment { get; set; }
        
                public Guid? OrganizationLogo { get; set; }
        
                public string OrganizationLogoFileName { get; set; }
    }
}