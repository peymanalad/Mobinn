using Abp.Application.Services.Dto;

namespace Chamran.Deed.Common.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}