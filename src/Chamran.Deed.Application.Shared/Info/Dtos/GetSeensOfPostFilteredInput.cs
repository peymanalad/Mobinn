using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetSeensOfPostFilteredInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public int PostId { get; set; }

    }
}