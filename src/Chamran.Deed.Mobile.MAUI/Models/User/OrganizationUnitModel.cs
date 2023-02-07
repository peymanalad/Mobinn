using Abp.AutoMapper;
using Chamran.Deed.Organizations.Dto;

namespace Chamran.Deed.Mobile.MAUI.Models.User
{
    [AutoMapFrom(typeof(OrganizationUnitDto))]
    public class OrganizationUnitModel : OrganizationUnitDto
    {
        public bool IsAssigned { get; set; }
    }
}
