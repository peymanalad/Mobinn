using Abp.Application.Services.Dto;

namespace Chamran.Deed.People.Dtos
{
    public class GetAllNoOrganizationDto: PagedAndSortedResultRequestDto
    {
        public int OrganizationId { get; set; }
    }
}