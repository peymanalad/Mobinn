using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}