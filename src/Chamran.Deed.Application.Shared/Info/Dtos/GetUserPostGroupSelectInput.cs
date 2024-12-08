using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetUserPostGroupSelectInput: PagedAndSortedResultRequestDto
    {
        public int OrganizationId { get; set; }
    } 
    public class GetAllowedUserPostGroupselectInput : PagedAndSortedResultRequestDto
    {
        public int OrganizationId { get; set; }
    }
}