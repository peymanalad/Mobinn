using Abp.AutoMapper;
using Chamran.Deed.Organizations.Dto;

namespace Chamran.Deed.Models.Users
{
    [AutoMapFrom(typeof(OrganizationUnitDto))]
    public class OrganizationUnitModel : OrganizationUnitDto
    {
        public bool IsAssigned { get; set; }
    }
}