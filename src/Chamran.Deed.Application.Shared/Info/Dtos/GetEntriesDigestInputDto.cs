using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetEntriesDigestInputDto : PagedAndSortedResultRequestDto
    {
        public string CaptionFilter { get; set; }

    

    }
}