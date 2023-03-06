using Abp.Application.Services.Dto;

namespace Chamran.Deed.People.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}