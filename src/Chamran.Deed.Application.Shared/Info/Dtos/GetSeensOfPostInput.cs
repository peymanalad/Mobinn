using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetSeensOfPostInput : PagedAndSortedResultRequestDto
    {
        public int PostId { get; set; }

    }
}