using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetCommentsOfPostInput : PagedAndSortedResultRequestDto
    {
        public int PostId { get; set; }
    }
}