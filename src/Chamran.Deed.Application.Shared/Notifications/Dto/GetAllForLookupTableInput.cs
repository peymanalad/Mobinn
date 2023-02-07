using Abp.Application.Services.Dto;

namespace Chamran.Deed.Notifications.Dto
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}